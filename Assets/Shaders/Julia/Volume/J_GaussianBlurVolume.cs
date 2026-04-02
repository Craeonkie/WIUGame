using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
[VolumeComponentMenu("Custom/Gaussian Blur")]
public class GaussianBlurVolume : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter Intensity = new ClampedFloatParameter(0f, 0f, 1f);
    public ClampedIntParameter Radius = new ClampedIntParameter(3, 0, 16);

    // 1 -> Full Res, 2 -> Half, 4 -> Quarter
    public ClampedIntParameter DownSample = new ClampedIntParameter(2, 1, 4);

    public bool IsActive() => Intensity.value > 0f && Radius.value > 0;
    public bool IsTileCompatible() => true;
}
