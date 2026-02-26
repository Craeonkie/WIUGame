using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
[VolumeComponentMenu("Custom/Vignette")]

public class VignetteVolume : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter radius = new ClampedFloatParameter(1f, -2f, 2f);
    public ClampedFloatParameter feather = new ClampedFloatParameter(1f, 0f, 3f);
    public ColorParameter tintColour = new ColorParameter(Color.black);
    public BoolParameter invert = new BoolParameter(false);

    public bool IsActive() => radius.value < 2f;
    public bool IsTileCompatible() => true;
}
