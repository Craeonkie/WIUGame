using UnityEngine;
using UnityEngine.Events;

public class DoorToAnotherScene : Interactable
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private GameObject doorGameobject;
    [SerializeField] private DialogueMenu _dialogueMenu;
    [SerializeField] private Dialogue _dialogue;
    public UnityEvent interactWith;

    public override void InteractWith()
    {
        if (interactWith != null)
        {
            interactWith.Invoke();
        }
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
            _dialogue.SetDialogueID(1);
        }
        else
        {
            _dialogue.SetDialogueID(0);
        }
    }

    public string ReturnNextSceneName()
    {
        return nextSceneName;
    }

    public void GoToNextScene()
    {
        SceneLoader.Instance.LoadScene(nextSceneName);
    }
}