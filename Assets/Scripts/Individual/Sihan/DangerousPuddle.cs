using UnityEngine;
using System.Collections;

public class DangerousPuddle : MonoBehaviour
{
    [SerializeField] private bool _active = true;
    [SerializeField] private AnimationCurve _dissolve1Curve;
    [SerializeField] private AnimationCurve _dissolve2Curve;
    [SerializeField] private float _dissolveTime;
    [SerializeField] private float _disableThreshold;
    private float timer = 0f;
    private Material _dissolveMaterial;
    [SerializeField] private float _damage;
    [SerializeField] private float _invincibleDuration;

    private void Start()
    {
        _dissolveMaterial = GetComponent<Renderer>().material;

        _dissolveMaterial.SetVector("_Offset", new Vector2(Random.Range(0, 100), Random.Range(0, 100)));
    }

    public void Initalise(AnimationCurve dissolve1Curve, AnimationCurve dissolve2Curve, float dissolveTime, float disableThreshold, float damage, float invincibleDuration)
    {
        _dissolve1Curve = dissolve1Curve;
        _dissolve2Curve = dissolve2Curve;
        _dissolveTime = dissolveTime;
        _disableThreshold = disableThreshold;
        _damage = damage;
        _invincibleDuration = invincibleDuration;
    }

    public void Update()
    {
        timer += Time.deltaTime;

        float percentage = timer / _dissolveTime;
        percentage = Mathf.Clamp01(percentage);

        _dissolveMaterial.SetFloat("_DissolveSlider1", _dissolve1Curve.Evaluate(percentage));
        _dissolveMaterial.SetFloat("_DissolveSlider2", _dissolve2Curve.Evaluate(percentage));

        if (_active && percentage >= _disableThreshold)
            _active = false;
        else if (percentage == 1) 
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        Dog.onDisablePuddles += DisablePuddle;
    }

    private void OnDisable()
    {
        Dog.onDisablePuddles -= DisablePuddle;
    }

    private void DisablePuddle()
    {
        _active = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_active && other.CompareTag("PlayerTag"))
        {
            other.GetComponent<Entity>().TakeDamage(_damage, _invincibleDuration);
        }
    }
}
