using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class J_InteractiveHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Button Configuration")]
    [SerializeField] private bool _applyToTextChild;
    [SerializeField] private float _resetEffectSpeed;
    [SerializeField] private float _hoverEffectSpeed;
    [SerializeField] private float _pressedEffectSpeed;

    [Header("Button Interaction Settings")]
    [SerializeField] private Vector2 _hoverPositionOffset;
    [SerializeField] private Vector2 _pressedPositionOffset;
    [SerializeField] private Vector2 _hoverSizeOffset;
    [SerializeField] private Vector2 _pressedSizeOffset;
    [SerializeField] private AudioClip _hoverClip;

    [Header("Text Interaction Settings")]
    [SerializeField] private Vector2 _hoverTextPositionOffset;
    [SerializeField] private Vector2 _pressedTextPositionOffset;
    [SerializeField] private float _hoverFontSize;
    [SerializeField] private float _pressedFontSize;
    [SerializeField] private Color _hoverColour;
    [SerializeField] private Color _pressedColour;
    [SerializeField] private Color _deactivatedColour;
    [SerializeField] private FontStyles _hoverFontStyle;
    [SerializeField] private FontStyles _pressedFontStyle;

    // Components
    private GameObject _text = null;
    private RectTransform _buttonTransform;

    // Original Values
    private Vector2 _originalPosition;
    private Vector2 _originalSize;
    
    private Vector2 _originalTextPosition;
    private float _originalFontSize;
    private Color _originalColour;
    private FontStyles _originalFontStyle;

    // Boolean
    private bool _isInitialized = false;
    private bool _isHovered = false;
    private bool _isPressed = false;

    // Coroutines
    private Coroutine _effectCoroutine = null;

    private void Awake()
    {
        _buttonTransform = GetComponent<RectTransform>();

        if (_applyToTextChild)
        {
            _text = transform.GetChild(0).gameObject;
            if (_text == null)
            {
                Debug.LogError("THIS BUTTON DOES NOT HAVE A TEXT CHILD");
            }
            else
            {
                _originalFontSize = _text.GetComponent<TextMeshProUGUI>().fontSize;
                _originalColour = _text.GetComponent<TextMeshProUGUI>().color;
                _originalFontStyle = _text.GetComponent<TextMeshProUGUI>().fontStyle;
            }
        }
    }

    private void Start()
    {
        // Wait one frame for Layout Group to calculate positions
        StartCoroutine(InitializeAfterLayout());
    }

    private void OnEnable()
    {
        if (_applyToTextChild)
        {
            _text.GetComponent<TextMeshProUGUI>().color = _originalColour;
        }

        J_MenuManager.OnUpdateQuality += UpdateOriginalTextFont;
    }

    private void OnDisable()
    {
        _buttonTransform.anchoredPosition = _originalPosition;
        _buttonTransform.sizeDelta = _originalSize;

        if (_applyToTextChild)
        {
            _text.GetComponent<RectTransform>().anchoredPosition = _originalTextPosition;
            _text.GetComponent<TextMeshProUGUI>().fontSize = _originalFontSize;
            _text.GetComponent<TextMeshProUGUI>().color = _deactivatedColour;
            _text.GetComponent<TextMeshProUGUI>().fontStyle = _originalFontStyle;
        }

        J_MenuManager.OnUpdateQuality -= UpdateOriginalTextFont;
    }

    private void Update()
    {
        if (!_isInitialized || _effectCoroutine != null)
            return;

        if (_isPressed)
        {
            _effectCoroutine = StartCoroutine(EffectCoroutine(_pressedPositionOffset, _pressedSizeOffset, _pressedTextPositionOffset, _pressedFontSize, _pressedColour, _pressedFontStyle, _pressedEffectSpeed));
        }
        else if (_isHovered)
        {
            _effectCoroutine = StartCoroutine(EffectCoroutine(_hoverPositionOffset, _hoverSizeOffset, _hoverTextPositionOffset, _hoverFontSize, _hoverColour, _hoverFontStyle, _hoverEffectSpeed));
        }
        else
        {
            _effectCoroutine = StartCoroutine(EffectCoroutine(Vector2.zero, Vector2.zero, Vector2.zero, _originalFontSize, _originalColour, _originalFontStyle, _resetEffectSpeed));
        }
    }

    private void UpdateOriginalTextFont(string text, FontStyles fontStyle)
    {
        if (!_applyToTextChild)
            return;            
        else if (_text.GetComponent<TextMeshProUGUI>().text != text)
            return;

        _originalFontStyle = fontStyle;
    }

    private IEnumerator InitializeAfterLayout()
    {
        yield return null; // Wait one frame

        _originalPosition = _buttonTransform.anchoredPosition;
        _originalSize = _buttonTransform.sizeDelta;

        if (_applyToTextChild && _text != null)
        {
            _originalTextPosition = _text.GetComponent<RectTransform>().anchoredPosition;
        }

        _isInitialized = true;
    }

    private IEnumerator EffectCoroutine(Vector2 _positionOffset, Vector2 _sizeOffset, Vector2 _textPositionOffset, float _textFontSize, Color _textColor, FontStyles _fontStyle, float speed)
    {
        float duration = 0f;
        var endPos = _originalPosition + _positionOffset;
        var endSize = _originalSize + _sizeOffset;
        var endTextPos = _originalTextPosition + _textPositionOffset;

        if (_applyToTextChild)
        {
            _text.GetComponent<TextMeshProUGUI>().fontStyle = _fontStyle;
        }

        while (duration < speed)
        {
            float t = duration / speed;

            _buttonTransform.anchoredPosition = Vector2.Lerp(_buttonTransform.anchoredPosition, endPos, t);
            _buttonTransform.sizeDelta = Vector2.Lerp(_buttonTransform.sizeDelta, endSize, t);

            if (_applyToTextChild)
            {
                _text.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_text.GetComponent<RectTransform>().anchoredPosition, endTextPos, t);
                _text.GetComponent<TextMeshProUGUI>().fontSize = Mathf.Lerp(_text.GetComponent<TextMeshProUGUI>().fontSize, _textFontSize, t);
                _text.GetComponent<TextMeshProUGUI>().color = Color.Lerp(_text.GetComponent<TextMeshProUGUI>().color, _textColor, t);
            }

            duration += Time.deltaTime;

            yield return null;
        }

        _buttonTransform.anchoredPosition = endPos;
        _buttonTransform.sizeDelta = endSize;

        if (_applyToTextChild)
        {
            _text.GetComponent<RectTransform>().anchoredPosition = endTextPos;
            _text.GetComponent<TextMeshProUGUI>().fontSize = _textFontSize;
            _text.GetComponent<TextMeshProUGUI>().color = _textColor; 
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isInitialized)
            return;

        _isHovered = true;

        if (_effectCoroutine != null)
        {
            StopCoroutine(_effectCoroutine);
            _effectCoroutine = null;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isInitialized)
            return;

        _isHovered = false;
        _isPressed = false;

        if (_effectCoroutine != null)
        {
            StopCoroutine(_effectCoroutine);
            _effectCoroutine = null;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_isHovered)
            return;

        _isPressed = true;

        if (_effectCoroutine != null)
        {
            StopCoroutine(_effectCoroutine);
            _effectCoroutine = null;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;

        if (_effectCoroutine != null)
        {
            StopCoroutine(_effectCoroutine);
            _effectCoroutine = null;
        }
    }
}
