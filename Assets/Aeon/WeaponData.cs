using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;
    public string weaponDescription;
    public float maxDurability;

    [Header("(Ensure Animation clips are named the same as their states!)")]
    [Header("Primary")]
    public Attack[] primaryAttack;

    [Header("Secondary")]
    public Attack[] secondaryAttack;

    [Header("Special(May become obsolete)")]
    public Attack[] specialAttack;
}

[System.Serializable]
public struct Attack
{
    [Header("Damage weapon does")]
    public float damage;
    [Header("Attack Animation")]
    public AnimationClip animationClip;
    [Header("Durability used by the attack")]
    public float durabilityUsage;
}
