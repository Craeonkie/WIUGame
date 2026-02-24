using UnityEngine;
using System.Collections.Generic;
using TMPro;

[CreateAssetMenu(fileName = "DialogueSettings", menuName = "Scriptable Objects/DialogueSettings")]
public class DialogueSettings : ScriptableObject
{
    [Header("Typing")]
    // Speed of each character being typed etc 0.01 for 1 character every 0.01 seconds
    public float typingSpeed;
    
    // Boolean to check if text auto goes to the next Line
    [SerializeField] private bool _autoNextLine;
    public bool autoNextLine
    {
        get => _autoNextLine;
        set => _autoNextLine = value;
    }
    // Time it takes to auto go to the next line
    [SerializeField] private float _autoNextLineTime;
    public float autoNextLineTime
    {
        get => _autoNextLineTime;
        set => _autoNextLineTime = value;
    }

    [System.Serializable]
    public class DialogueUISettings
    {
        [Header("Thingamabob")]
        public string dialogueUIName;

        [Header("Dialogue UI")]
        public float dialogueUIYOffset;

        [Header("Dialogue Box")]
        public float dialogueBoxXOffset;
        public float dialogueBoxYOffset;
        public float dialogueBoxWidth;
        public float dialogueBoxHeight;

        [Header("Dialogue Portrait")]
        public float dialoguePortraitXOffset;
        public float dialoguePortraitYOffset;
        public float dialoguePortraitWidth;
        public float dialoguePortraitHeight;

        [Header("Dialogue Portrait 2")]
        public float dialoguePortrait2XOffset;
        public float dialoguePortrait2YOffset;
        public float dialoguePortrait2Width;
        public float dialoguePortrait2Height;

        [Header("Dialogue Name")]
        public float dialogueNameXOffset;
        public float dialogueNameYOffset;
        public float dialogueNameWidth;
        public float dialogueNameHeight;
        public float dialogueNameFontSize;
        public TextAlignmentOptions dialogueNameTextAlignment;


        [Header("Dialogue Text")]
        public float dialogueTextXOffset;
        public float dialogueTextYOffset;
        public float dialogueTextWidth;
        public float dialogueTextHeight;
        public float dialogueTextFontSize;
        public TextAlignmentOptions dialogueTextAlignment;
    }

    public List<DialogueUISettings> dialogueUserInterfaces;
}