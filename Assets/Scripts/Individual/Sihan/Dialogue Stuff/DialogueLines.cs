using UnityEngine;

[System.Serializable]
public class DialogueLines
{
    // The dialogue line itself
    [TextArea] public string dialogue;

    // Icon show at the side of the dialogue ui
    public Sprite icon;

    // Name of the character speaking
    public string name;

    // Check whether the dialogue line is from the which
    public int dialogueUIIndex;

    public bool isSkippable = true;
    public bool ableToEnterNextLine = true;
    public bool isAutoNextLine = true;
    public bool isInstant = false;
    public bool erasePreviousText = true;
    // Whether the player can use left and right arrows like <color> </color> to change colour of text
    public bool useLeftRightArrowShits = true;

    public bool customTypingSpeed = false;
    public float typingSpeed;

    public bool customAutoNextLineTime = false;
    public float autoNextLineTime;

    public bool playAnimation = true;

    // Dialogue Events that get triggered when entering or exiting this dialogue line
    public DialogueEvent onEnterDialogue;
    public DialogueEvent onExitDialogue;
}