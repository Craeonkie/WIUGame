using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class J_BloomRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader bloomShader;
        public Shader blurShader;
        public bool onlyBaseCamera = true;
    }

    [SerializeField] private Settings _settings = new Settings();

    private Material _materialExtracted;
    private Material _materialH;
    private Material _materialV;
    private Material _materialFinal;
    private Pass _pass;

    public override void Create()
    {
        if (_settings.bloomShader == null)
            _settings.bloomShader = Shader.Find("Hidden/Custom/Bloom");

        if (_settings.blurShader == null)
            _settings.blurShader = Shader.Find("Hidden/Custom/GaussianBlur");

        if (_settings.bloomShader == null || _settings.blurShader == null)
            return;

        // Separate materials
        _materialExtracted = CoreUtils.CreateEngineMaterial(_settings.bloomShader);
        _materialH = CoreUtils.CreateEngineMaterial(_settings.blurShader);
        _materialV = CoreUtils.CreateEngineMaterial(_settings.blurShader);
        _materialFinal = CoreUtils.CreateEngineMaterial(_settings.bloomShader);

        _pass = new Pass(_materialExtracted, _materialH, _materialV, _materialFinal, _settings.onlyBaseCamera)
        {
            renderPassEvent = _settings.passEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_materialH == null || _materialV == null)
            return;

        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
        CoreUtils.Destroy(_materialExtracted);
        CoreUtils.Destroy(_materialH);
        CoreUtils.Destroy(_materialV);
        CoreUtils.Destroy(_materialFinal);
    }

    private sealed class Pass : ScriptableRenderPass
    {
        private static readonly int ThresholdID = Shader.PropertyToID("_threshold");
        private static readonly int IntensityID = Shader.PropertyToID("_intensity");
        private static readonly int ScatterID = Shader.PropertyToID("_scatter");
        private static readonly int DirectionID = Shader.PropertyToID("_direction");
        private static readonly int TexelSizeID = Shader.PropertyToID("_texelSize");
        private static readonly int RadiusID = Shader.PropertyToID("_radius");
        private static readonly int BloomTexID = Shader.PropertyToID("_BloomTex");
        private static readonly int ExposureID = Shader.PropertyToID("_exposure");

        private readonly Material _matExtracted;
        private readonly Material _matH;
        private readonly Material _matV;
        private readonly Material _matFinal;
        private readonly bool _onlyBaseCamera;

        public Pass(Material matExtracted, Material matH, Material matV, Material matFinal, bool onlyBaseCamera)
        {
            _matExtracted = matExtracted;
            _matH = matH;
            _matV = matV;
            _matFinal = matFinal;
            _onlyBaseCamera = onlyBaseCamera;
        }

        public void Dispose() { }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_matExtracted == null || _matFinal == null || _matH == null || _matV == null)
                return;

            var cameraData = frameData.Get<UniversalCameraData>();
            // If it is preview camera in the editor, skip
            if (cameraData.isPreviewCamera)
                return;

            // Only apply to base camera if specified
            if (_onlyBaseCamera && cameraData.renderType != CameraRenderType.Base)
                return;

            // Check if volume is active
            var vol = VolumeManager.instance.stack.GetComponent<BloomVolume>();
            if (vol == null || !vol.IsActive())
                return;

            float threshold = vol.Threshold.value;
            float intensity = vol.Intensity.value;
            float radius = vol.Intensity.value;
            float exposure = vol.Exposure.value;

            int downSample = Mathf.Max(1, vol.DownSample.value);

            // Get the current active colour texture
            var resources = frameData.Get<UniversalResourceData>();
            if (resources.isActiveTargetBackBuffer)
                return;


            // Source texture
            TextureHandle target = resources.activeColorTexture;
            if (!target.IsValid())
                return;

            // Create temporary textures
            var upsampleDesc = renderGraph.GetTextureDesc(target);
            upsampleDesc.clearBuffer = false;

            // Temporary copy
            upsampleDesc.name = "Bloom_Temp";
            TextureHandle brightTex = renderGraph.CreateTexture(upsampleDesc);

            // Upsampled after blurring bright areas
            upsampleDesc.name = "Bloom_Upsample";
            TextureHandle upTex = renderGraph.CreateTexture(upsampleDesc);


            // Downsampled
            var downDesc = renderGraph.GetTextureDesc(target);
            downDesc.clearBuffer = false;
            downDesc.width = Mathf.Max(1, downDesc.width / downSample);
            downDesc.height = Mathf.Max(1, downDesc.height / downSample);

            // Texture to be down sampled
            downDesc.name = "GaussianBlur_Downsample";
            TextureHandle downTex = renderGraph.CreateTexture(downDesc);

            // Texture to be blurred horizontally
            downDesc.name = "GaussianBlur_TempH";
            TextureHandle tempH = renderGraph.CreateTexture(downDesc);

            // Texture to be blurred vertically
            downDesc.name = "GaussianBlur_TempV";
            TextureHandle tempV = renderGraph.CreateTexture(downDesc);

            Vector2 texelSize = new Vector2(1f / downDesc.width, 1f / downDesc.height);

            // PASS 0: Copy to temp (source -> brightTex)
            using (var b = renderGraph.AddRasterRenderPass<BloomPassData>("Bloom Extraction", out var pd))
            {
                b.UseTexture(target, AccessFlags.Read);
                b.SetRenderAttachment(brightTex, 0);

                pd.source = target;
                pd.material = _matExtracted;
                pd.threshold = threshold;

                b.SetRenderFunc((BloomPassData data, RasterGraphContext ctx) =>
                {
                    data.material.SetFloat(ThresholdID, data.threshold);
                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.source,
                        new Vector4(1f, 1f, 0f, 0f),
                        data.material,
                        0
                    );
                });
            }

            // PASS 1: Downsample (brightTex -> downTex)
            using (var b = renderGraph.AddRasterRenderPass<DownPassData>("Blur Downsample", out var pd))
            {
                b.UseTexture(brightTex, AccessFlags.Read);
                b.SetRenderAttachment(downTex, 0);
                
                pd.source = brightTex;

                b.SetRenderFunc((DownPassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.source,
                        new Vector4(1f, 1f, 0f, 0f),
                        0,
                        false
                    );
                });
            }

            // PASS 2: Blur Horizontally (downTex -> tempH)
            using (var b = renderGraph.AddRasterRenderPass<BlurPassData>("Blur Horizontal", out var pd))
            {
                b.UseTexture(downTex, AccessFlags.Read);
                b.SetRenderAttachment(tempH, 0);

                pd.source = downTex;
                pd.material = _matH;
                pd.intensity = intensity;
                pd.radius = radius;
                pd.direction = new Vector2(1f, 0f);
                pd.texelSize = texelSize;

                b.SetRenderFunc((BlurPassData data, RasterGraphContext ctx) =>
                {
                    data.material.SetFloat(IntensityID, data.intensity);
                    data.material.SetFloat(RadiusID, data.radius);
                    data.material.SetVector(DirectionID, new Vector4(data.direction.x, data.direction.y, 0f, 0f));
                    data.material.SetVector(TexelSizeID, new Vector4(data.texelSize.x, data.texelSize.y, 0f, 0f));

                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.source,
                        new Vector4(1f, 1f, 0f, 0f),
                        data.material,
                        0
                    );
                });
            }

            // PASS 3: Vertical (tempH -> tempV)
            using (var b = renderGraph.AddRasterRenderPass<BlurPassData>("Blur Vertical", out var pd))
            {
                b.UseTexture(tempH, AccessFlags.Read);
                b.SetRenderAttachment(tempV, 0);

                pd.source = tempH;
                pd.material = _matV;
                pd.intensity = intensity;
                pd.radius = radius;
                pd.direction = new Vector2(0f, 1f);
                pd.texelSize = texelSize;

                b.SetRenderFunc((BlurPassData data, RasterGraphContext ctx) =>
                {
                    data.material.SetFloat(IntensityID, data.intensity);
                    data.material.SetFloat(RadiusID, data.radius);
                    data.material.SetVector(DirectionID, new Vector4(data.direction.x, data.direction.y, 0f, 0f));
                    data.material.SetVector(TexelSizeID, new Vector4(data.texelSize.x, data.texelSize.y, 0f, 0f));

                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.source,
                        new Vector4(1f, 1f, 0f, 0f),
                        data.material,
                        0
                    );
                });
            }

            // PASS 4: Combine (tempV + target -> outputTex)
            using (var b = renderGraph.AddRasterRenderPass<BloomPassData>("Bloom Final Combine", out var pd))
            {
                b.UseTexture(target, AccessFlags.Read);
                b.UseTexture(tempV, AccessFlags.Read);

                b.SetRenderAttachment(upTex, 0);

                pd.source = target;
                pd.bloomSource = tempV;
                pd.material = _matFinal;
                pd.exposure = exposure;

                b.SetRenderFunc((BloomPassData data, RasterGraphContext ctx) =>
                {
                    data.material.SetTexture(BloomTexID, data.bloomSource);
                    data.material.SetFloat(ExposureID, data.exposure);

                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.source,
                        new Vector4(1, 1, 0, 0),
                        data.material,
                        1
                    );
                });
            }

            resources.cameraColor = upTex;
        }
    }


    private class CopyPassData
    {
        public TextureHandle source;
    }

    private class DownPassData
    {
        public TextureHandle source;
    }

    private class BloomPassData
    {
        public TextureHandle source;
        public TextureHandle bloomSource;
        public Material material;
        public float threshold;
        public float exposure;
    }

    private class BlurPassData
    {
        public TextureHandle source;
        public Material material;
        public float intensity;
        public float radius;
        public Vector2 direction;
        public Vector2 texelSize;
    }
}
