using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
[VolumeComponentMenu("Custom/DepthOfField")]
public class DepthOfFieldVolume : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter focusDistance = new ClampedFloatParameter(10f, 0f, 100f);
    public ClampedFloatParameter focusRange = new ClampedFloatParameter(3f, 0.1f, 10f);
    public ClampedFloatParameter bokehRadius = new ClampedFloatParameter(4f, 1f, 10f);

    public bool IsActive() => focusDistance.value > 0f;
    public bool IsTileCompatible() => true;
}
