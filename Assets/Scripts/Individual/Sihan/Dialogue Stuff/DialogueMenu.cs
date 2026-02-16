using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueMenu : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueSettings dialogueSettings;

    [Header("Dialogue UI Elements")]
    [SerializeField] private Image _dialogueBox;
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private TMP_Text _dialogueName;
    [SerializeField] private Image _dialogueIcon;

    [Header("Garbage Manager")]
    //public GarbageManager garbageManager;

    [Header("Dialogue Statistics")]
    // If in a dialogue
    [SerializeField] private bool _type;
    // If dialogue is typing
    [SerializeField] private bool _isTyping;
    // number of dialogue lines in the current dialogue
    [SerializeField] private int _maxDialogueLinesIndex;
    // current dialogue line index
    [SerializeField] private int _dialogueLineIndex;
    // current index of each letter in the current dialogue line
    [SerializeField] private int _typingIndex;
    // number of letters in the current dialogue line
    [SerializeField] private int _maxTypingIndex;
    // time to next letter typed
    [SerializeField] private float _typingCounter;
    // timer to next line (if auto next line is enabled)
    [SerializeField] private float _nextLineTimer;

    // current dialogue
    [SerializeField] private Dialogue _currentDialogue;
    public Dialogue currentDialogue
    {
        get => _currentDialogue;
        set => _currentDialogue = value;
    }
    [SerializeField] private StateDialogue _currentStateDialogue;

    public bool isPaused { get; set; }

    public void Awake()
    {
        // Reset
        ResetDialogue();
    }

    public void Start()
    {
    }

    public void StartTyping()
    {
        // Check if Dialogue Scriptable Object exists before starting Dialogue
        if (currentDialogue != null)
        {
            var stateDialogues = _currentDialogue.stateDialogues;

            // Check through all the state dialogues in the NPC's dialogue
            foreach (var stateDialogue in stateDialogues)
            {
                // Check if theres a state dialogue that matches the current dialogue ID
                // Sets the required details based on the matched state dialogue
                if (_currentDialogue.dialogueID == stateDialogue.dialogueID)
                {
                    // Set the current state dialogue to the matched state dialogue
                    _currentStateDialogue = stateDialogue;

                    // Reset the line index to 0 so it starts from the start of the state dialogue;
                    _dialogueLineIndex = 0;

                    // Set the max amount of dialogues lines to the amount of dialogue lines in the current state dialogue
                    _maxDialogueLinesIndex = _currentStateDialogue.dialogueLines.Count;

                    // Check if theres 0 dialogue lines
                    if (_maxDialogueLinesIndex == 0)
                    {
                        Debug.LogWarning($"Dialogue found for id {_currentStateDialogue.dialogueID} in dialogue {_currentDialogue.name} has 0 lines");
                        return;
                    }

                    // Start the dialogue
                    EnterNewDialogue();

                    // Makes dialogue UI visible
                    transform.GetChild(0).gameObject.SetActive(true);

                    break;
                }
            }

            //if (!dialogueSettings.isTyping)
            //{
            //    Debug.LogWarning($"No dialogue found for id {dialogueSettings.currentDialogue.dialogueID} in dialogue {dialogueSettings.currentDialogue.name}");
            //    ResetDialogue();
            //}
        }
        else
        {
            Debug.LogWarning("CurrentDialogue is null");
        }
    }

    private int FindNextLetter(string dialogue, int startIndex, char delimiter)
    {
        for (int i = startIndex; i < dialogue.Length; i++)
        {
            if (dialogue[i] == delimiter)
            {
                return i - startIndex;
            }
        }

        return 0;
    }


    private void Update()
    {
        if (dialogueSettings != null/* && !garbageManager.isMenuOpen*/)
        {
            if (_type && !isPaused)
            {
                // Check if im typing still
                if (_isTyping && _currentDialogue != null)
                {
                    // Increase typing counter by delta time
                    _typingCounter += Time.deltaTime;

                    // Check if typing counter passes the typing speed for each letter
                    if (_typingCounter >= dialogueSettings.typingSpeed)
                    {
                        // Reset the typing counter
                        _typingCounter = 0;

                        // Check if the typing index (letter) is still smaller than the last letter index
                        if (_typingIndex < _maxTypingIndex)
                        {
                            if (_currentStateDialogue.dialogueLines[_dialogueLineIndex].useLeftRightArrowShits)
                            {
                                if (_currentStateDialogue.dialogueLines[_dialogueLineIndex].dialogue[_typingIndex] == '<')
                                {
                                    int endIndex = FindNextLetter(_currentStateDialogue.dialogueLines[_dialogueLineIndex].dialogue, _typingIndex, '>');
                                    if (endIndex > 0)
                                    {
                                        // Add the whole thing with the < > to the TMP text
                                        _dialogueText.text += _currentStateDialogue.dialogueLines[_dialogueLineIndex].dialogue.Substring(_typingIndex, endIndex);
                                        _typingIndex += endIndex;

                                        if (_typingIndex < _maxTypingIndex)
                                        {
                                            // Add the letter at typing index to the TMP text
                                            _dialogueText.text += _currentStateDialogue.dialogueLines[_dialogueLineIndex].dialogue[_typingIndex];
                                            _typingIndex++;
                                        }
                                    }
                                    else
                                    {
                                        // Add the letter at typing index to the TMP text
                                        _dialogueText.text += _currentStateDialogue.dialogueLines[_dialogueLineIndex].dialogue[_typingIndex];
                                        _typingIndex++;
                                    }
                                }
                                else
                                {
                                    // Add the letter at typing index to the TMP text
                                    _dialogueText.text += _currentStateDialogue.dialogueLines[_dialogueLineIndex].dialogue[_typingIndex];
                                    _typingIndex++;
                                }
                            }
                            else
                            {
                                // Add the letter at typing index to the TMP text
                                _dialogueText.text += _currentStateDialogue.dialogueLines[_dialogueLineIndex].dialogue[_typingIndex];
                                _typingIndex++;
                            }
                        }
                        else
                        {
                            // Finished typing all the letters, so stop typing
                            _isTyping = false;

                            //if (AudioLibrary.Instance != null)
                            //{
                            //    AudioLibrary.Instance.StopSound("npcTyping");
                            //    AudioLibrary.Instance.StopSound("playerTyping");
                            //}
                        }
                    }
                }
                else
                {
                    // Check if it auto goes to the next line
                    if (_currentStateDialogue.dialogueLines[_dialogueLineIndex].isAutoNextLine && _currentStateDialogue.dialogueLines[_dialogueLineIndex].ableToEnterNextLine)
                    {
                        _nextLineTimer += Time.deltaTime;
                        if (_nextLineTimer >= dialogueSettings.autoNextLineTime)
                        {
                            // Go to the next dialogue line since typing alr finished
                            _dialogueLineIndex++;

                            _currentStateDialogue.dialogueLines[_dialogueLineIndex - 1].onExitDialogue?.InvokeEvent();

                            // Check if dialogue line index passes max dialogue lines index, if it does, stop typing and stop dialogue
                            if (_dialogueLineIndex >= _maxDialogueLinesIndex)
                            {
                                // garbageManager.isUIOpen = false;

                                if (_currentStateDialogue.increaseMainDialogueIDWhenComplete)
                                {
                                    _currentDialogue.dialogueID++;
                                }

                                ResetDialogue();                                

                                return;
                            }

                            EnterNewDialogue();
                        }
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        //var click = InputSystem.actions.FindAction("TalkClick");
        //if (click != null)
        //{
        //    click.started += Click;
        //}

        //var talk = InputSystem.actions.FindAction("Interact");
        //if (talk != null)
        //{
        //    talk.started += Talk;
        //}
    }

    private void OnDisable()
    {
        //var click = InputSystem.actions.FindAction("TalkClick");
        //if (click != null)
        //{
        //    click.started -= Click;
        //}

        //var talk = InputSystem.actions.FindAction("Interact");
        //if (talk != null)
        //{
        //    talk.started -= Talk;
        //}

        //if (_currentDialogue != null)
        //{
        //    _currentDialogue.currentStateDialogue = null;
        //    _currentDialogue = null;
        //}
    }

    private void Talk(InputAction.CallbackContext ctx)
    {
        StartTyping();
    }

    private void Click(InputAction.CallbackContext ctx)
    {
        if (dialogueSettings != null/* && garbageManager != null*/)
        {
            // Check if im in a dialogue
            if (_type & !isPaused)
            {
                if (true/*!garbageManager.isMenuOpen*/)
                {
                    var currentLine = _currentStateDialogue.dialogueLines[_dialogueLineIndex];

                    // Check if im still typing
                    if (_isTyping)
                    {
                        if (currentLine.isSkippable)
                        {
                            // Set typing to false, skip the typing and show the full dialogue line
                            _isTyping = false;
                            _dialogueText.text = currentLine.dialogue;

                            //if (AudioLibrary.Instance != null)
                            //{
                            //    AudioLibrary.Instance.StopSound("npcTyping");
                            //    AudioLibrary.Instance.StopSound("playerTyping");
                            //}
                        }
                    }
                    else if (currentLine.ableToEnterNextLine)
                    {
                        // Go to the next dialogue line since typing alr finished
                        _dialogueLineIndex++;

                        _currentStateDialogue.dialogueLines[_dialogueLineIndex - 1].onExitDialogue?.InvokeEvent();

                        // Check if dialogue line index passes max dialogue lines index, if it does, stop typing and stop dialogue
                        if (_dialogueLineIndex >= _maxDialogueLinesIndex)
                        {
                            // garbageManager.isUIOpen = false;

                            if (_currentStateDialogue.increaseMainDialogueIDWhenComplete)
                            {
                                _currentDialogue.dialogueID++;
                            }

                            ResetDialogue();

                            return;
                        }

                        // Go to next dialogue line
                        EnterNewDialogue();
                    }
                }
            }
        }
    }

    // extra
    public void Test()
    {
        Debug.Log("Yes");
    }

    public void ResetDialogue()
    {
        // Reset everything
        _type = false;
        _isTyping = false;
        _dialogueLineIndex = 0;
        _maxDialogueLinesIndex = 0;
        _typingCounter = 0;
        _typingIndex = 0;
        _dialogueText.text = "";
        _dialogueName.text = "";
        _dialogueIcon.sprite = null;
        _currentDialogue = null;
        _currentStateDialogue = null;
        isPaused = false;

        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void EnterNewDialogue()
    {
        // Enable UI
        // garbageManager.isUIOpen = true;

        if (_dialogueLineIndex >= _maxDialogueLinesIndex)
        {
            ResetDialogue();
            return;
        }

        // Enter a new dialogue line, resets the counter etc and gets the new dialogue line
        var currentLine = _currentStateDialogue.dialogueLines[_dialogueLineIndex];

        currentLine.onEnterDialogue?.InvokeEvent();
        string currentLineDialogue = currentLine.dialogue;
        _maxTypingIndex = currentLineDialogue.Length;

        _type = true;

        if (!currentLine.isInstant)
        {
            _isTyping = true;
            _typingCounter = 0;
            _typingIndex = 0;

            if (currentLine.erasePreviousText) _dialogueText.text = "";
        }
        else
        {
            _isTyping = false;
            _typingIndex = _maxTypingIndex;
            if (currentLine.erasePreviousText)
            {
                _dialogueText.text = currentLineDialogue;
            }
            else
            {
                _dialogueText.text += currentLineDialogue;
            }
        }

        _dialogueName.text = currentLine.name;

        isPaused = false;

        if (currentLine.isAutoNextLine)
        {
            _nextLineTimer = 0;
        }

        var dialogueUIIndex = currentLine.dialogueUIIndex;
        if (dialogueSettings.dialogueUIs != null && dialogueUIIndex < dialogueSettings.dialogueUIs.Count)
        {
            var dialogueUI = dialogueSettings.dialogueUIs[dialogueUIIndex];
            _dialogueBox.rectTransform.sizeDelta = new Vector2(dialogueUI.dialogueBoxWidth, dialogueUI.dialogueBoxHeight);
            _dialogueBox.rectTransform.anchoredPosition = new Vector2(dialogueUI.dialogueBoxXOffset, dialogueUI.dialogueBoxYOffset);

            _dialogueIcon.rectTransform.sizeDelta = new Vector2(dialogueUI.dialoguePortraitWidth, dialogueUI.dialoguePortraitHeight);
            _dialogueIcon.rectTransform.anchoredPosition = new Vector2(dialogueUI.dialoguePortraitXOffset, dialogueUI.dialoguePortraitYOffset);

            _dialogueName.rectTransform.sizeDelta = new Vector2(dialogueUI.dialogueNameWidth, dialogueUI.dialogueNameHeight);
            _dialogueName.rectTransform.anchoredPosition = new Vector2(dialogueUI.dialogueNameXOffset, dialogueUI.dialogueNameYOffset);
            _dialogueName.fontSize = dialogueUI.dialogueNameFontSize;

            _dialogueText.rectTransform.sizeDelta = new Vector2(dialogueUI.dialogueTextWidth, dialogueUI.dialogueTextHeight);
            _dialogueText.rectTransform.anchoredPosition = new Vector2(dialogueUI.dialogueTextXOffset, dialogueUI.dialogueTextYOffset);
            _dialogueText.fontSize = dialogueUI.dialogueTextFontSize;


            //if (AudioLibrary.Instance != null)
            //{
            //    AudioLibrary.Instance.PlaySound("playerTyping");
            //}
        }
        else
        {
            Debug.LogWarning("Dialogue UI Settings not found, using default editor UI Settings");
        }

        if (currentLine.icon != null)
        {
            _dialogueIcon.sprite = currentLine.icon;
        }
    }
}