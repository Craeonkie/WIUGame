using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class C_VHSTransition : MonoBehaviour
{
    public enum VHSState
    {
        Off,
        On,            
        Closing        
    }

    [Header("Flickering")]
    [SerializeField] private Image _playButtonUI;
    [SerializeField] private float _minFlickerTime = 0.05f;
    [SerializeField] private float _maxFlickerTime = 0.15f;
    [SerializeField] private float _minAlpha = 0.3f;
    [SerializeField] private float _maxAlpha = 1f;
    private Coroutine _playFlickerCoroutine;

    [Header("Volume")]
    [SerializeField] private TextMeshProUGUI _volText;
    private float _volValue = 1f;
    private int _currentVolBar = -1;

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _timerSText;
    [SerializeField] private TextMeshProUGUI _timerMSText;
    private float _currentGameTime = 0f;
    private Coroutine _timeCoroutine;
    private Coroutine _timeFlickerCoroutine;
    private Coroutine _timeSFlickerCoroutine;
    private Coroutine _timeMSFlickerCoroutine;

    [Header("VHS Overlay")]
    [SerializeField] private bool _VHSOn = true;
    [SerializeField] private GameObject _vhsCanvas;
    [SerializeField] private Material _vhsMat;

    [Header("Closing Transition Data")]
    [SerializeField] private float _maxScaleSize = 3f;
    [SerializeField] private float _closingWaitDuration = 0.75f;
    [SerializeField] private float _duration = 1f;

    private bool _finishClosingTransition = false;

    // cached shader values
    private float _oriBlurOffSet;
    private Color _oriHDRColor;
    private float _scanLineSpeed;
    private float _numOfScanLine;
    private float _noiseAmt;
    private Vector2 _noiseSpeed;
    private float _noiseScale;

    [Header("Clapper")]
    [SerializeField] private GameObject _clapper;
    [SerializeField] private float _stage1ZPos;
    [SerializeField] private GameObject _clapperTop;
    [SerializeField] private float _targetAngle;
    [SerializeField] private float _yPosTarget;
    [SerializeField] private float _moveSpeedY = 3f;
    [SerializeField] private AnimationCurve _moveCurveZ;
    [SerializeField] private float _phase1Duration = 1.5f;
    [SerializeField] private float _rotSpeed = 180f;
    private Vector3 _clapperOriginalPos;

    public static event System.Action FinishTransiition;

    public bool OffVHS { get; set; }

    private VHSState _state = VHSState.Off;
    private Coroutine _stateCoroutine;

    private void UpdateCurVol(float masterVol, float bgmVol, float sfxVol, float globalPitch)
    {
        _volValue = Mathf.Clamp01(masterVol);
    }

    private void Awake()
    {
        AudioLibrary.UpdateAudio += UpdateCurVol;

        CacheOriginalVhsMat();

        if (_clapper != null)
        {
            _clapperOriginalPos = _clapper.transform.localPosition;
        }

        if (_VHSOn)
        {
            SetState(VHSState.On);
        }
        else
        {
            SetState(VHSState.Off);
        }
    }

    private void Update()
    {
        // Only lightweight per-frame logic here (no starting/stopping 10 coroutines in Update)
        if (_state == VHSState.On)
        {
            UpdateTimerValue();
            UpdateVolumeUI();
            EnsureOverlayCoroutinesRunning();
        }

        if (!_VHSOn && !_finishClosingTransition)
        {
            StartClosingTransition();
            _finishClosingTransition = true;
        }

        if (_VHSOn && _finishClosingTransition)
        {
            SetState(VHSState.On);
            _finishClosingTransition = false;
        }
    }

    private void SetState(VHSState newState)
    {
        if (_state == newState) return;

        _state = newState;

        if (_stateCoroutine != null)
        {
            StopCoroutine(_stateCoroutine);
            _stateCoroutine = null;
        }

        if (_state == VHSState.Off)
        {
            DisableVHSOverlay();
            StopAllVhsCoroutines();
        }
        else if (_state == VHSState.On)
        {
            EnableVHSOverlay();
            StartVHS();
        }
        else if (_state == VHSState.Closing)
        {
            StopAllVhsCoroutines();
            _stateCoroutine = StartCoroutine(ClosingSequence());
        }
    }

    // Call this when you want to end/close the VHS
    private void StartClosingTransition()
    {
        if (_state == VHSState.Closing) return;
        SetState(VHSState.Closing);
    }

    // Public API
    public void TurnONVHS()
    {
        _VHSOn = true;
        SetState(VHSState.On);
    }

    public void TurnOFFVHS()
    {
        _VHSOn = false;
        StartClosingTransition();
    }

    private void EnableVHSOverlay()
    {
        if (_vhsCanvas != null) _vhsCanvas.SetActive(true);
        if (_clapper != null) _clapper.SetActive(true);

        ApplyOriginalVhsMat(false);
        SetVhsShaderOn(true);
    }

    private void DisableVHSOverlay()
    {
        if (_clapper != null) _clapper.SetActive(false);
        if (_vhsCanvas != null) _vhsCanvas.SetActive(false);
        SetVhsShaderOn(false);
    }

    private void StartVHS()
    {
        UpdateVolumeUI(true);
        EnsureOverlayCoroutinesRunning();
    }

    private void EnsureOverlayCoroutinesRunning()
    {
        if (_playFlickerCoroutine == null && _playButtonUI != null)
        {
            _playFlickerCoroutine = StartCoroutine(FlickeringPlayButton());
        }

        if (_timeCoroutine == null && _timerText != null && _timerSText != null && _timerMSText != null)
        {
            _timeCoroutine = StartCoroutine(TimeCoroutine());
            _timeFlickerCoroutine = StartCoroutine(FlickerText(_timerText, 0.25f, 0.5f, 0.1f, 0.25f));
            _timeSFlickerCoroutine = StartCoroutine(FlickerText(_timerSText, 0.75f, 0.15f, 0.05f, 0.15f));
            _timeMSFlickerCoroutine = StartCoroutine(FlickerText(_timerMSText, 0.002f, 0.08f, 0.002f, 1f));
        }
    }

    private void StopAllVhsCoroutines()
    {
        if (_playFlickerCoroutine != null) StopCoroutine(_playFlickerCoroutine);
        if (_timeCoroutine != null) StopCoroutine(_timeCoroutine);
        if (_timeFlickerCoroutine != null) StopCoroutine(_timeFlickerCoroutine);
        if (_timeSFlickerCoroutine != null) StopCoroutine(_timeSFlickerCoroutine);
        if (_timeMSFlickerCoroutine != null) StopCoroutine(_timeMSFlickerCoroutine);

        _playFlickerCoroutine = null;
        _timeCoroutine = null;
        _timeFlickerCoroutine = null;
        _timeSFlickerCoroutine = null;
        _timeMSFlickerCoroutine = null;
    }


    //update ui
    private void UpdateTimerValue()
    {
        if (J_GameManager.Instance != null)
        {
            _currentGameTime = J_GameManager.Instance.GetGameTime();
        }
        else
        {
            _currentGameTime = 0f;
        }
    }

    private void UpdateVolumeUI(bool force = false)
    {
        if (_volText == null) return;

        float currentVol = _volValue * 100f;
        int bar = (int)currentVol / 10;

        if (!force && _currentVolBar == bar) return;

        _currentVolBar = bar;
        _volText.text = BuildVolBar(_currentVolBar);
    }

    private string BuildVolBar(int barCount)
    {
        barCount = Mathf.Clamp(barCount, 0, 10);

        string newText = "";
        for (int i = 0; i < barCount; i++)
        {
            newText += "| ";
        }

        int addOn = 10 - barCount;
        for (int i = 0; i < addOn; i++)
        {
            newText += "- ";
        }

        return newText;
    }


    private void CacheOriginalVhsMat()
    {
        if (_vhsMat == null) return;

        _oriHDRColor = _vhsMat.GetColor("_Color");
        _oriBlurOffSet = _vhsMat.GetFloat("_blur_offset");
        _scanLineSpeed = _vhsMat.GetFloat("_scan_lines_speed");
        _numOfScanLine = _vhsMat.GetFloat("_numberOfScanLine");
        _noiseAmt = _vhsMat.GetFloat("_noiseAmount");
        _noiseSpeed = _vhsMat.GetVector("_noiseSpeed");
        _noiseScale = _vhsMat.GetFloat("_noiseScale");
    }

    private void ApplyOriginalVhsMat(bool reseting)
    {
        if (_vhsMat == null) return;

        _vhsMat.SetColor("_Color", _oriHDRColor);
        _vhsMat.SetFloat("_blur_offset", _oriBlurOffSet);
        _vhsMat.SetFloat("_scan_lines_speed", _scanLineSpeed);
        _vhsMat.SetFloat("_numberOfScanLine", _numOfScanLine);
        _vhsMat.SetFloat("_noiseAmount", _noiseAmt);
        _vhsMat.SetVector("_noiseSpeed", _noiseSpeed);
        _vhsMat.SetFloat("_noiseScale", _noiseScale);

        if (_vhsCanvas != null) _vhsCanvas.transform.localScale = Vector3.one;

        if (!reseting && _clapper != null)
        {
            _clapper.transform.localPosition = _clapperOriginalPos;
        }
        _vhsMat.SetFloat("_onVhs", 0);

    }

    private void SetVhsShaderOn(bool on)
    {
        if (_vhsMat == null) return;
        _vhsMat.SetFloat("_onVhs", on ? 1f : 0f);
    }


    private IEnumerator FlickeringPlayButton()
    {
        while (true)
        {
            Color color = _playButtonUI.color;
            color.a = Random.Range(_minAlpha, _maxAlpha);
            _playButtonUI.color = color;

            yield return new WaitForSeconds(Random.Range(_minFlickerTime, _maxFlickerTime));
        }
    }

    private IEnumerator TimeCoroutine()
    {
        while (true)
        {
            if (Random.value < 0.1f)
            {
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
                continue;
            }

            int hr = Mathf.FloorToInt(_currentGameTime / 3600f);
            int min = Mathf.FloorToInt((_currentGameTime % 3600f) / 60f);
            int s = Mathf.FloorToInt(_currentGameTime % 60f);
            int ms = Mathf.FloorToInt((_currentGameTime * 1000f) % 1000f / 10f);

            _timerText.SetText("{0:00}:{1:00}", hr, min);
            _timerSText.SetText(":{0:00}", s);
            _timerMSText.SetText(":{0:00}", ms);

            yield return new WaitForSeconds(Random.Range(0.03f, 0.12f));
        }
    }

    private IEnumerator FlickerText(TextMeshProUGUI text, float minTime, float maxTime, float minA, float maxA)
    {
        while (true)
        {
            Color color = text.color;
            color.a = Random.Range(minA, maxA);
            text.color = color;

            yield return new WaitForSeconds(Random.Range(minTime, maxTime));
        }
    }

    private IEnumerator ClosingSequence()
    {
        // Phase 1: clapper animation
        yield return StartCoroutine(ClapperSequence());

        // Phase 2: shader fade + scale up + hide
        yield return StartCoroutine(ClosingCoroutine());

        // done
        SetState(VHSState.Off);
        _VHSOn = false;

        FinishTransiition?.Invoke();
    }

    private IEnumerator ClapperSequence()
    {
        if (_clapper == null || _clapperTop == null) yield break;

        _clapper.SetActive(true);

        // move clapper backwards (z)
        float elapsed = 0f;
        Vector3 startPos = _clapper.transform.localPosition;
        Vector3 targetPos = startPos;
        targetPos.z = _stage1ZPos;

        while (elapsed < _phase1Duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / _phase1Duration);
            float curveT = Mathf.Clamp01(_moveCurveZ.Evaluate(t));

            _clapper.transform.localPosition = Vector3.Lerp(startPos, targetPos, curveT);
            yield return null;
        }

        _clapper.transform.localPosition = targetPos;
        yield return new WaitForSeconds(0.75f);

        // rotate topper up
        float targetZ = (_targetAngle < 0) ? 360 + _targetAngle : _targetAngle;
        Vector3 euler = _clapperTop.transform.localRotation.eulerAngles;

        while (Mathf.Abs(Mathf.DeltaAngle(euler.z, targetZ)) > 0.1f)
        {
            euler.z = Mathf.MoveTowardsAngle(euler.z, targetZ, _rotSpeed * Time.deltaTime);
            _clapperTop.transform.localRotation = Quaternion.Euler(euler);
            yield return null;
        }

        yield return new WaitForSeconds(0.25f);

        // rotate topper down to 0
        while (Mathf.Abs(Mathf.DeltaAngle(euler.z, 0f)) > 0.1f)
        {
            euler.z = Mathf.MoveTowardsAngle(euler.z, 0f, _rotSpeed * Time.deltaTime);
            _clapperTop.transform.localRotation = Quaternion.Euler(euler);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // move clapper up (y)
        Vector3 pos = _clapper.transform.localPosition;
        while (pos.y < _yPosTarget)
        {
            pos.y = Mathf.MoveTowards(pos.y, _yPosTarget, _moveSpeedY * Time.deltaTime);
            _clapper.transform.localPosition = pos;
            yield return null;
        }

        _clapper.SetActive(false);
    }

    private IEnumerator ClosingCoroutine()
    {
        if (_vhsCanvas == null || _vhsMat == null) yield break;

        yield return new WaitForSeconds(_closingWaitDuration);

        float elapsed = 0f;

        Vector3 startScale = _vhsCanvas.transform.localScale;
        Vector3 targetScale = Vector3.one * _maxScaleSize;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);

            _vhsCanvas.transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            _vhsMat.SetColor("_Color", Color.Lerp(_oriHDRColor, Color.white, t));
            _vhsMat.SetFloat("_blur_offset", Mathf.Lerp(_oriBlurOffSet, 0f, t));
            _vhsMat.SetFloat("_scan_lines_speed", Mathf.Lerp(_scanLineSpeed, 0f, t));
            _vhsMat.SetFloat("_numberOfScanLine", Mathf.Lerp(_numOfScanLine, 0f, t));
            _vhsMat.SetFloat("_noiseAmount", Mathf.Lerp(_noiseAmt, 0f, t));
            _vhsMat.SetVector("_noiseSpeed", Vector2.Lerp(_noiseSpeed, Vector2.zero, t));
            _vhsMat.SetFloat("_noiseScale", Mathf.Lerp(_noiseScale, 0f, t));

            yield return null;
        }

        _vhsCanvas.transform.localScale = targetScale;
        SetVhsShaderOn(false);
        _vhsCanvas.SetActive(false);
    }

    private void OnDestroy()
    {
        AudioLibrary.UpdateAudio -= UpdateCurVol;

        _VHSOn = true;
        ApplyOriginalVhsMat(true);
        StopAllCoroutines();
    }
}