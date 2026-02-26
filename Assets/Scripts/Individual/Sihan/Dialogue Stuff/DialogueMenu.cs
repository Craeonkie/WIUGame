using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueMenu : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueSettings dialogueSettings;

    [Header("Dialogue UI Elements")]
    [SerializeField] private RectTransform _dialogueUI;
    [SerializeField] private Image _dialogueBox;
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private TMP_Text _dialogueName;
    [SerializeField] private Image _dialogueIcon;
    [SerializeField] private Image _dialogueIcon2;


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
    // time for each letter to be typed
    [SerializeField] private float _typingSpeed;
    // time to go to the next line if auto next line is enabled
    [SerializeField] private float _autoNextLineTime;

    // current dialogue
    [SerializeField] private Dialogue _currentDialogue;
    public Dialogue currentDialogue
    {
        get => _currentDialogue;
        set => _currentDialogue = value;
    }
    [SerializeField] private StateDialogue _currentStateDialogue;

    [Header("Animation :D")]
    [SerializeField] private float _defaultScale;
    [SerializeField] private AnimationCurve _growCurve;
    [SerializeField] private AnimationCurve _shrinkCurve;
    [SerializeField] private AnimationCurve _nextLineCurve;
    [SerializeField] private float _growSpeed;
    [SerializeField] private float _shrinkSpeed;
    [SerializeField] private float _nextLineAnimSpeed;
    private Coroutine _animCoroutine;

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

                    if (_animCoroutine != null)
                    {
                        StopCoroutine(_animCoroutine);
                        _animCoroutine = null;
                    }

                    // Start the dialogue
                    EnterNewDialogue();

                    // Makes dialogue UI visible
                    _dialogueUI.gameObject.SetActive(true);

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
        if (Time.timeScale == 0) return;

        if (dialogueSettings != null/* && !garbageManager.isMenuOpen*/)
        {
            Click();

            if (_type && !isPaused)
            {
                // Check if im typing still
                if (_isTyping && _currentDialogue != null)
                {
                    // Increase typing counter by delta time
                    _typingCounter += Time.deltaTime;

                    // Check if typing counter passes the typing speed for each letter
                    if (_typingCounter >= _typingSpeed)
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
                    if (_currentStateDialogue.dialogueLines[_dialogueLineIndex].isAutoNextLine)
                    {
                        _nextLineTimer += Time.deltaTime;
                        if (_nextLineTimer >= _autoNextLineTime)
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

                                if (_currentStateDialogue.playCloseAnimation)
                                {
                                    if (_animCoroutine != null)
                                    {
                                        StopCoroutine(_animCoroutine);
                                        _animCoroutine = null;
                                    }

                                    _animCoroutine = StartCoroutine(ShrinkUI());
                                    return;
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
        //var click = InputSystem.actions.FindAction("Primary");
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
        //var click = InputSystem.actions.FindAction("Primary");
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
                            _dialogueText.text += currentLine.dialogue.Substring(_typingIndex, _maxTypingIndex - _typingIndex);

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

                            if (_currentStateDialogue.playCloseAnimation)
                            {
                                if (_animCoroutine != null)
                                {
                                    StopCoroutine(_animCoroutine);
                                    _animCoroutine = null;
                                }

                                _animCoroutine = StartCoroutine(ShrinkUI());
                                return;
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

    private void Click()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
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
                            _dialogueText.text += currentLine.dialogue.Substring(_typingIndex, _maxTypingIndex - _typingIndex);


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

                            if (_currentStateDialogue.playCloseAnimation)
                            {
                                if (_animCoroutine != null)
                                {
                                    StopCoroutine(_animCoroutine);
                                    _animCoroutine = null;
                                }

                                _animCoroutine = StartCoroutine(ShrinkUI());
                                return;
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
        if (_dialogueText != null) _dialogueText.text = "";
        if (_dialogueName != null) _dialogueName.text = "";
        if (_dialogueIcon != null) _dialogueIcon.sprite = null;
        if (_dialogueIcon2 != null) _dialogueIcon2.sprite = null;
        _currentDialogue = null;
        _currentStateDialogue = null;
        isPaused = false;
        _typingSpeed = 0;
        _autoNextLineTime = 0;

        _dialogueUI.gameObject.SetActive(false);
    }

    public void EnterNewDialogue()
    {
        // Enable UI
        // garbageManager.isUIOpen = true;
        _typingCounter = 0;

        if (_dialogueLineIndex >= _maxDialogueLinesIndex)
        {
            if (_currentStateDialogue.playCloseAnimation)
            {
                if (_animCoroutine != null)
                {
                    StopCoroutine(_animCoroutine);
                    _animCoroutine = null;
                }

                _animCoroutine = StartCoroutine(ShrinkUI());
                return;
            }

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

            if (currentLine.erasePreviousText || _dialogueLineIndex == 0) _dialogueText.text = "";
        }
        else
        {
            _isTyping = false;
            _typingIndex = _maxTypingIndex;
            if (currentLine.erasePreviousText || _dialogueLineIndex == 0)
            {
                _dialogueText.text = currentLineDialogue;
            }
            else
            {
                _dialogueText.text += currentLineDialogue;
            }
        }

        if (_dialogueLineIndex != 0)
        {
            if (currentLine.playAnimation)
            {
                if (_animCoroutine != null)
                {
                    StopCoroutine(_animCoroutine);
                    _animCoroutine = null;
                }

                _animCoroutine = StartCoroutine(NextLineAnimUI());
            }
            else
            {
                if (_animCoroutine != null)
                {
                    StopCoroutine(_animCoroutine);
                    _animCoroutine = null;
                }

                _dialogueUI.localScale = Vector3.one * _defaultScale;
            }
        }
        else if (_currentStateDialogue.playOpenAnimation)
        {
            if (_animCoroutine != null)
            {
                StopCoroutine(_animCoroutine);
                _animCoroutine = null;
            }

            _animCoroutine = StartCoroutine(GrowUI());
        }

        _dialogueName.text = currentLine.name;

        isPaused = false;

        if (currentLine.isAutoNextLine)
        {
            _nextLineTimer = 0;
        }

        var dialogueUIIndex = currentLine.dialogueUIIndex;
        if (dialogueSettings.dialogueUserInterfaces != null && dialogueUIIndex < dialogueSettings.dialogueUserInterfaces.Count)
        {
            var dialogueUI = dialogueSettings.dialogueUserInterfaces[dialogueUIIndex];
            if (_dialogueUI != null)
            {
                _dialogueUI.anchoredPosition = new Vector2(0, dialogueUI.dialogueUIYOffset);
            }

            if (_dialogueBox != null)
            {
                _dialogueBox.rectTransform.sizeDelta = new Vector2(dialogueUI.dialogueBoxWidth, dialogueUI.dialogueBoxHeight);
                _dialogueBox.rectTransform.anchoredPosition = new Vector2(dialogueUI.dialogueBoxXOffset, dialogueUI.dialogueBoxYOffset);
            }

            if (_dialogueIcon != null)
            {
                _dialogueIcon.rectTransform.sizeDelta = new Vector2(dialogueUI.dialoguePortraitWidth, dialogueUI.dialoguePortraitHeight);
                _dialogueIcon.rectTransform.anchoredPosition = new Vector2(dialogueUI.dialoguePortraitXOffset, dialogueUI.dialoguePortraitYOffset);
                _dialogueIcon.color = currentLine.iconColor;
            }

            if (_dialogueIcon2 != null)
            {
                _dialogueIcon2.rectTransform.sizeDelta = new Vector2(dialogueUI.dialoguePortrait2Width, dialogueUI.dialoguePortrait2Height);
                _dialogueIcon2.rectTransform.anchoredPosition = new Vector2(dialogueUI.dialoguePortrait2XOffset, dialogueUI.dialoguePortrait2YOffset);
                _dialogueIcon2.color = currentLine.iconColor2;
            }

            if (_dialogueName != null)
            {
                _dialogueName.rectTransform.sizeDelta = new Vector2(dialogueUI.dialogueNameWidth, dialogueUI.dialogueNameHeight);
                _dialogueName.rectTransform.anchoredPosition = new Vector2(dialogueUI.dialogueNameXOffset, dialogueUI.dialogueNameYOffset);
                _dialogueName.fontSize = dialogueUI.dialogueNameFontSize;
                _dialogueName.alignment = dialogueUI.dialogueNameTextAlignment;
            }

            if (_dialogueText != null)
            {
                _dialogueText.rectTransform.sizeDelta = new Vector2(dialogueUI.dialogueTextWidth, dialogueUI.dialogueTextHeight);
                _dialogueText.rectTransform.anchoredPosition = new Vector2(dialogueUI.dialogueTextXOffset, dialogueUI.dialogueTextYOffset);
                _dialogueText.fontSize = dialogueUI.dialogueTextFontSize;
                _dialogueText.alignment = dialogueUI.dialogueTextAlignment;
            }


            //if (AudioLibrary.Instance != null)
            //{
            //    AudioLibrary.Instance.PlaySound("playerTyping");
            //}
        }
        else
        {
            Debug.LogWarning("Dialogue UI Settings not found, using default editor UI Settings");
        }

        if (_dialogueIcon != null)
        {
            if (currentLine.icon != null)
            {
                _dialogueIcon.sprite = currentLine.icon;
                _dialogueIcon.color = currentLine.iconColor;
            }
            else
                _dialogueIcon.color = new Color(0, 0, 0, 0);
        }

        if (_dialogueIcon2 != null)
        {
            if (currentLine.icon2 != null)
            {
                _dialogueIcon2.sprite = currentLine.icon2;
                _dialogueIcon2.color = currentLine.iconColor2;
            }
            else
                _dialogueIcon2.color = new Color(0, 0, 0, 0);
        }

        // Overriding
        if (currentLine.overrideTypingSpeed)
        {
            _typingSpeed = currentLine.typingSpeed;
        }
        else
        {
            _typingSpeed = dialogueSettings.typingSpeed;
        }

        if (currentLine.overrideAutoNextLineTime)
        {
            _autoNextLineTime = currentLine.autoNextLineTime;
        }
        else
        {
            _autoNextLineTime = dialogueSettings.autoNextLineTime;
        }

        if (currentLine.overrideDialogueNameAlignment)
        {
            _dialogueName.alignment = currentLine.dialogueNameAlignment;
        }

        if (currentLine.overrideDialogueTextAlignment)
        {
            _dialogueText.alignment = currentLine.dialogueTextAlignment;
        }

        if (currentLine.overrideBaseNameColor)
        {
            _dialogueName.color = currentLine.baseNameColor;
        }

        if (currentLine.overrideBaseTextColor)
        {
            _dialogueText.color = currentLine.baseTextColor;
        }
    }

    public IEnumerator GrowUI()
    {
        float elapsedTime = 0f;

        while (elapsedTime < 1)
        {
            elapsedTime += Time.deltaTime * _growSpeed;
            elapsedTime = Mathf.Clamp01(elapsedTime);

            _dialogueUI.localScale = Vector3.one * _defaultScale * Mathf.Clamp(_growCurve.Evaluate(elapsedTime), 0, Mathf.Infinity);
            yield return null;
        }
    }

    public IEnumerator ShrinkUI()
    {
        _type = false;
        _isTyping = false;
        _dialogueLineIndex = 0;
        _maxDialogueLinesIndex = 0;
        _typingCounter = 0;
        _typingIndex = 0;
        _currentDialogue = null;
        _currentStateDialogue = null;
        isPaused = false;
        _typingSpeed = 0;
        _autoNextLineTime = 0;

        float elapsedTime = 0f;
        float localScale = transform.localScale.x;

        while (elapsedTime < 1)
        {
            elapsedTime += Time.deltaTime * _shrinkSpeed;
            elapsedTime = Mathf.Clamp01(elapsedTime);

            _dialogueUI.localScale = Vector3.one * localScale * Mathf.Clamp(_shrinkCurve.Evaluate(elapsedTime), 0, Mathf.Infinity);
            yield return null;
        }

        ResetDialogue();
    }

    public IEnumerator NextLineAnimUI()
    {
        float elapsedTime = 0f;

        while (elapsedTime < 1)
        {
            elapsedTime += Time.deltaTime * _nextLineAnimSpeed;
            elapsedTime = Mathf.Clamp01(elapsedTime);
            _dialogueUI.localScale = Vector3.one * _defaultScale * Mathf.Clamp(_nextLineCurve.Evaluate(elapsedTime), 0, Mathf.Infinity);
            yield return null;
        }
    }
}