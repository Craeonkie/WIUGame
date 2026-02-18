using System.Collections;
using UnityEngine;

public class J_Damageable : MonoBehaviour
{
    [SerializeField] private float _health = 100f;
    private float _currentHealth;
    [SerializeField] private RectTransform _greenBar;
    private float _maxWidth;

    // For damage effects
    private Color _originalColor;
    public Color DamageColor = Color.red;
    public float DamageEffectDuration = 0.5f;
    public float IFramesDuration = 0.5f; // this is only for effects caused NOT by player
    public bool ShouldDestroy = true;
    private bool _isInvincible;
    private Renderer _objectRenderer;
    private Coroutine _damageCoroutine;

    public System.Action<Vector2> OnHit;
    public System.Action<Vector2> OnDead;

    private void Start()
    {
        _objectRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (_objectRenderer != null)
            _originalColor = _objectRenderer.material.color;

        _currentHealth = _health;

        _maxWidth = _greenBar.sizeDelta.x;
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        float healthPercentage = Mathf.Clamp01(_currentHealth / _health);
        float newLeft = Mathf.Lerp(0.5f, _maxWidth, healthPercentage);
        _greenBar.offsetMax = new Vector2(-newLeft, _greenBar.offsetMax.y);

        // Trigger colour change effect
        if (_objectRenderer != null)
        {
            // Stop any existing colour change effect to stop stacking
            if (_damageCoroutine != null)
                StopCoroutine(_damageCoroutine);

            _damageCoroutine = StartCoroutine(DamageEffect());
        }

        if (_currentHealth < 0)
            Destroy();
    }

    public void TakeExternalDamage(Vector2 dir, float amount)
    {
        if (_isInvincible || _currentHealth < 0)
            return;

        _currentHealth -= amount;
        float healthPercentage = Mathf.Clamp01(_currentHealth / _health);
        float newLeft = Mathf.Lerp(0f, _maxWidth, healthPercentage);
        _greenBar.sizeDelta = new Vector2(newLeft, _greenBar.sizeDelta.y);

        // Trigger colour change effect
        if (_objectRenderer != null)
        {
            Debug.Log("Colour change effect called!");

            // Stop any existing colour change effect to stop stacking
            if (_damageCoroutine != null)
                StopCoroutine(_damageCoroutine);

            _damageCoroutine = StartCoroutine(DamageEffect());
        }

        if (_currentHealth < 0)
        {
            if (ShouldDestroy)
                Destroy();
            else
                OnDead?.Invoke(dir);
        }
        else
        {
            _isInvincible = true;
            StartCoroutine(IFramesTimer());

            OnHit?.Invoke(dir);
        }
    }

    public void Destroy()
    {
        Debug.Log(gameObject.name + " has died.");
        Destroy(gameObject);
    }

    private IEnumerator DamageEffect()
    {
        // Set to damage colour instantly
        _objectRenderer.material.color = DamageColor;

        // Gradually transition back to the original colour overtime
        float _elapsedTime = 0f;
        while (_elapsedTime < DamageEffectDuration)
        {
            _objectRenderer.material.color = Color.Lerp(DamageColor, _originalColor, _elapsedTime / DamageEffectDuration);
            _elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final colour is reset to the original colour
        _objectRenderer.material.color = _originalColor;
    }

    private IEnumerator IFramesTimer()
    {
        yield return new WaitForSeconds(IFramesDuration);
        _isInvincible = false;
    }
}
