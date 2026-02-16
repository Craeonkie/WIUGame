using UnityEngine;

[CreateAssetMenu(fileName = "DialogueEvent", menuName = "Scriptable Objects/DialogueEvent")]
public class DialogueEvent : ScriptableObject
{
    // Dialogue Event that can be triggered in the Dialogue System
    public System.Action Event;

    // Listener to the Event
    public void InvokeEvent() => Event?.Invoke();
}