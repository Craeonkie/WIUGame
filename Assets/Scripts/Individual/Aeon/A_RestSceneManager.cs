using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class A_RestSceneManager : MonoBehaviour
{
    [SerializeField] private DoorToAnotherScene _kidRoomDoor;
    [SerializeField] private DoorToAnotherScene _kitchenDoor;
    [SerializeField] private DoorToAnotherScene _parentsRoomDoor;
    [SerializeField] private RestingPlayerController _player;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _kidsDoorSpawn;
    [SerializeField] private Transform _kitchenSpawn;
    [SerializeField] private Transform _parentsDoorSpawn;

    [Header("Do these things when the corresponding scene is completed")]
    public UnityEvent noScenesCompleted;
    public UnityEvent kidsRoomSceneCompleted;
    public UnityEvent kitchenSceneCompleted;
    public UnityEvent parentsRoomSceneCompleted;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateDoorsInScene();
    }

    // Run this function to update the doors in the scene
    public void UpdateDoorsInScene()
    {
        // Make only kids room accessible
        _kidRoomDoor.ToggleSceneEnterable(true, false);
        _kitchenDoor.ToggleSceneEnterable(false, false);
        _parentsRoomDoor.ToggleSceneEnterable(false, false);
        _player.transform.position = _spawnPoint.position;
        _player.transform.rotation = _spawnPoint.rotation;

        // Check if kids room is completed
        if (J_GameManager.Instance.IsStageCompleted(_kidRoomDoor.ReturnNextSceneName()))
        {
            Debug.Log("Kid's room completed");
            _kidRoomDoor.ToggleSceneEnterable(false, true);
            kidsRoomSceneCompleted?.Invoke();
            _player.transform.position = _kidsDoorSpawn.position;
            _player.transform.rotation = _kidsDoorSpawn.rotation;

            // Check if kitchen is completed
            if (J_GameManager.Instance.IsStageCompleted(_kitchenDoor.ReturnNextSceneName()))
            {
                Debug.Log("Kitchen completed");
                _kitchenDoor.ToggleSceneEnterable(false, true);
                kitchenSceneCompleted?.Invoke();
                _player.transform.position = _kitchenSpawn.position;
                _player.transform.rotation = _kitchenSpawn.rotation;

                // Check if parents room is completed (Shouldn't run)
                if (J_GameManager.Instance.IsStageCompleted(_parentsRoomDoor.ReturnNextSceneName()))
                {
                    Debug.Log("Parents room completed");
                    _parentsRoomDoor.ToggleSceneEnterable(false, false);
                    parentsRoomSceneCompleted?.Invoke();
                    _player.transform.position = _parentsDoorSpawn.position;
                    _player.transform.rotation = _parentsDoorSpawn.rotation;
                }
                else
                {
                    _parentsRoomDoor.ToggleSceneEnterable(true, false);
                }
            }
            else
            {
                _kitchenDoor.ToggleSceneEnterable(true, false);
            }
        }
        else
        {
            noScenesCompleted?.Invoke();
        }
    }
}
