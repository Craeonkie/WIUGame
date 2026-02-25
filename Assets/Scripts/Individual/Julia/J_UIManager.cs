using UnityEngine;
using UnityEngine.UI;

public class J_UIManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject _combatUI;

    [Header("Weapon UI")]
    [SerializeField] private J_AssetManager _assetManager;
    [SerializeField] private Image _primaryWeaponIcon;
    [SerializeField] private Image _secondaryWeaponIcon;
    [SerializeField] private Image _shieldWeaponIcon;
    [SerializeField] private Image _shieldHealthbar;
    //[SerializeField] private Image _primarySpecialCooldown;
    //[SerializeField] private Image _secondarySpecialCooldown;
    //[SerializeField] private Image _shieldSpecialCooldown;
    [SerializeField] private Image _energyBar;
    [SerializeField] private float _fullDurabilityThreshold;
    [SerializeField] private float _halfDurabilityThreshold;
    [SerializeField] private float _brokenDurabilityThreshold;


    [Header("Boss UI")]
    [SerializeField] private Image _bossHealthbar;
    [SerializeField] private RectTransform _bossIcon;
    [SerializeField] private Vector2 _iconOffset;
    [SerializeField] private Sprite _dogBossIcon;
    [SerializeField] private Sprite _friendBossIcon;
    [SerializeField] private Sprite _monsterBossIcon;

    [Header("Player UI")]
    [SerializeField] private Image _playerHealthbar;

    private void OnEnable()
    {
        SceneLoader.OnSceneLoaded += UpdateBossIcon;
        Entity.OnHealthChanged += UpdateBossHealth;
        PlayerController.OnPlayerHealthChanged += UpdatePlayerHealth;
        Inventory.OnEquipPrimary += UpdatePrimaryWeapon;
        Inventory.OnEquipSecondary += UpdateSecondaryWeapon;
        Inventory.OnEquipShield += UpdateShieldIcon;
        Weapon.OnDurabilityChangeWeapon += UpdatePrimaryWeaponDurability;
        Weapon.OnDurabilityChangeShield += UpdateShieldHealth;
        PlayerController.OnEnergyChanged += UpdateEnergyBar;
    }

    private void OnDisable()
    {
        SceneLoader.OnSceneLoaded -= UpdateBossIcon;
        Entity.OnHealthChanged -= UpdateBossHealth;
        PlayerController.OnPlayerHealthChanged -= UpdatePlayerHealth;
        Inventory.OnEquipPrimary -= UpdatePrimaryWeapon;
        Inventory.OnEquipSecondary -= UpdateSecondaryWeapon;
        Inventory.OnEquipShield -= UpdateShieldIcon;
        Weapon.OnDurabilityChangeWeapon -= UpdatePrimaryWeaponDurability;
        Weapon.OnDurabilityChangeShield -= UpdateShieldHealth;
        PlayerController.OnEnergyChanged -= UpdateEnergyBar;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdatePlayerHealth(float currentHealth, float maxHealth)
    {
        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        _playerHealthbar.fillAmount = healthPercentage;
    }

    private void UpdateShieldHealth(float currentShieldHealth, float maxShieldHealth)
    {
        float healthPercentage = Mathf.Clamp01(currentShieldHealth / maxShieldHealth);
        _shieldHealthbar.fillAmount = healthPercentage;
    }

    private void UpdatePrimaryWeapon(Item primaryWeapon)
    {
        if (primaryWeapon == null)
        {
            _primaryWeaponIcon.sprite = _assetManager.emptyPrimaryWeaponSprite;
        }
        else
        {
            UpdatePrimaryWeaponDurability(primaryWeapon);
        }
    }

    private void UpdatePrimaryWeaponDurability(Item primaryWeapon)
    {
        if (!primaryWeapon.hasDurability)
        {
            _primaryWeaponIcon.sprite = _assetManager.GetFullDurabilitySprite(primaryWeapon);
            return;
        }

        // Get durability
        float durabilityPercentage = primaryWeapon.currentDurability / primaryWeapon.maxDurability;
        if (durabilityPercentage > _halfDurabilityThreshold)
            _primaryWeaponIcon.sprite = _assetManager.GetFullDurabilitySprite(primaryWeapon);
        else if (durabilityPercentage > _halfDurabilityThreshold)
            _primaryWeaponIcon.sprite = _assetManager.GetHalfDurabilitySprite(primaryWeapon);
        else
            _primaryWeaponIcon.sprite = _assetManager.GetBrokenDurabilitySprite(primaryWeapon);
    }

    private void UpdateSecondaryWeapon(Item secondaryWeapon)
    {
        Debug.Log("secondary wewapon: " + secondaryWeapon);

        if (secondaryWeapon == null)
        {
            _secondaryWeaponIcon.sprite = _assetManager.emptySecondaryWeaponSprite;
        }
        else
        {
            UpdateSecondaryWeaponDurability(secondaryWeapon);
        }
    }

    private void UpdateSecondaryWeaponDurability(Item secondaryWeapon)
    {
        if (!secondaryWeapon.hasDurability)
        {
            _secondaryWeaponIcon.sprite = _assetManager.GetFullDurabilitySprite(secondaryWeapon);
            return;
        }

        // Get durability
        float durabilityPercentage = secondaryWeapon.currentDurability / secondaryWeapon.maxDurability;
        if (durabilityPercentage > _halfDurabilityThreshold)
            _secondaryWeaponIcon.sprite = _assetManager.GetFullDurabilitySprite(secondaryWeapon);
        else if (durabilityPercentage > _halfDurabilityThreshold)
            _secondaryWeaponIcon.sprite = _assetManager.GetHalfDurabilitySprite(secondaryWeapon);
        else
            _secondaryWeaponIcon.sprite = _assetManager.GetBrokenDurabilitySprite(secondaryWeapon);
    }

    private void UpdateShieldIcon(Item shield)
    {
        if (shield == null)
        {
            _shieldWeaponIcon.sprite = _assetManager.emptyShieldWeaponSprite;
            UpdateShieldHealth(0f, 1f);
        }
        else
        {
            // Get durability
            _shieldWeaponIcon.sprite = _assetManager.GetFullDurabilitySprite(shield);
            UpdateShieldHealth(shield.currentDurability, shield.maxDurability);
        }
    }

    private void UpdateEnergyBar(float currentEnergy, float maxEnergy)
    {
        float energyPercentage = currentEnergy / maxEnergy;
        _energyBar.fillAmount = energyPercentage;
    }

    private void UpdateBossIcon(string sceneName)
    {
        if (sceneName == J_GameManager.DOG_SCENE)
        {
            _bossIcon.GetComponent<Image>().sprite = _dogBossIcon;
        }
        else if (sceneName == J_GameManager.KID_SCENE)
        {
            _bossIcon.GetComponent<Image>().sprite = _friendBossIcon;
        }
        else if (sceneName == J_GameManager.MONSTER_SCENE)
        {
            _bossIcon.GetComponent<Image>().sprite = _monsterBossIcon;
        }
        else
        {
            // Default to dog boss icon
            _bossIcon.GetComponent<Image>().sprite = null;
        }
    }

    private void UpdateBossHealth(float currentHealth, float maxHealth)
    {
        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        _bossHealthbar.fillAmount = healthPercentage;

        float xOffset = (_iconOffset.y - _iconOffset.x) * healthPercentage;
        _bossIcon.anchoredPosition = new Vector2(_iconOffset.x + xOffset, 0f);
    }
}
