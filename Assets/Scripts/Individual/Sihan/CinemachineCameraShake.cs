using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CinemachineCameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource _impulseSource;
    [SerializeField] private float _shakeIntensity;
    [SerializeField] private float _shakeInterval;
    [SerializeField] private float _lerpTime;
    private Coroutine _shakeCoroutine;
    private float _intensity;

    public void ShakeCamera(bool shake)
    {
        if (shake)
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }

            _shakeCoroutine = StartCoroutine(ShakeCoroutine(_shakeIntensity));
        }
        else
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }

            _shakeCoroutine = StartCoroutine(ShakeCoroutine(0));
        }
    }

    private IEnumerator ShakeCoroutine(float target)
    {
        float elapsedTime = 0f;
        float shakeTime = 0f;
        float startIntensity = _intensity;
        float targetIntensity = target;

        while (true)
        {
            shakeTime += Time.deltaTime;
            elapsedTime += Time.deltaTime;
            _intensity = Mathf.Lerp(startIntensity, targetIntensity, Mathf.Clamp01(elapsedTime / _lerpTime));

            if (shakeTime >= _shakeInterval)
            {
                shakeTime = 0f;

                _impulseSource.DefaultVelocity = Random.insideUnitSphere.normalized;
                _impulseSource.GenerateImpulse(_intensity);

                if (targetIntensity == 0 && _intensity == 0)
                    yield break;
            }

            yield return null;
        }
    }

    public void ShakeCamera(float intensity)
    {
        _impulseSource.DefaultVelocity = Random.insideUnitSphere.normalized;
        _impulseSource.GenerateImpulse(intensity);
    }
}
