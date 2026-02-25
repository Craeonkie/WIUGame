using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class A_RestSceneManager : MonoBehaviour
{
    [SerializeField] private DoorToAnotherScene kidRoomDoor;
    [SerializeField] private DoorToAnotherScene kitchenDoor;
    [SerializeField] private DoorToAnotherScene parentsRoomDoor;


    [Header("Do these things when the corresponding scene is completed")]
    public UnityEvent kidsRoomSceneCompleted;
    public UnityEvent kitchenSceneCompleted;
    public UnityEvent parentsRoomSceneCompleted;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //if (J_GameManager.Instance == null)
        //{
        //    Cursor.lockState = CursorLockMode.Confined;
        //    SceneManager.LoadScene("J_MenuScene", LoadSceneMode.Single);
        //    return;
        //}
        UpdateDoorsInScene();
    }

    // Run this function to update the doors in the scene
    public void UpdateDoorsInScene()
    {
        // Make only kids room accessible
        kidRoomDoor.ToggleSceneEnterable(true, false);
        kitchenDoor.ToggleSceneEnterable(false, false);
        parentsRoomDoor.ToggleSceneEnterable(false, false);

        // Check if kids room is completed
        if (J_GameManager.Instance.IsStageCompleted(kidRoomDoor.ReturnNextSceneName()))
        {
            kidRoomDoor.ToggleSceneEnterable(false, true);
            kidsRoomSceneCompleted?.Invoke();

            // Check if kitchen is completed
            if (J_GameManager.Instance.IsStageCompleted(kitchenDoor.ReturnNextSceneName()))
            {
                kitchenDoor.ToggleSceneEnterable(false, true);
                kitchenSceneCompleted?.Invoke();

                // Check if parents room is completed (Shouldn't run)
                if (J_GameManager.Instance.IsStageCompleted(parentsRoomDoor.ReturnNextSceneName()))
                {
                    parentsRoomDoor.ToggleSceneEnterable(true, false);
                    parentsRoomSceneCompleted?.Invoke();
                }
                else
                {
                    parentsRoomDoor.ToggleSceneEnterable(false, true);
                }
            }
            else
            {
                kitchenDoor.ToggleSceneEnterable(true, false);
            }
        }
    }
}
