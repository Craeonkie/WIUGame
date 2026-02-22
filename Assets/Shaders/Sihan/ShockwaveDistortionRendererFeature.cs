using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class ShockwaveDistortionRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shockwaveDistortion;
    }

    public Settings settings = new Settings();
    private Material _material;
    private Pass _pass;

    public override void Create()
    {
        if (settings.shockwaveDistortion == null)
            settings.shockwaveDistortion = Shader.Find("Custom/ShockwaveDistortion");

        if (settings.shockwaveDistortion == null) return;

        _material = CoreUtils.CreateEngineMaterial(settings.shockwaveDistortion);
        _pass = new Pass(_material)
        {
            renderPassEvent = settings.passEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_material == null) return;
        _pass.renderPassEvent = settings.passEvent;
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
        CoreUtils.Destroy(_material);
    }

    class Pass : ScriptableRenderPass
    {
        private Material _mat;
        public Pass(Material mat) => _mat = mat;

        private class PassData { public TextureHandle source; }

        public void Dispose() { }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();
            var vol = VolumeManager.instance.stack.GetComponent<ShockwaveDistortionVolume>();

            if (vol == null || !vol.IsActive()) return;

            TextureHandle activeColor = resourceData.activeColorTexture;
            if (!activeColor.IsValid()) return;

            var desc = renderGraph.GetTextureDesc(activeColor);
            desc.name = "Shockwave Copy";
            desc.depthBufferBits = 0;
            TextureHandle copy = renderGraph.CreateTexture(desc);

            // Pass 1: Copy the scene
            using (var b = renderGraph.AddRasterRenderPass<PassData>("Shockwave Copy", out var pd))
            {
                b.UseTexture(activeColor, AccessFlags.Read);
                b.SetRenderAttachment(copy, 0);
                pd.source = activeColor;
                b.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                    Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false));
            }

            // Pass 2: Apply the Shockwave Zoom Blur
            using (var b = renderGraph.AddRasterRenderPass<PassData>("Shockwave Apply", out var pd))
            {
                b.UseTexture(copy, AccessFlags.Read);
                b.SetRenderAttachment(activeColor, 0);
                pd.source = copy;

                b.SetRenderFunc((PassData data, RasterGraphContext ctx) => {
                    _mat.SetFloat("_Intensity", vol.intensity.value);
                    _mat.SetFloat("_Samples", vol.samples.value);
                    Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), _mat, 0);
                });
            }
        }
    }
}