using UnityEngine;
using UnityEngine.Events;

public class SimpleInteractable : Interactable
{
    public UnityEvent onInteract;

    public override void InteractWith()
    {
        onInteract?.Invoke();
    }
}
