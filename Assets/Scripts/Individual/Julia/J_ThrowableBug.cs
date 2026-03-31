using System.Collections;
using UnityEngine;

public class J_ThrowableBug : MonoBehaviour
{
    [Header("Bug Settings")]
    [SerializeField] private float _durationBeforeDestroy;
    private IEnumerator _destroyAfterDurationCoroutine;

    private void OnEnable()
    {
        StopAllCoroutines();

        //_destroyAfterDurationCoroutine = DestroyAfterDuration(_durationBeforeDestroy);
        //StartCoroutine(_destroyAfterDurationCoroutine);
    }

    public void StopDestroyCountdown()
    {
        StopCoroutine(_destroyAfterDurationCoroutine);
        _destroyAfterDurationCoroutine = null;
    }

    public void StartDestroyCountdown()
    {
        _destroyAfterDurationCoroutine = DestroyAfterDuration(_durationBeforeDestroy);
        StartCoroutine(_destroyAfterDurationCoroutine);
    }

    private void DestroyImmediately()
    {
        //J_SpawnManager.Instance.Release("ThrowableBug", gameObject);
        J_SpawnManager2.Instance.ReleaseItem("ThrowableBug", gameObject);
    }

    private IEnumerator DestroyAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        DestroyImmediately();
    }
}
