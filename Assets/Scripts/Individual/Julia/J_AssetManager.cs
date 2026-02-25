using UnityEngine;

[System.Serializable]
public class UIWeaponSO {
    public Item weapon;
    public Sprite fullDurabilitySprite;
    public Sprite halfDurabilitySprite;
    public Sprite brokenDurabilitySprite;
    public Sprite swapSprite;
    public Sprite throwableSprite;
}

[CreateAssetMenu(fileName = "J_AssetManager", menuName = "Scriptable Objects/J_AssetManager")]
public class J_AssetManager : ScriptableObject
{
    public Sprite emptyPrimaryWeaponSprite;
    public Sprite emptySecondaryWeaponSprite;
    public Sprite emptyShieldWeaponSprite;
    public UIWeaponSO[] uiWeapons;

    public Sprite GetFullDurabilitySprite(Item weapon)
    {
        var uiWeapon = GetWeapon(weapon);
        if (uiWeapon == null)
            return null;

        Debug.Log("weapon not null");

        return uiWeapon.fullDurabilitySprite;
    }
    public Sprite GetHalfDurabilitySprite(Item weapon)
    {
        var uiWeapon = GetWeapon(weapon);
        if (uiWeapon == null)
            return null;

        return uiWeapon.halfDurabilitySprite;
    }

    public Sprite GetBrokenDurabilitySprite(Item weapon)
    {
        var uiWeapon = GetWeapon(weapon);
        if (uiWeapon == null)
            return null;

        return uiWeapon.brokenDurabilitySprite;
    }

    public Sprite GetSwapSprite(Item weapon)
    {
        var uiWeapon = GetWeapon(weapon);
        if (uiWeapon == null) 
            return null;

        return uiWeapon.swapSprite;
    }

    public Sprite GetThrowableSprite(Item weapon)
    {
        var uiWeapon = GetWeapon(weapon);
        if (uiWeapon == null)
            return null;

        return uiWeapon.swapSprite;
    }

    private UIWeaponSO GetWeapon(Item weapon)
    {
        if (weapon == null)
            Debug.Log("weapon is null!");

        for (int i = 0; i < uiWeapons.Length; i++) {
            if (uiWeapons[i].weapon == null)
                continue;

            if (weapon.itemName == uiWeapons[i].weapon.itemName)
                return uiWeapons[i];
        }

        return null;
    }
}
