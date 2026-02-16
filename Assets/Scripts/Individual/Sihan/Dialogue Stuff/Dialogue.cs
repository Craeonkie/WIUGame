using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Scriptable Objects/Dialogue")]
public class Dialogue : ScriptableObject
{
    // Integer associated to dialogue dialogue
    [SerializeField] private int _dialogueID;
    public int dialogueID { get => _dialogueID; set => _dialogueID = value; }

    // List of Dialogues Lines + their respective ids
    public List<StateDialogue> stateDialogues;

    public void IncreaseDialogueID()
    {
        dialogueID++;
    }

    public void SetDialogueID(int id)
    {
        dialogueID = id;
    }
}