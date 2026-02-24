using UnityEngine;

[System.Serializable]
public struct UIWeaponSO {
    public Item weapon;
    public Sprite fullDurabilitySprite;
    public Sprite halfDurabilitySprite;
    public Sprite brokenDurabilitySprite;
}

[CreateAssetMenu(fileName = "J_AssetManager", menuName = "Scriptable Objects/J_AssetManager")]
public class J_AssetManager : ScriptableObject
{
    public UIWeaponSO uiWeapons;
}
