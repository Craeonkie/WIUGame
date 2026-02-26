using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class J_MenuManager : MonoBehaviour, J_IDataPersistence
{
    [Header("Components")]
    [SerializeField] private AudioManager _audioManager;
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    public static System.Action OnEnterGame;
    public static System.Action OnNewGame;
    public static System.Action OnOpenSettings;

    public UnityEvent OnPause;
    public UnityEvent OnResume;
    public UnityEvent OnExit;

    public enum QUALITYMODE {
        LOW = 0,
        MEDIUM,
        HIGH
    }
    private QUALITYMODE _qualityMode;


    private void Awake()
    {
        // TODO: Load player prefs
        // TODO: Underline the text accordingly based on the quality
        UpdateAudioSliders();
    }

    private void OnEnable()
    {
        J_GameManager.OnPause += PauseGame;
    }

    private void OnDisable()
    {
        J_GameManager.OnPause -= PauseGame;
    }

    // Public functions that can have stuff added into them
    public void EnterGame()
    {
        OnEnterGame?.Invoke();
        // NOTE: I don't know if we should still handle time scale here or not
         Time.timeScale = 1f;

        Debug.Log("Enter Game was called!"); // comment when done
        
        // Load the game
        //J_DataPersistenceManager.instance.LoadGame();
        SceneLoader.Instance.LoadScene(J_GameManager.REST_SCENE);
    }

    public void NewGame()
    {
        OnNewGame?.Invoke();

        // might shift this, since enter game exists anyways so i can just call time scale in enter game
        // NOTE: I don't know if we should still handle time scale here or not
         Time.timeScale = 1f;

        Debug.Log("New Game was called!"); // comment when done

        // Reset the game
        J_DataPersistenceManager.instance.ResetGame();
        SceneLoader.Instance.LoadScene(J_GameManager.START_SCENE);

        // TODO: might want to go to a different start scene to play beginning cutscenes?
    }

    public void Settings()
    {
        OnOpenSettings?.Invoke();
    }

    //public void UpdateQualitySettings(TextMeshProUGUI text)
    //{
    //    // this is a REALLY lazy way of doing this but i dont know if update quality will be used elsewhere so
    //    // also this is probably really really inefficient
    //    //OnUpdateQuality?.Invoke();

    //    for (int i = 0; i < _qualityText.Length; i++)
    //    {
    //        if (_qualityText[i].text == text.text)
    //        {
    //            _qualityText[i].fontStyle = FontStyles.Underline | FontStyles.Bold;
    //            _qualityMode = (QUALITYMODE)i;
    //        }
    //        else
    //        {
    //            _qualityText[i].fontStyle = FontStyles.Bold;
    //        }

    //        OnUpdateQuality?.Invoke(_qualityText[i].text, _qualityText[i].fontStyle);
    //    }
    //}

    public void UpdateAudioSettings()
    {
        Debug.Log("scene manager: " + SceneManager.GetActiveScene() + " updated");

        _audioManager.masterVolume = _masterSlider.value;
        _audioManager.bgmVolume = _bgmSlider.value;
        _audioManager.sfxVolume = _sfxSlider.value;

        J_GameManager.Instance.UpdateAudio(_masterSlider.value, _bgmSlider.value, _sfxSlider.value);
    }

    public void UpdateAudioSliders()
    {
        _masterSlider.SetValueWithoutNotify(_audioManager.masterVolume);
        _bgmSlider.SetValueWithoutNotify(_audioManager.bgmVolume);
        _sfxSlider.SetValueWithoutNotify(_audioManager.sfxVolume);
    }

    //public void UpdateCameraSensitivitySlider(float cameraSens) => _cameraSensSlider.SetValueWithoutNotify(cameraSens);

    //public void UpdateCameraSensitivity()
    //{
    //    _cameraSens = _cameraSensSlider.value;
    //}

    public void PauseGame()
    {
        if (_pauseMenu.activeSelf)
            return;

        OnPause?.Invoke();
        Time.timeScale = 0f;

        Debug.Log("Pause Game was called!"); // comment when done
    }

    public void ResumeGame()
    {
        OnResume?.Invoke();
        Time.timeScale = 1f;
    }

    public void ExitGame()
    {
        OnExit?.Invoke();
        Time.timeScale = 1f;

        // Put here cause rn theres no leaving transition
        SceneLoader.Instance.LoadScene(J_GameManager.MENU_SCENE);

        // Save the game in its current state
        J_DataPersistenceManager.instance.SaveGame();
    }

    public void QuitGame()
    {
        Application.Quit(); // If we were making this for mobile, there's actually Application.Pause we need to consider cause Application.Quit doesn't always get called

        Debug.Log("Quit Game was called!"); // comment when done
    }

    public void LoadData(J_GameData data)
    {
        Debug.Log("Quit Game was called!"); // comment when done

        _audioManager.masterVolume = data.masterVolume;
        _audioManager.bgmVolume = data.bgmVolume;
        _audioManager.sfxVolume = data.sfxVolume;

        UpdateAudioSliders();

        //_cameraSens = data.cameraSensitivity;
        //UpdateCameraSensitivitySlider(data.cameraSensitivity);

        //_qualityMode = (QUALITYMODE)data.qualityMode;
        //UpdateQualitySettings(_qualityText[(int)_qualityMode]);
    }

    public void SaveData(ref J_GameData data)
    {
        data.masterVolume = _audioManager.masterVolume;
        data.bgmVolume = _audioManager.bgmVolume;
        data.sfxVolume = _audioManager.sfxVolume;

        //data.cameraSensitivity = _cameraSens;
        //data.qualityMode = (J_GameData.QUALITYMODE)_qualityMode;
    }
}