using UnityEngine;
using UnityEngine.UI;

public class J_UIManager : MonoBehaviour
{
    [Header("Combat UI")]
    [SerializeField] private GameObject _combatUI;
    [SerializeField] private Image _playerHealthbar;
    [SerializeField] private Image _bossHealthbar;
    [SerializeField] private RectTransform _bossIcon;
    [SerializeField] private Vector2 _iconOffset;
    [SerializeField] private Sprite _dogBossIcon;
    [SerializeField] private Sprite _friendBossIcon;
    [SerializeField] private Sprite _monsterBossIcon;

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
