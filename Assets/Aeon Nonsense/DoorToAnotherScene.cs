using UnityEngine;

public class DoorToAnotherScene : Interactable
{
    [SerializeField] private string nextSceneName;

    public override void InteractWith()
    {
        SceneLoader.Instance.LoadScene(nextSceneName);
    }
}