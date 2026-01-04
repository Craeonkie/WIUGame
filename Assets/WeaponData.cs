using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public float maxDurability;

    [Header("Primary")]
    public float primaryDamage;
    public float primaryDuration;
    public float primaryDurabilityUsage;

    [Header("Secondary")]
    public float secondaryDamage;
    public float secondaryDuration;
    public float secondaryDurabilityUsage;

    [Header("Special")]
    public float specialDamage;
    public float specialDuration;
}
