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
    public const string START_SCENE = "StartScene";
    public const string REST_SCENE = "A_RestScene";
    public const string DOG_SCENE = "S_DogScene";
    public const string KID_SCENE = "C_FriendScene";
    public const string MONSTER_SCENE = "J_MonsterScene";
    public const string END_SCENE = "EndScene";

    private int _currentPhase;
    private string _currentScene;
    private float _currentGameTime;

    private string _nextStage;
    private Dictionary<string, bool> _completedStages = new Dictionary<string, bool>();
    private Vector3 _previousPosition; // Use this to spawn player back in a room or something

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
        if (_completedStages.Count == 0)
        {
            _completedStages.Add(MENU_SCENE, false);
            _completedStages.Add(DOG_SCENE, false);
            _completedStages.Add(KID_SCENE, false);
            _completedStages.Add(MONSTER_SCENE, false);
            _completedStages.Add(REST_SCENE, false);
            _currentPhase = 0;
        }
    }

    private void Update()
    {
        _currentGameTime += Time.deltaTime;
    }

    public float GetGameTime() => _currentGameTime;

    public void SetCurrentScene(string scene)
    {
        // Find the key
        if (!_completedStages.ContainsKey(scene))
            return;

        // Visit current scene
        _completedStages[scene] = true;
        UpdateNextScene();
    }

    public void SetCurrentPhase(int phase)
    {
        _currentPhase = phase;
    }

    public string GetNextStage()
    {
        return _nextStage;
    }

    public int GetCurrentPhase()
    {
        return _currentPhase;
    }

    public bool IsStageCompleted(string scene)
    {

        if (!_completedStages.ContainsKey(scene))
            return false;

        Debug.Log("scene: " + scene + " is " + _completedStages[scene]);
        return _completedStages[scene];
    }

    private void UpdateNextScene()
    {
        if (!_completedStages[DOG_SCENE])
            _nextStage = DOG_SCENE;
        else if (!_completedStages[KID_SCENE])
            _nextStage = KID_SCENE;
        else if (!_completedStages[MONSTER_SCENE])
            _nextStage = MONSTER_SCENE;
        else
        {
            Debug.Log("All scenes were completed");
        }
    }

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
