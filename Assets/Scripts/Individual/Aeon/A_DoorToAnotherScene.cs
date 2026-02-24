using UnityEngine;

public class DoorToAnotherScene : Interactable
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private GameObject doorGameobject;

    public override void InteractWith()
    {
        SceneLoader.Instance.LoadScene(nextSceneName);
    }

    // Set the door open (Should not ever be true, true, for our purposes)
    public void ToggleSceneEnterable(bool isEnterable, bool isDoorOpen)
    {
        if (doorGameobject != null)
        {
            if (isDoorOpen)
            {
                doorGameobject.transform.rotation = Quaternion.Euler(0, 80, 0);
            }
            else
            {
                doorGameobject.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
        if (isEnterable)
        {
            tag = "Interactable";
        }
        else
        {
            tag = "Default";
        }
    }

    public string ReturnNextSceneName()
    {
        return nextSceneName;
    }
}