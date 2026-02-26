using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class J_UIWithDuration : MonoBehaviour
{
    [SerializeField] private float _duration;
    [SerializeField] private bool _playOnAwake;
    [SerializeField] private GameObject[] _affectedUIObjects;
    public UnityEvent OnTrigger, OnEnd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_playOnAwake)
            StartCoroutine(DisableAfterDelay());
    }

    public void TriggerEffect()
    {
        StopAllCoroutines();
        StartCoroutine(DisableAfterDelay());
    }

    private IEnumerator DisableAfterDelay()
    {
        for (int i = 0; i < _affectedUIObjects.Length; i++)
        {
            _affectedUIObjects[i].SetActive(true);
        }

        OnTrigger?.Invoke();

        yield return new WaitForSeconds(_duration);

        for (int i = 0; i < _affectedUIObjects.Length; i++)
        {
            _affectedUIObjects[i].SetActive(false);
        }

        OnEnd?.Invoke();
    }
}
