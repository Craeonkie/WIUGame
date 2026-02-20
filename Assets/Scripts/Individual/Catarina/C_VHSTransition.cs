using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class C_VHSTransition : MonoBehaviour
{
    [Header("Flickering")]
    [SerializeField] private Image _playButtonUI;
    [SerializeField] private float _minFlickerTime = 0.05f;
    [SerializeField] private float _maxFlickerTime = 0.15f;
    [SerializeField] private float _minAlpha = 0.3f;
    [SerializeField] private float _maxAlpha = 1f;
    private Coroutine flickerCoroutine;

    [Header("Volume")]
    [SerializeField] private TextMeshProUGUI _volText;
    //NEED TO CHANGE THIS VAR NEED TO USE THE GAME VOL VALUE N NOT THIS 
    [SerializeField][Range(0, 1)] private float _volValue = 1;
    private int _currentVolBar;

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _timerSText;
    [SerializeField] private TextMeshProUGUI _timerMSText;
    private float _currentGameTime = 0;
    private Coroutine _timeCoroutine;
    private Coroutine _timeFlickerCoroutine;
    private Coroutine _timeSFlickerCoroutine;
    private Coroutine _timeMSFlickerCoroutine;

    [Header("Close Transition Data")]
    [SerializeField] private bool _VHSOn = true;
    [SerializeField] private GameObject _vhsCanvas;
    [SerializeField] private float _maxScaleSize = 3;
    [SerializeField] private float _closingWaitDuration = .75f;
    [SerializeField] private float _duration = 1f;
    //[SerializeField] private GameObject _volume;
    [SerializeField] private Material _vhsMat;
    private bool _finishClosingTransition = false;

    private float _oriBlurOffSet;
    private Color _oriHDRColor;
    private float _scanLineSpeed;
    private float _numOfScanLine;
    private float _noiseAmt;
    private Vector2 _noiseSpeed;
    private float _noiseScale;

    private Coroutine _closingCoroutine;

    private Coroutine _startCoroutine;

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
    private void Start()
    {

        if (_volText != null)
        {
            float currentVol = _volValue * 100;

            _currentVolBar = (int)currentVol / 10;
            string newText = "";
            for (int i = 0; i < _currentVolBar; i++)
            {
                newText += "| ";
            }
            if (_currentVolBar < 10)
            {
                int AddOn = 10 - _currentVolBar;
                for (int i = 0; i < AddOn; i++)
                {
                    newText += "- ";
                }
            }
            _volText.text = newText;
        }
        if (_vhsCanvas != null)
        {
            _oriHDRColor = _vhsMat.GetColor("_Color");
            _oriBlurOffSet = _vhsMat.GetFloat("_blur_offset");
            _scanLineSpeed = _vhsMat.GetFloat("_scan_lines_speed");
            _numOfScanLine = _vhsMat.GetFloat("_numberOfScanLine");
            _noiseAmt = _vhsMat.GetFloat("_noiseAmount");
            _noiseSpeed = _vhsMat.GetVector("_noiseSpeed");
            _noiseScale = _vhsMat.GetFloat("_noiseScale");
            if (!_VHSOn)
            {
                StopAllCoroutines();
                _timeCoroutine = null;
                _timeFlickerCoroutine = null;
                _timeSFlickerCoroutine = null;
                _timeMSFlickerCoroutine = null;
                flickerCoroutine = null;
                _finishClosingTransition = true;
            }
            else
            {
                _startCoroutine = StartCoroutine(StartOfClosingTransition());
            }
        }
        if (_clapper != null)
        {
            _clapperOriginalPos = _clapper.transform.localPosition;
        }
    }

    private void Update()
    {
        _currentGameTime += Time.deltaTime;
        if (!_VHSOn) return;
        if (_timeCoroutine==null)
        {
            _timeCoroutine = StartCoroutine(TimeCoroutine());
            _timeFlickerCoroutine = StartCoroutine(FlickerText(_timerText, 0.25f, 0.5f, 0.1f, 0.25f));
            _timeSFlickerCoroutine = StartCoroutine(FlickerText(_timerSText, 0.75f, 0.15f, 0.05f, 0.15f));
            _timeMSFlickerCoroutine = StartCoroutine(FlickerText(_timerMSText, 0.002f, 0.08f, 0.002f, 1));
        }
        if (flickerCoroutine == null)
        {
            flickerCoroutine = StartCoroutine(FlickeringText());
        }
        //doing the calculation of the volume
        if (_volText != null)
        {
            float currenVol = _volValue * 100;
            int currentBar = (int)currenVol / 10;
            if (_currentVolBar != currentBar)
            {
                _currentVolBar = currentBar;
                string newText = "";
                for (int i = 0; i < _currentVolBar; i++)
                {
                    newText += "| ";
                }
                if (_currentVolBar < 10)
                {
                    int AddOn = 10 - _currentVolBar;
                    for (int i = 0; i < AddOn; i++)
                    {
                        newText += "- ";
                    }
                }
                _volText.text = newText;
            }
        }

        if (!_VHSOn && !_finishClosingTransition)
        {
            if (_vhsCanvas != null)
            {
                if (_startCoroutine != null)
                {
                    StopCoroutine(_startCoroutine);
                    _startCoroutine = null;
                }
                if (_closingCoroutine != null)
                {
                    StopCoroutine(_closingCoroutine);
                    _closingCoroutine = null;
                }
                _startCoroutine = StartCoroutine(StartOfClosingTransition());
            }
            _finishClosingTransition = true;
        }
        if (_VHSOn && _finishClosingTransition)
        {
            if (_vhsCanvas != null)
            {
                ResetVHSEffect(false);
                _vhsCanvas.SetActive(true);
                _clapper.SetActive(true);
            }

            if (_closingCoroutine != null)
            {
                StopCoroutine(_closingCoroutine);
                _closingCoroutine = null;
            }
            if (_startCoroutine != null)
            {
                StopCoroutine(_startCoroutine);
                _startCoroutine = null;
            }
            _startCoroutine = StartCoroutine(StartOfClosingTransition());

            _finishClosingTransition = false;
        }
    }

    private void ResetVHSEffect(bool reseting)
    {
        if (!_VHSOn) return;
        _vhsMat.SetColor("_Color", _oriHDRColor);
        _vhsMat.SetFloat("_blur_offset", _oriBlurOffSet);
        _vhsMat.SetFloat("_scan_lines_speed", _scanLineSpeed);
        _vhsMat.SetFloat("_numberOfScanLine", _numOfScanLine);
        _vhsMat.SetFloat("_noiseAmount", _noiseAmt);
        _vhsMat.SetVector("_noiseSpeed", _noiseSpeed);
        _vhsMat.SetFloat("_noiseScale", _noiseScale);
        _vhsMat.SetFloat("_onVhs", 1);
        _vhsCanvas.transform.localScale = Vector3.one;
        if (!reseting)
        {
            _clapper.transform.localPosition = _clapperOriginalPos;
        }
    }

    private IEnumerator FlickeringText()
    {
        while (true)
        {
            //get the currernt obj color then randomise the new color alpha
            Color color = _playButtonUI.color;
            color.a = Random.Range(_minAlpha, _maxAlpha);

            _playButtonUI.color = color;
            float waitTime = Random.Range(_minFlickerTime, _maxFlickerTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator TimeCoroutine()
    {
        while (true)
        {
            //random freeze frame
            if (Random.value < 0.1f)
            {
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
                continue;
            }

            //Debug.Log("_current time"+_currentGameTime);
            int hr = Mathf.FloorToInt(_currentGameTime / 3600f);
            int min = Mathf.FloorToInt((_currentGameTime % 3600f) / 60f);
            int s = Mathf.FloorToInt(_currentGameTime % 60f);
            int ms = Mathf.FloorToInt((_currentGameTime * 1000f) % 1000f / 10f);

            _timerText.SetText("{0:00}:{1:00}", hr, min);
            _timerSText.SetText(":{0:00}", s);
            _timerMSText.SetText(":{0:00}", ms);

            // make the update rate irregular
            yield return new WaitForSeconds(Random.Range(0.03f, 0.12f));
        }
    }

    private IEnumerator FlickerText(TextMeshProUGUI _text, float _minTime, float _maxTime, float _minA, float _maxA)
    {
        while (true)
        {
            //get the currernt obj color then randomise the new color alpha
            Color color = _text.color;
            color.a = Random.Range(_minA, _maxA);

            _text.color = color;
            float waitTime = Random.Range(_minTime, _maxTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator ClosingCoroutine()
    {
        yield return new WaitForSeconds(_closingWaitDuration);

        float elapsed = 0f;

        Vector3 startScale = _vhsCanvas.transform.localScale;
        Vector3 targetScale = Vector3.one * _maxScaleSize;
        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / _duration);

            // Smooth scale
            _vhsCanvas.transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            //fade the shader out too
            _vhsMat.SetColor("_Color", Color.Lerp(_oriHDRColor, Color.white, t));
            _vhsMat.SetFloat("_blur_offset", Mathf.Lerp((float)_oriBlurOffSet, 0f, t));
            _vhsMat.SetFloat("_scan_lines_speed", Mathf.Lerp(_scanLineSpeed, 0f, t));
            _vhsMat.SetFloat("_numberOfScanLine", Mathf.Lerp(_numOfScanLine, 0f, t));
            _vhsMat.SetFloat("_noiseAmount", Mathf.Lerp(_noiseAmt, 0f, t));
            _vhsMat.SetVector("_noiseSpeed", Vector2.Lerp(_noiseSpeed, Vector2.zero, t));
            _vhsMat.SetFloat("_noiseScale", Mathf.Lerp(_noiseScale, 0, t));

            yield return null;
        }

        _vhsCanvas.transform.localScale = targetScale;
        _vhsMat.SetFloat("_onVhs", 0);
        //_volume.SetActive(false);
        _vhsCanvas.SetActive(false);
        _VHSOn = false;

    }

    private IEnumerator StartOfClosingTransition()
    {
        if (_clapper == null || _clapperTop == null) yield return null;

        // zoom out then open then close then up
        // switch cases then yea
        _clapper.SetActive(true);

        //move clapper backwards
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

        // move topper up
        float targetZ = (_targetAngle < 0) ? 360 + _targetAngle : _targetAngle;
        Vector3 euler = _clapperTop.transform.localRotation.eulerAngles;
        while (Mathf.Abs(Mathf.DeltaAngle(euler.z, targetZ)) > 0.1f)
        {
            euler.z = Mathf.MoveTowardsAngle(euler.z, targetZ, _rotSpeed * Time.deltaTime);
            _clapperTop.transform.localRotation = Quaternion.Euler(euler);
            yield return null;
        }

        // small pause
        yield return new WaitForSeconds(0.25f);
        // move topper down to 0
        while (Mathf.Abs(Mathf.DeltaAngle(euler.z, 0f)) > 0.1f)
        {
            euler.z = Mathf.MoveTowardsAngle(euler.z, 0f, _rotSpeed * Time.deltaTime);
            _clapperTop.transform.localRotation = Quaternion.Euler(euler);
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        var pos = _clapper.transform.localPosition;

        // move clapper up
        while (pos.y < _yPosTarget)
        {
            pos.y = Mathf.MoveTowards(pos.y, _yPosTarget, _moveSpeedY * Time.deltaTime);
            _clapper.transform.localPosition = pos;
            yield return null;
        }

        //end
        _clapper.SetActive(false);
        _startCoroutine = null;

        //start 2nd phase of closing coroutine
        if (_closingCoroutine != null)
        {
            StopCoroutine(_closingCoroutine);
            _closingCoroutine = null;
        }
        _closingCoroutine = StartCoroutine(ClosingCoroutine());
    }
    private void OnDestroy()
    {
        _VHSOn = true;
        ResetVHSEffect(true);
        StopAllCoroutines();
    }
}