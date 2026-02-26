using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class VignetteRendererFeature : ScriptableRendererFeature
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
            _settings.shader = Shader.Find("Hidden/Custom/VignetteShader");

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

            // Volume lookup
            var vignetteVolume = VolumeManager.instance.stack.GetComponent<VignetteVolume>();
            if (vignetteVolume == null || !vignetteVolume.IsActive())
                return;

            // Get parameters
            float radius = vignetteVolume.radius.value;
            float feather = vignetteVolume.feather.value;
            Color tintColour = vignetteVolume.tintColour.value;
            bool invert = vignetteVolume.invert.value;


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
            desc.name = "Vignette_TempCopy";
            desc.clearBuffer = false;
            TextureHandle tempCopy = renderGraph.CreateTexture(desc);

            // Pass 1: Copy target -> tempCopy
            using (var b = renderGraph.AddRasterRenderPass<CopyPassData>("Vignette Copy", out var pd))
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

            // Pass 2: Apply vignette tempCopy -> target
            using (var b = renderGraph.AddRasterRenderPass<VignettePassData>("Vignette Apply", out var pd))
            {
                b.UseTexture(tempCopy, AccessFlags.Read);
                b.SetRenderAttachment(target, 0);

                pd.source = tempCopy;
                pd.material = _mat;
                pd.radius = radius;
                pd.feather = feather;
                pd.tintColour = tintColour;
                pd.invert = invert;

                b.SetRenderFunc((VignettePassData data, RasterGraphContext ctx) =>
                {
                    // Set shader parameter
                    data.material.SetFloat("_radius", data.radius);
                    data.material.SetFloat("_feather", data.feather);
                    data.material.SetColor("_tintColour", data.tintColour);
                    data.material.SetFloat("_invertColour", data.invert ? 1 : 0);

                    // Apply vignette shader
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

        private class VignettePassData
        {
            public TextureHandle source;
            public Material material;
            public float radius;
            public float feather;
            public Color tintColour;
            public bool invert;
        }
    }
}
