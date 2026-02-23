using UnityEngine;

public class DoorManager : MonoBehaviour
{
    [SerializeField] private DoorToAnotherScene kidRoomDoor;
    [SerializeField] private DoorToAnotherScene kitchenDoor;
    [SerializeField] private DoorToAnotherScene parentsRoomDoor;

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
            // Check if kitchen is completed
            if (J_GameManager.Instance.IsSceneVisited(kitchenDoor.ReturnNextSceneName()))
            {
                kitchenDoor.ToggleSceneEnterable(false, true);
                // Check if parents room is completed (Shouldn't run)
                if (J_GameManager.Instance.IsSceneVisited(kitchenDoor.ReturnNextSceneName()))
                {
                    parentsRoomDoor.ToggleSceneEnterable(true, false);
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
