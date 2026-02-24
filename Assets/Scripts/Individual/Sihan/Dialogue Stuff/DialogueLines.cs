using UnityEngine;
using TMPro;

[System.Serializable]
public class DialogueLines
{
    [Header("Dialogue Line Info")]
    // The dialogue line itself
    [TextArea] public string dialogue;

    // Icon show at the side of the dialogue ui
    public Sprite icon;
    public Color iconColor = Color.white;
    // Icon positioned behind the first icon
    public Sprite icon2;
    public Color iconColor2 = Color.white;

    // Name of the character speaking
    public string name;

    [Header("Dialogue Line Settings")]
    // Check whether the dialogue line is from the which
    public int dialogueUIIndex;
    public bool isSkippable = true;
    public bool ableToEnterNextLine = true;
    public bool isAutoNextLine = true;
    public bool isInstant = false;
    public bool erasePreviousText = true;
    // Whether the player can use left and right arrows like <color> </color> to change colour of text
    public bool useLeftRightArrowShits = true;
    public bool playAnimation = true;

    [Header("Overrides")]
    public bool overrideTypingSpeed = false;
    public float typingSpeed;

    public bool overrideAutoNextLineTime = false;
    public float autoNextLineTime;

    public bool overrideDialogueNameAlignment = false;
    public TextAlignmentOptions dialogueNameAlignment;

    public bool overrideDialogueTextAlignment = false;
    public TextAlignmentOptions dialogueTextAlignment;

    [Header("Dialogue Events")]
    // Dialogue Events that get triggered when entering or exiting this dialogue line
    public DialogueEvent onEnterDialogue;
    public DialogueEvent onExitDialogue;
}