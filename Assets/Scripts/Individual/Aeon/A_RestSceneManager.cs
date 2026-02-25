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
    [SerializeField] private Vector3 _spawnPoint;
    [SerializeField] private Vector3 _kidsDoorSpawn;
    [SerializeField] private Vector3 _kitchenSpawn;
    [SerializeField] private Vector3 _parentsDoorSpawn;

    [Header("Do these things when the corresponding scene is completed")]
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
        _player.MovePlayerTo(_spawnPoint);

        // Check if kids room is completed
        if (J_GameManager.Instance.IsStageCompleted(_kidRoomDoor.ReturnNextSceneName()))
        {
            _kidRoomDoor.ToggleSceneEnterable(false, true);
            kidsRoomSceneCompleted?.Invoke();
            _player.MovePlayerTo(_kidsDoorSpawn);

            // Check if kitchen is completed
            if (J_GameManager.Instance.IsStageCompleted(_kitchenDoor.ReturnNextSceneName()))
            {
                _kitchenDoor.ToggleSceneEnterable(false, true);
                kitchenSceneCompleted?.Invoke();
                _player.MovePlayerTo(_kitchenSpawn);

                // Check if parents room is completed (Shouldn't run)
                if (J_GameManager.Instance.IsStageCompleted(_parentsRoomDoor.ReturnNextSceneName()))
                {
                    _parentsRoomDoor.ToggleSceneEnterable(true, false);
                    parentsRoomSceneCompleted?.Invoke();
                    _player.MovePlayerTo(_parentsDoorSpawn);
                }
                else
                {
                    _parentsRoomDoor.ToggleSceneEnterable(false, true);
                }
            }
            else
            {
                _kitchenDoor.ToggleSceneEnterable(true, false);
            }
        }
    }
}
