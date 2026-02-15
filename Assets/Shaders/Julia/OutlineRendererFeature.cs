using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using Unity.Mathematics;

public class OutlineRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public Shader shader;
    }

    [SerializeField] private Settings _settings;
    private Material _material;
    private Pass _pass;

    public override void Create()
    {
        // Find or create shader
        if (_settings.shader == null)
            _settings.shader = Shader.Find("Hidden/Custom/OutlineShader");

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
        CoreUtils.Destroy(_material);
    }

    // The actual render pass
    private sealed class Pass : ScriptableRenderPass
    {
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
            cameraData.requiresDepthTexture = true;
            cameraData.requiresOpaqueTexture = true;

            // Volume lookup
            var outlineVolume = VolumeManager.instance.stack.GetComponent<OutlineVolume>();
            if (outlineVolume == null || !outlineVolume.IsActive())
                return;

            // Get thickness parameter
            float outlineThickness = outlineVolume.outlineThickness.value;
            Color outlineColour = outlineVolume.outlineColour.value;


            // Get resources
            var resources = frameData.Get<UniversalResourceData>();

            // Check if rendering to back buffer
            if (resources.isActiveTargetBackBuffer)
                return;

            // Get the screen texture
            TextureHandle target = resources.activeColorTexture;
            if (!target.IsValid())
                return;


            // Create temporary texture for processing
            var desc = renderGraph.GetTextureDesc(target);
            desc.name = "Outline_TempCopy";
            desc.clearBuffer = false;
            TextureHandle tempCopy = renderGraph.CreateTexture(desc);

            // Pass 1: Copy target -> tempCopy
            using (var b = renderGraph.AddRasterRenderPass<CopyPassData>("Outline Copy", out var pd))
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

            // Pass 2: Apply Outline tempCopy -> target
            using (var b = renderGraph.AddRasterRenderPass<OutlinePassData>("Outline Apply", out var pd))
            {
                b.UseTexture(tempCopy, AccessFlags.Read);
                b.SetRenderAttachment(target, 0);

                pd.source = tempCopy;
                pd.material = _mat;
                pd.outlineThickness = outlineThickness;
                pd.outlineColour = outlineColour;

                b.SetRenderFunc((OutlinePassData data, RasterGraphContext ctx) =>
                {
                    // Set shader parameter
                    data.material.SetFloat("_outlineThickness", data.outlineThickness);
                    data.material.SetColor("_outlineColour", data.outlineColour);

                    // Apply outline shader
                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.source,
                        new Vector4(1f, 1f, 0f, 0f),
                        data.material,
                        0
                    );
                });
            }
        }

        private class CopyPassData
        {
            public TextureHandle source;
        }

        private class OutlinePassData
        {
            public TextureHandle source;
            public Material material;
            public float outlineThickness;
            public Color outlineColour;
        }
    }
}
