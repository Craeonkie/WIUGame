using System.Collections.Generic;

[System.Serializable]
public class StateDialogue
{
    // Integer associated with this dialogue
    public int dialogueID;

    public bool increaseMainDialogueIDWhenComplete = true;

    // List of each dialogue
    public List<DialogueLines> dialogueLines;
}