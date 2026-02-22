using System.Collections;
using UnityEngine;

public class Effect
{
    public string effectName;
    public Material material;
}

public class J_EffectsManager : MonoBehaviour
{
    public static J_EffectsManager Instance;

    [Header("Dust Effect")]
    [SerializeField] private Material _material;
    [SerializeField] private float _dustIncreaseSpeed;
    [SerializeField] private float _dustDecreaseSpeed;

    private IEnumerator _increaseStrengthCoroutine;
    private IEnumerator _decreaseStrengthCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartDustEffect()
    {
        if (_increaseStrengthCoroutine != null)
        {
            StopCoroutine(_increaseStrengthCoroutine);
            _increaseStrengthCoroutine = null;
        }

        _increaseStrengthCoroutine = IncreaseStrength();
        StartCoroutine(_increaseStrengthCoroutine);
    }

    private IEnumerator IncreaseStrength()
    {
        float currentDustStrength = _material.GetFloat("_DustStrength");

        while (currentDustStrength < 1)
        {
            currentDustStrength += _dustIncreaseSpeed * Time.deltaTime;
            currentDustStrength = Mathf.Clamp01(currentDustStrength);
            _material.SetFloat("_DustStrength", currentDustStrength);
            yield return null;
        }

        Debug.Log("out");

        _material.SetFloat("_DustStrength", 1f);
        _increaseStrengthCoroutine = null;
        _decreaseStrengthCoroutine = DecreaseStrength();
        StartCoroutine(_decreaseStrengthCoroutine);
    }

    private IEnumerator DecreaseStrength()
    {
        float currentDustStrength = _material.GetFloat("_DustStrength");

        Debug.Log("here");

        while (currentDustStrength > 0)
        {
            currentDustStrength -= _dustDecreaseSpeed * Time.deltaTime;
            currentDustStrength = Mathf.Clamp01(currentDustStrength);
            _material.SetFloat("_DustStrength", currentDustStrength);
            yield return null;
        }

        _material.SetFloat("_DustStrength", 0f);
        _decreaseStrengthCoroutine = null;
    }


    private void Reset()
    {
        _material.SetFloat("_DustStrength", 0f);
    }
    private void OnDestroy()
    {
        Reset();
    }
}
