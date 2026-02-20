using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class J_DepthOfFieldRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public Shader shader;
        public bool onlyBaseCamera = true;
    }

    [SerializeField] private Settings _settings;
    private Material _material;
    private Pass _pass;

    public override void Create()
    {
        // Find or create shader
        if (_settings.shader == null)
            _settings.shader = Shader.Find("Hidden/Custom/DepthOfField");

        if (_settings.shader != null)
            _material = CoreUtils.CreateEngineMaterial(_settings.shader);

        // Create the render pass
        _pass = new Pass(_material)
        {
            renderPassEvent = _settings.renderPassEvent
        };
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_material == null)
            return;

        // Add pass to rendering queue
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
        CoreUtils.Destroy(_material);
    }

    // The actual render pass
    private sealed class Pass : ScriptableRenderPass
    {
        private static readonly int FocusDistanceID = Shader.PropertyToID("_focusDistance");
        private static readonly int FocusRangeID = Shader.PropertyToID("_focusRange");
        private static readonly int BokehRadiusID = Shader.PropertyToID("_bokehRadius");
        private static readonly int DepthTextureID = Shader.PropertyToID("_CameraDepthTexture");
        private static readonly int DOFTextureID = Shader.PropertyToID("_DoFTexture");
        private static readonly int COCTextureID = Shader.PropertyToID("_CoCTexture");
        private static readonly int cocPass = 0;
        private static readonly int preFilterPass = 1;
        private static readonly int bokehPass = 2;
        private static readonly int postFilterPass = 3;
        private static readonly int combinePass = 4;
        private readonly Material _mat;

        public Pass(Material mat)
        {
            _mat = mat;
        }

        public void Dispose() { }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_mat == null)
                return;

            // Get camera data
            var cameraData = frameData.Get<UniversalCameraData>();

            // Volume lookup
            var depthOfFieldVolume = VolumeManager.instance.stack.GetComponent<J_DepthOfFieldVolume>();
            if (depthOfFieldVolume == null || !depthOfFieldVolume.IsActive())
                return;

            // Get parameters
            float focusDistance = depthOfFieldVolume.focusDistance.value;
            float focusRange = depthOfFieldVolume.focusRange.value;
            float bokehRadius = depthOfFieldVolume.bokehRadius.value;

            // Get resources
            var resources = frameData.Get<UniversalResourceData>();

            // Check if rendering to back buffer
            if (resources.isActiveTargetBackBuffer)
                return;

            // Get the depth texture
            TextureHandle depthTexture = resources.cameraDepthTexture;

            // Get the screen texture
            TextureHandle target = resources.activeColorTexture;
            if (!target.IsValid())
                return;


            // Create temporary texture for processing
            var desc = renderGraph.GetTextureDesc(target);
            desc.clearBuffer = false;
            
            desc.name = "DepthOfField_TempCopy";
            TextureHandle tempCopy = renderGraph.CreateTexture(desc);

            desc.name = "DepthOfField_COC";
            TextureHandle dofCOC = renderGraph.CreateTexture(desc);

            desc.name = "DepthOfField_PreFilter";
            desc.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat;
            TextureHandle dofPreFilter = renderGraph.CreateTexture(desc);

            desc.name = "DepthOfField_Bokeh";
            TextureHandle dofBokeh = renderGraph.CreateTexture(desc);

            desc.name = "DepthOfField_PostFilter";
            TextureHandle dofPostFilter = renderGraph.CreateTexture(desc);


            // Pass 1: Copy target -> tempCopy
            using (var b = renderGraph.AddRasterRenderPass<CopyPassData>("DepthOfField Copy", out var pd))
            {
                b.UseTexture(target, AccessFlags.Read);
                b.SetRenderAttachment(tempCopy, 0);

                pd.source = target;

                b.SetRenderFunc((CopyPassData data, RasterGraphContext ctx) =>
                {
                    // Copy blit (no material)
                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.source,
                        new Vector4(1f, 1f, 0f, 0f),
                        0,
                        false
                    );
                });
            }

            // Pass 2: Apply COC, tempCopy -> dofCOC
            using (var b = renderGraph.AddRasterRenderPass<DepthOfFieldPassData>("DepthOfField COC", out var pd))
            {
                b.UseTexture(tempCopy, AccessFlags.Read);
                b.UseTexture(depthTexture, AccessFlags.Read);
                b.SetRenderAttachment(dofCOC, 0);

                pd.source = tempCopy;
                pd.material = _mat;
                pd.cameraDepthTexture = depthTexture;
                pd.focusDistance = focusDistance;
                pd.focusRange = focusRange;

                b.SetRenderFunc((DepthOfFieldPassData data, RasterGraphContext ctx) =>
                {
                    // Set shader parameter
                    data.material.SetFloat(FocusDistanceID, data.focusDistance);
                    data.material.SetFloat(FocusRangeID, data.focusRange);

                    if (data.cameraDepthTexture.IsValid())
                        data.material.SetTexture(DepthTextureID, data.cameraDepthTexture);

                    // Apply DOF shader
                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.source,
                        new Vector4(1f, 1f, 0f, 0f),
                        data.material,
                        cocPass
                    );
                });
            }

            // Pass 3: Apply PreFilter dofCOC -> dofPreFilter (Downsampling)
            using (var b = renderGraph.AddRasterRenderPass<DepthOfFieldPassData>("DepthOfField PreFilter", out var pd))
            {
                b.UseTexture(tempCopy, AccessFlags.Read);
                b.UseTexture(dofCOC, AccessFlags.Read);
                b.SetRenderAttachment(dofPreFilter, 0);

                pd.source = tempCopy;
                pd.cocTexture = dofCOC;
                pd.material = _mat;
                pd.bokehRadius = bokehRadius;

                b.SetRenderFunc((DepthOfFieldPassData data, RasterGraphContext ctx) =>
                {
                    data.material.SetTexture(COCTextureID, pd.cocTexture);
                    data.material.SetFloat(BokehRadiusID, pd.bokehRadius);

                    // Apply DOF shader
                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.source,
                        new Vector4(1f, 1f, 0f, 0f),
                        data.material,
                        preFilterPass
                    );
                });
            }

            // Pass 4: Apply Bokeh dofCOC -> dofBokeh
            using (var b = renderGraph.AddRasterRenderPass<DepthOfFieldPassData>("DepthOfField Bokeh", out var pd))
            {
                b.UseTexture(dofPreFilter, AccessFlags.Read);
                b.SetRenderAttachment(dofBokeh, 0);

                pd.source = dofPreFilter;
                pd.material = _mat;

                b.SetRenderFunc((DepthOfFieldPassData data, RasterGraphContext ctx) =>
                {
                    // Apply DOF shader
                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.source,
                        new Vector4(1f, 1f, 0f, 0f),
                        data.material,
                        bokehPass
                    );
                });
            }

            // Pass 5: Apply PostFilter dofBokeh -> dofPostFilter
            using (var b = renderGraph.AddRasterRenderPass<DepthOfFieldPassData>("DepthOfField PostFilter", out var pd))
            {
                b.UseTexture(dofBokeh, AccessFlags.Read);
                b.SetRenderAttachment(dofPostFilter, 0);

                pd.source = dofBokeh;
                pd.material = _mat;

                b.SetRenderFunc((DepthOfFieldPassData data, RasterGraphContext ctx) =>
                {
                    // Apply DOF shader
                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.source,
                        new Vector4(1f, 1f, 0f, 0f),
                        data.material,
                        postFilterPass
                    );
                });
            }

            // Pass 6: Combine postFilter with original source texture
            using (var b = renderGraph.AddRasterRenderPass<DepthOfFieldPassData>("DepthOfField Combine", out var pd))
            {
                b.UseTexture(tempCopy, AccessFlags.Read);
                b.UseTexture(dofCOC, AccessFlags.Read);
                b.UseTexture(dofPostFilter, AccessFlags.Read);
                b.SetRenderAttachment(target, 0);

                pd.source = tempCopy;
                pd.cocTexture = dofCOC;
                pd.dofTexture = dofPostFilter;
                pd.material = _mat;

                b.SetRenderFunc((DepthOfFieldPassData data, RasterGraphContext ctx) =>
                {
                    data.material.SetTexture(COCTextureID, pd.cocTexture);
                    data.material.SetTexture(DOFTextureID, pd.dofTexture);

                    // Apply DOF shader
                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.source,
                        new Vector4(1f, 1f, 0f, 0f),
                        data.material,
                        combinePass
                    );
                });
            }
        }

        private class CopyPassData
        {
            public TextureHandle source;
        }

        private class DepthOfFieldPassData
        {
            public TextureHandle source;
            public Material material;
            public TextureHandle cameraDepthTexture;
            public TextureHandle cocTexture;
            public TextureHandle dofTexture;
            public float focusDistance;
            public float focusRange;
            public float bokehRadius;
        }
    }
}
