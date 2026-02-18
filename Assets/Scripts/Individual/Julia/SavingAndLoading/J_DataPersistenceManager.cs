using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class J_DataPersistenceManager : MonoBehaviour
{
    [Header(" File Storage Config")]
    [SerializeField] private string _fileName;
    [SerializeField] private bool _useEncryption;

    private J_GameData _gameData;
    private List<J_IDataPersistence> _dataPersistenceObjects;
    private J_FileDataHandler _dataHandler;

    public static J_DataPersistenceManager instance { get; private set; }


    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene");
        }
        instance = this;
    }

    private void Start()
    {
        // Application.persistentDataPath will give the operating system standard directory for persistent data in a unity project
        _dataHandler = new J_FileDataHandler(Application.persistentDataPath, _fileName, _useEncryption);
        _dataPersistenceObjects = FindAllDataPersistenceObjects();

        // Load game data on startup
        LoadGame();
    }

    public void NewGame()
    {
        _gameData = new J_GameData();
    }

    public void ResetGame()
    {
        if (_gameData == null)
        {
            Debug.LogError("No data was found so this shouldn't be possible.");
            return;
        }

        _gameData.ResetData();
    }

    public void LoadGame()
    {
        // Load any saved data from a file using the data handler
        _gameData = _dataHandler.Load();

        // If no data can be loaded, initialise to a new game
        if (_gameData == null)
        {
            Debug.Log("No data was found. Creating new game data...");
            NewGame();
        }
        
        // Push the loaded data to all other scripts that require the data
        foreach (J_IDataPersistence dataPersistenceObj in _dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(_gameData);
        }

        Debug.Log("Loaded Quality Mode: " + _gameData.qualityMode.ToString());
    }

    public void SaveGame()
    {
        // Pass the data to other scripts to update
        foreach (J_IDataPersistence dataPersistenceObj in _dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref _gameData);
        }

        Debug.Log("Saved Quality Mode: " + _gameData.qualityMode.ToString());

        // Save the data to a file using the data handler
        _dataHandler.Save(_gameData);
    }

    // Precautionary save
    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<J_IDataPersistence> FindAllDataPersistenceObjects()
    {
        // Reminder, all those scripts must have monobehaviour to be found in this way
        IEnumerable<J_IDataPersistence> dataPersistenceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<J_IDataPersistence>();

        return new List<J_IDataPersistence>(dataPersistenceObjects);
    }
}
