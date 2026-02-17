using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class SceneLoader : MonoBehaviour
{
    public GarbageManager garbageManager;
    //public bool readyToLoad = false;
    //private AsyncOperation asyncOperation;
    public UnityEvent onSceneLoaded;

    public void Start()
    {
        //asyncOperation = SceneManager.LoadSceneAsync("LoadingScreen");
        //asyncOperation.allowSceneActivation = false;

        //if (AudioLibrary.Instance != null)
        //{
        //    AudioLibrary.Instance.StopAllSounds();
        //}

        // Set menu open to false immediately
        garbageManager.isMenuOpen = false;
        onSceneLoaded.Invoke();
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