using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
[VolumeComponentMenu("Custom/Bloom")]
public class BloomVolume : VolumeComponent, IPostProcessComponent
{
    public FloatParameter Threshold = new FloatParameter(1f);
    public FloatParameter Intensity = new FloatParameter(0f);
    public FloatParameter Exposure = new FloatParameter(1f);
    public ColorParameter Tint = new ColorParameter(Color.white);

    // 1 -> Full Res, 2 -> Half, 4 -> Quarter
    public ClampedIntParameter DownSample = new ClampedIntParameter(2, 1, 4);

    public bool IsActive() => Intensity.value > 0f;
    public bool IsTileCompatible() => true;
}
