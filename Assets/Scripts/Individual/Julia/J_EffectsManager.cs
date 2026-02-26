using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class Effect
{
    public string effectName;
    public Material material;
}

public class J_EffectsManager : MonoBehaviour
{
    public static J_EffectsManager Instance;
    public Volume GlobalVolume;

    [Header("Fire Effect")]
    [SerializeField] private Material _fireMat;
    [SerializeField] private Material _smokeMat;
    [SerializeField] private float _burnEffectSpeed = 1f; // Single speed for both
    [SerializeField] private float _fireStartValue = 10f; // Starting vignette power
    [SerializeField] private float _fireTargetValue = 3f; // Target vignette power
    private Coroutine _burnCoroutine;

    [Header("Dust Effect")]
    [SerializeField] private Material _dustMat;
    [SerializeField] private float _dustIncreaseSpeed;
    [SerializeField] private float _dustDecreaseSpeed;
    private Coroutine _dustCoroutine;

    [Header("Vignette Effect")]
    [SerializeField] private bool _setValueOnAwake;
    [SerializeField] private float _vignetteValue;
    [SerializeField] private float _transitionSpeed;
    private VignetteVolume _vignetteVolume;
    private Coroutine _vignetteCoroutine;
    public UnityEvent OnVignetteTransitionInwardStart;
    public UnityEvent OnVignetteTransitionInwardFinish;
    public UnityEvent OnVignetteTransitionOutwardStart;
    public UnityEvent OnVignetteTransitionOutwardFinish;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (GlobalVolume.profile.TryGet<VignetteVolume>(out var vignetteSetting))
        {
            _vignetteVolume = vignetteSetting;
        }

