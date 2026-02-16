using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class DialogueEventListeners : MonoBehaviour
{
    [System.Serializable]
    public class DialogueEventListener
    {
        // Dialogue Event associated with the Unity Event
        [SerializeField] private DialogueEvent dialogueEvent;

        // Unity Event
        [SerializeField] private UnityEvent whatToDo;

        // Add System Action to trigger the Unity Event
        public void OnEnable()
        {
            if (dialogueEvent != null)
            {
                dialogueEvent.Event += InvokeEvent;
            }
        }
        public void OnDisable()
        {
            if (dialogueEvent != null)
            {
                dialogueEvent.Event -= InvokeEvent;
            }
        }

        // Invoke the Unity Event when the Dialogue System Action is triggered
        private void InvokeEvent() => whatToDo?.Invoke();
    }

    [SerializeField] List<DialogueEventListener> _dialogueEventListeners;
    private void OnEnable()
    {
        foreach (var listener in _dialogueEventListeners)
            listener.OnEnable();
    }

    private void OnDisable()
    {
        foreach (var listener in _dialogueEventListeners)
            listener.OnDisable();
    }
}