using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public GameManagerData garbageManager;
    public static System.Action<string> OnSceneLoaded;
    public UnityEvent sceneLoadEvent;
    //public bool readyToLoad = false;
    //private AsyncOperation asyncOperation;

    public void Start()
    {
        //asyncOperation = SceneManager.LoadSceneAsync("LoadingScreen");
        //asyncOperation.allowSceneActivation = false;

        //if (AudioLibrary.Instance != null)
        //{
        //    AudioLibrary.Instance.StopAllSounds();
        //}

        // Set menu open to false immediately
        //garbageManager.isMenuOpen = false;
        OnSceneLoaded?.Invoke(SceneManager.GetActiveScene().name);
        sceneLoadEvent?.Invoke();
    }

    private void Update()
    {
        //if (readyToLoad)
        //{
        //    if (asyncOperation != null)
        //    {
        //        asyncOperation.allowSceneActivation = true;
        //        readyToLoad = false;
        //    }
        //}
    }

    //public void SetNextScene(string nextSceneName)
    //{
    //    garbageManager.nextSceneName = nextSceneName;
    //}

    //public void LoadLoadingScene()
    //{
    //    readyToLoad = true;
    //}

    public void LoadScene(string nextSceneName)
    {
        Debug.Log("here");
        SceneManager.LoadScene(nextSceneName);
    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}