        if (_setValueOnAwake)
        {
            _vignetteVolume.radius.value = Mathf.Clamp(_vignetteValue, -2f, 2f);
        }
    }
    public void StartDustEffect()
    {
        if (_dustCoroutine != null)
        {
            StopCoroutine(_dustCoroutine);
        }
        _dustCoroutine = StartCoroutine(IncreaseDustStrength());
    }

    public void StartBurnEffect()
    {
        if (_burnCoroutine != null)
        {
            StopCoroutine(_burnCoroutine);
        }
        _burnCoroutine = StartCoroutine(IncreaseBurnEffect());
    }

    public void StopBurnEffect()
    {
        if (_burnCoroutine != null)
        {
            StopCoroutine(_burnCoroutine);
        }
        _burnCoroutine = StartCoroutine(DecreaseBurnEffect());
    }

    public void StartVignetteInwardEffect()
    {
        if (_vignetteCoroutine != null)
        {
            StopCoroutine(_vignetteCoroutine);
        }
        _vignetteCoroutine = StartCoroutine(IncreaseVignetteEffect());
    }

    public void StartVignetteOutwardEffect()
    {
        if (_vignetteCoroutine != null)
        {
            StopCoroutine(_vignetteCoroutine);
        }
        _vignetteCoroutine = StartCoroutine(DecreaseVignetteEffect());
    }

    private IEnumerator IncreaseDustStrength()
    {
        float currentDustStrength = _dustMat.GetFloat("_DustStrength");
        while (currentDustStrength < 1)
        {
            currentDustStrength += _dustIncreaseSpeed * Time.deltaTime;
            currentDustStrength = Mathf.Clamp01(currentDustStrength);
            _dustMat.SetFloat("_DustStrength", currentDustStrength);
            yield return null;
        }
        _dustMat.SetFloat("_DustStrength", 1f);

        // Auto-decrease after reaching max
        _dustCoroutine = StartCoroutine(DecreaseDustStrength());
    }

    private IEnumerator DecreaseDustStrength()
    {
        float currentDustStrength = _dustMat.GetFloat("_DustStrength");
        while (currentDustStrength > 0)
        {
            currentDustStrength -= _dustDecreaseSpeed * Time.deltaTime;
            currentDustStrength = Mathf.Clamp01(currentDustStrength);
            _dustMat.SetFloat("_DustStrength", currentDustStrength);
            yield return null;
        }
        _dustMat.SetFloat("_DustStrength", 0f);
        _dustCoroutine = null;
    }

    // COMBINED BURN EFFECT (Fire + Smoke synced)
    private IEnumerator IncreaseBurnEffect()
    {
        float currentFireStrength = _fireMat.GetFloat("_VignettePower");
        float currentSmokeStrength = _smokeMat.GetFloat("_DustStrength");

        // Calculate the normalized progress (0 to 1)
        while (currentFireStrength > _fireTargetValue)
        {
            currentFireStrength -= _burnEffectSpeed * Time.deltaTime;
            currentFireStrength = Mathf.Max(_fireTargetValue, currentFireStrength);

            // Calculate progress from fire values
            float progress = 1f - ((currentFireStrength - _fireTargetValue) / (_fireStartValue - _fireTargetValue));
            progress = Mathf.Clamp01(progress);

            // Sync smoke to the same progress
            currentSmokeStrength = progress;

            _fireMat.SetFloat("_VignettePower", currentFireStrength);
            _smokeMat.SetFloat("_DustStrength", currentSmokeStrength);

            yield return null;
        }

        // Ensure final values
        _fireMat.SetFloat("_VignettePower", _fireTargetValue);
        _smokeMat.SetFloat("_DustStrength", 1f);
        _burnCoroutine = null;
    }

    private IEnumerator DecreaseBurnEffect()
    {
        float currentFireStrength = _fireMat.GetFloat("_VignettePower");
        float currentSmokeStrength = _smokeMat.GetFloat("_DustStrength");

        while (currentFireStrength < _fireStartValue)
        {
            currentFireStrength += _burnEffectSpeed * Time.deltaTime;
            currentFireStrength = Mathf.Min(_fireStartValue, currentFireStrength);

            // Calculate progress (1 to 0 as fire returns to start)
            float progress = 1f - ((currentFireStrength - _fireTargetValue) / (_fireStartValue - _fireTargetValue));
            progress = Mathf.Clamp01(progress);

            // Sync smoke to the same progress
            currentSmokeStrength = progress;

            _fireMat.SetFloat("_VignettePower", currentFireStrength);
            _smokeMat.SetFloat("_DustStrength", currentSmokeStrength);

            yield return null;
        }

        // Ensure final values
        _fireMat.SetFloat("_VignettePower", _fireStartValue);
        _smokeMat.SetFloat("_DustStrength", 0f);
        _burnCoroutine = null;
    }

    private IEnumerator IncreaseVignetteEffect()
    {
        OnVignetteTransitionInwardStart?.Invoke();

        while (_vignetteVolume.radius.value > _vignetteVolume.radius.min)
        {
            _vignetteVolume.radius.value -= _transitionSpeed * Time.deltaTime;
            yield return null;
        }

        _vignetteVolume.radius.value = _vignetteVolume.radius.min;

        OnVignetteTransitionInwardFinish?.Invoke();
        _vignetteCoroutine = null;
    }

    private IEnumerator DecreaseVignetteEffect()
    {

        OnVignetteTransitionOutwardStart?.Invoke();

        while (_vignetteVolume.radius.value < _vignetteVolume.radius.max)
        {
            _vignetteVolume.radius.value += _transitionSpeed * Time.deltaTime;
            yield return null;
        }

        _vignetteVolume.radius.value = _vignetteVolume.radius.max;

        OnVignetteTransitionOutwardFinish?.Invoke();
        _vignetteCoroutine = null;
    }


    private void Reset()
    {
        if (_dustMat != null)
            _dustMat.SetFloat("_DustStrength", 0f);

        if (_fireMat != null)
            _fireMat.SetFloat("_VignettePower", _fireStartValue);

        if (_smokeMat != null)
            _smokeMat.SetFloat("_DustStrength", 0f);
    }

    private void OnDestroy()
    {
        Reset();
    }
}