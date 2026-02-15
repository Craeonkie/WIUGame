using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
[VolumeComponentMenu("Custom/Outlines")]
public class OutlineVolume : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter outlineThickness = new ClampedFloatParameter(1f, 0f, 3f);
    public ColorParameter outlineColour = new ColorParameter(Color.black);

    public bool IsActive() => outlineThickness.value > 0f;
    public bool IsTileCompatible() => true;
}
