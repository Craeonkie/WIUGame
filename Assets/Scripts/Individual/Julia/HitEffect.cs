using System.Collections;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    private Renderer _objectRenderer;
    private Color _originalColor;
    [SerializeField] private Color _colourHit = Color.white;
    [SerializeField] private float _hitDuration;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _objectRenderer = GetComponent<Renderer>();
        if (_objectRenderer != null)
            _originalColor = _objectRenderer.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleHitColour()
    {
        StopAllCoroutines();
        StartCoroutine(ColourChange());
    }

    IEnumerator ColourChange()
    {
        _objectRenderer.material.color = _colourHit;

        // Gradually transition back to the original colour overtime
        float _elapsedTime = 0f;
        while (_elapsedTime < _hitDuration)
        {
            _objectRenderer.material.color = Color.Lerp(_colourHit, _originalColor, _elapsedTime / _hitDuration);
            _elapsedTime += Time.deltaTime;
            yield return null;
        }

        _objectRenderer.material.color = _originalColor;
    }
}
