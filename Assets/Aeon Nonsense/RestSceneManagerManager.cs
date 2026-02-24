using UnityEngine;
using UnityEngine.Events;

public class RestSceneManagerManager : MonoBehaviour
{
    [SerializeField] private DoorToAnotherScene kidRoomDoor;
    [SerializeField] private DoorToAnotherScene kitchenDoor;
    [SerializeField] private DoorToAnotherScene parentsRoomDoor;

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
        kidRoomDoor.ToggleSceneEnterable(true, false);
        kitchenDoor.ToggleSceneEnterable(false, false);
        parentsRoomDoor.ToggleSceneEnterable(false, false);

        // Check if kids room is completed
        if (J_GameManager.Instance.IsSceneVisited(kidRoomDoor.ReturnNextSceneName()))
        {
            kidRoomDoor.ToggleSceneEnterable(false, true);
            kidsRoomSceneCompleted?.Invoke();

            // Check if kitchen is completed
            if (J_GameManager.Instance.IsSceneVisited(kitchenDoor.ReturnNextSceneName()))
            {
                kitchenDoor.ToggleSceneEnterable(false, true);
                kitchenSceneCompleted?.Invoke();

                // Check if parents room is completed (Shouldn't run)
                if (J_GameManager.Instance.IsSceneVisited(parentsRoomDoor.ReturnNextSceneName()))
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
