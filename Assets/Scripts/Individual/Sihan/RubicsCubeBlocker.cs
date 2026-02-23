using System.Collections;
using UnityEngine;

public class RubicsCubeBlocker : MonoBehaviour
{
    [SerializeField] private bool _cube = true;
    public float _smallScale;
    [SerializeField] private float _growMultiplier;
    [SerializeField] private AnimationCurve _growCurve;
    [SerializeField] private AnimationCurve _shrinkCurve;
    [SerializeField] private float _scaleSpeed;
    [SerializeField] private float _solveSpeed;

    public void Start()
    {
        StartCoroutine(RubicsCubeUpdate());

        if (TryGetComponent<Animator>(out Animator animator))
        {
            animator.speed = _solveSpeed;
        }
    }

    public void BegoneCube()
    {
        _cube = false;
    }

    public IEnumerator RubicsCubeUpdate()
    {
        yield return MakeCubeBig();

        while (_cube)
        {
            yield return null;
        }

        yield return MakeCubeSmall();
    }

    public IEnumerator MakeCubeBig()
    {
        yield return null;

        float elapsedTime = 0f;

        while (_cube && elapsedTime < 1)
        {
            elapsedTime += Time.deltaTime * _scaleSpeed;
            elapsedTime = Mathf.Clamp01(elapsedTime);

            transform.localScale = Vector3.one * _smallScale * _growMultiplier * Mathf.Clamp(_growCurve.Evaluate(elapsedTime), 0, Mathf.Infinity) + Vector3.one * _smallScale;
            yield return null;
        }

        GetComponent<Rigidbody>().isKinematic = false;
    }

    public IEnumerator MakeCubeSmall()
    {
        yield return null;

        float elapsedTime = 0f;
        float localScale = transform.localScale.x;

        while (elapsedTime < 1)
        {
            elapsedTime += Time.deltaTime * _scaleSpeed;
            elapsedTime = Mathf.Clamp01(elapsedTime);

            transform.localScale = Vector3.one * localScale * Mathf.Clamp(_shrinkCurve.Evaluate(elapsedTime), 0, Mathf.Infinity);
            yield return null;
        }

        Destroy(gameObject);
    }
}
