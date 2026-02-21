using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Level
{
    public string keyName;
    public Dictionary<int, bool> visitedPhases;

    //[SerializeField] Cutscene _cutScenes; // then have a isvisied before in case

    public Level(string name, int numOfPhases)
    {
        keyName = name;
        visitedPhases = new Dictionary<int, bool>();
        for (int i = 0; i < numOfPhases; ++i)
        {
            visitedPhases.Add(i, false);
        }
    }
}

public class J_GameManager : MonoBehaviour, J_IDataPersistence
{
    public static J_GameManager Instance { get; private set; }
    public const string MENU_SCENE = "J_MenuScene";
    public const string REST_SCENE = "J_RestScene";
    public const string DOG_SCENE = "S_DogScene";
    public const string KID_SCENE = "C_TestScene";
    public const string MONSTER_SCENE = "J_MonsterScene";
    
    // shoul i make ths serislaizeifle.d..
    // then u can reference and shtia nd like idk add in ur own scene phase number u duration whtaefer
    
    private string _currentScene;

    //private Dictionary<Level, bool> _visitedScenes = new Dictionary<Level, bool>();
    private Dictionary<string, bool> _completedStages = new Dictionary<string, bool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Initialise dictionary
        //_visitedScenes.Add(new Level(MENU_SCENE, 0), false);
        //_visitedScenes.Add(new Level(DOG_SCENE, 2), false);
        //_visitedScenes.Add(new Level(KID_SCENE, 2), false);
        //_visitedScenes.Add(new Level(MONSTER_SCENE, 3), false);
        //_visitedScenes.Add(new Level(REST_SCENE, 0), false);

        _completedStages.Add(MENU_SCENE, false);
        _completedStages.Add(DOG_SCENE, false);
        _completedStages.Add(KID_SCENE, false);
        _completedStages.Add(MONSTER_SCENE, false);
        _completedStages.Add(REST_SCENE, false);
    }

    public void SetCurrentScene(string scene)
    {
        // Find the key
        if (!_completedStages.ContainsKey(scene))
            return;

        // Visit current scene
        _completedStages[scene] = true;
    }

    public bool IsSceneVisited(string scene)
    {
        if (!_completedStages.ContainsKey(scene))
            return false;

        return _completedStages[scene];
    }

    //public void UpdateScenePhase(string sceneName, int phaseNumber)
    //{

    //}

    public void LoadData(J_GameData data)
    {
        _currentScene = data.currentStage;
        _completedStages[MENU_SCENE] = data.completedStages[MENU_SCENE];
        _completedStages[DOG_SCENE] = data.completedStages[DOG_SCENE];
        _completedStages[KID_SCENE] = data.completedStages[KID_SCENE];
        _completedStages[MONSTER_SCENE] = data.completedStages[MONSTER_SCENE];
        _completedStages[REST_SCENE] = data.completedStages[REST_SCENE];
    }

    public void SaveData(ref J_GameData data)
    {
        data.currentStage = _currentScene;
        data.completedStages[MENU_SCENE] = _completedStages[MENU_SCENE];
        data.completedStages[DOG_SCENE] = _completedStages[DOG_SCENE];
        data.completedStages[KID_SCENE] = _completedStages[KID_SCENE];
        data.completedStages[MONSTER_SCENE] = _completedStages[MONSTER_SCENE];
        data.completedStages[REST_SCENE] = _completedStages[REST_SCENE];
    }
}
