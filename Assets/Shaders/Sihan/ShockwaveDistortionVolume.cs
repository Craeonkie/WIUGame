using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("Custom/ShockwaveDistortion")]
public class ShockwaveDistortionVolume : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    public MinIntParameter samples = new MinIntParameter(2, 2);

    public bool IsActive() => intensity.value > 0f;
    public bool IsTileCompatible() => false;
}