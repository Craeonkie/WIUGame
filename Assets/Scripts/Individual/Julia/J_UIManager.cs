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
    }

    private void OnDisable()
    {
        SceneLoader.OnSceneLoaded -= UpdateBossIcon;
        Entity.OnHealthChanged -= UpdateBossHealth;
        PlayerController.OnPlayerHealthChanged -= UpdatePlayerHealth;
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
            // Get durability
            float durabilityPercentage = primaryWeapon.currentDurability / primaryWeapon.maxDurability;
            if (durabilityPercentage > _halfDurabilityThreshold)
                _primaryWeaponIcon.sprite = _assetManager.GetFullDurabilitySprite(primaryWeapon);
            else if (durabilityPercentage > _halfDurabilityThreshold)
                _primaryWeaponIcon.sprite = _assetManager.GetHalfDurabilitySprite(primaryWeapon);
            else
                _primaryWeaponIcon.sprite = _assetManager.GetBrokenDurabilitySprite(primaryWeapon);
        }
    }

    private void UpdateSecondaryWeapon(Item secondaryWeapon)
    {

    }

    private void UpdateShield(Item shield)
    {

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
