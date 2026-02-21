using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class LerpObject {
    [SerializeField] public Transform Destination;
    [SerializeField] public float Duration = 1f;
    [SerializeField] public bool Enabled = true;
    public AnimationCurve animCurve;
    public UnityEvent OnReach;
}

public class J_TransformAnimation : MonoBehaviour
{
    private enum ANIMATIONTYPE
    {
        INDIVIDUAL,
        CONTINUOUS
    }

    private enum LERPTYPE 
    {
        LINEAR,
        ARC
    }

    //[SerializeField] private RectTransform m_CanvasTransform;

    [Header("Components")]
    [SerializeField] private GameObject _objectToLerp;
    [SerializeField] private LerpObject[] _waypoints;

    private Vector3 _originalPos, _startPos, _originalScale, _startScale;
    private Quaternion _originalRotation, _startRotation;

    [Header("Settings")]
    [SerializeField] private bool _lerpOnStart = true;
    private bool _hasAnimationStarted = false;
    private bool _destinationReached = false;
    [SerializeField] private bool _repeat = false;
    [SerializeField] private ANIMATIONTYPE _animationType = ANIMATIONTYPE.CONTINUOUS;
    [SerializeField] private LERPTYPE _lerpType = LERPTYPE.LINEAR;
    [SerializeField] private bool _isUI = false;
    [SerializeField] private bool _lerpTranslate = false;
    [SerializeField] private bool _lerpScale = false;
    [SerializeField] private bool _lerpRotation = false;

    private bool _hasAnimationEnded = false;
    private int _currentWayptIndex = 0;
    private float _elapsedTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // This script exists on the object to be lerped itself
        if (_objectToLerp.transform == null)
            _objectToLerp = gameObject;

        if (_lerpOnStart)
            EnableLerp();

        if (_isUI)
        {
            _originalPos = _objectToLerp.transform.localPosition;
            _startPos = _objectToLerp.transform.localPosition;

            _originalScale = _objectToLerp.transform.localScale;
            _startScale = _objectToLerp.transform.localScale;

            _originalRotation = _objectToLerp.transform.localRotation;
            _startRotation = _objectToLerp.transform.localRotation;
        }
        else
        {
            _originalPos = _objectToLerp.transform.position;
            _startPos = _objectToLerp.transform.position;

            _originalScale = _objectToLerp.transform.localScale;
            _startScale = _objectToLerp.transform.localScale;

            _originalRotation = _objectToLerp.transform.rotation;
            _startRotation = _objectToLerp.transform.rotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the animation has ended or if it has not started
        if (_hasAnimationEnded || !_hasAnimationStarted) return;

        if (_animationType == ANIMATIONTYPE.CONTINUOUS || (_animationType == ANIMATIONTYPE.INDIVIDUAL && !_destinationReached))
            Lerp();
    }

    private void Lerp()
    {
        // safety check
        if (_currentWayptIndex >= _waypoints.Length)
        {
            Debug.Log("ur code didnt prevent this from running");
            return;
        }

        // Check if should lerp to this destination
        if (!_waypoints[_currentWayptIndex].Enabled)
        {
            _currentWayptIndex++;
            if (_currentWayptIndex == _waypoints.Length)
                _currentWayptIndex = 0;

            return;
        }
        
        float t = _elapsedTime / _waypoints[_currentWayptIndex].Duration;
        t = _waypoints[_currentWayptIndex].animCurve.Evaluate(t);

        // Lerp
        switch (_lerpType)
        {
            case LERPTYPE.LINEAR:
                if (_isUI)
                {
                    if (_lerpTranslate)
                        _objectToLerp.transform.localPosition = Vector2.Lerp(_startPos, _waypoints[_currentWayptIndex].Destination.localPosition, t);

                    if (_lerpScale)
                        _objectToLerp.transform.localScale = Vector3.Lerp(_startScale, _waypoints[_currentWayptIndex].Destination.localScale, t);

                    if (_lerpRotation)
                        _objectToLerp.transform.localRotation = Quaternion.Lerp(_startRotation, _waypoints[_currentWayptIndex].Destination.localRotation, t);
                }
                else
                {
                    if (_lerpTranslate)
                        _objectToLerp.transform.position = Vector2.Lerp(_startPos, _waypoints[_currentWayptIndex].Destination.position, t);

                    if (_lerpScale)
                        _objectToLerp.transform.localScale = Vector3.Lerp(_startScale, _waypoints[_currentWayptIndex].Destination.localScale, t);

                    if (_lerpRotation)
                        _objectToLerp.transform.rotation = Quaternion.Lerp(_startRotation, _waypoints[_currentWayptIndex].Destination.rotation, t);
                }
                break;
            case LERPTYPE.ARC:
                // Offset the start and end positions from the center of all points
                var center = CalculateCenter();
                var relativeStartPos = _startPos - center;
                Vector3 endPos;
                if (_isUI)
                {
                    endPos = _waypoints[_currentWayptIndex].Destination.localPosition;
                }
                else
                {
                    endPos = _waypoints[_currentWayptIndex].Destination.position;
                }

                var relativeEndPos = endPos - center;

                if (_isUI)
                {
                    if (_lerpTranslate)
                        _objectToLerp.transform.localPosition = Vector3.Slerp(relativeStartPos, relativeEndPos, t);

                    if (_lerpScale)
                        _objectToLerp.transform.localScale = Vector3.Lerp(_startScale, _waypoints[_currentWayptIndex].Destination.localScale, t);

                    if (_lerpRotation)
                        _objectToLerp.transform.localRotation = Quaternion.Lerp(_startRotation, _waypoints[_currentWayptIndex].Destination.localRotation, t);
                }
                else
                {
                    if (_lerpTranslate)
                        _objectToLerp.transform.position = Vector3.Slerp(relativeStartPos, relativeEndPos, t);

                    if (_lerpScale)
                        _objectToLerp.transform.localScale = Vector3.Lerp(_startScale, _waypoints[_currentWayptIndex].Destination.localScale, t);

                    if (_lerpRotation)
                        _objectToLerp.transform.rotation = Quaternion.Lerp(_startRotation, _waypoints[_currentWayptIndex].Destination.rotation, t);
                }


                break;
        }        

        // Update time
        _elapsedTime += Time.deltaTime;

        // Check if time has passed the duration it should take to reach destination
        if (_elapsedTime >= _waypoints[_currentWayptIndex].Duration)
        {
            // Set object's position to destination
            if (_isUI)
            {
                _startPos = _waypoints[_currentWayptIndex].Destination.localPosition;
                _startScale = _waypoints[_currentWayptIndex].Destination.localScale;
                _startRotation = _waypoints[_currentWayptIndex].Destination.localRotation;

                _objectToLerp.transform.localPosition = _startPos;
                _objectToLerp.transform.localScale = _startScale;
                _objectToLerp.transform.localRotation = _startRotation;
            }
            else
            {
                _startPos = _waypoints[_currentWayptIndex].Destination.position;
                _startScale = _waypoints[_currentWayptIndex].Destination.localScale;
                _startRotation = _waypoints[_currentWayptIndex].Destination.rotation;
                
                _objectToLerp.transform.position = _startPos;
                _objectToLerp.transform.localScale = _startScale;
                _objectToLerp.transform.rotation = _startRotation;
            }

            // Invoke event
            _waypoints[_currentWayptIndex].OnReach.Invoke();

            // Set to next destination
            _currentWayptIndex++;

            _destinationReached = true;

            // Reset time
            _elapsedTime = 0f;
        }

        // Repeat lerp
        if (_currentWayptIndex == _waypoints.Length && _repeat)
            _currentWayptIndex = 0;
        else if (_currentWayptIndex == _waypoints.Length)
            _hasAnimationEnded = true;

        // Check whether to stop lerping
        if (_destinationReached && _animationType == ANIMATIONTYPE.INDIVIDUAL)
            _hasAnimationStarted = false;
    }

    Vector3 CalculateCenter() {
    
        Vector3 total = Vector3.zero;
        foreach (LerpObject pt in _waypoints)
        {
            if (_isUI)
                total += pt.Destination.localPosition;
            else
                total += pt.Destination.position;
        }

        return total / _waypoints.Length;
    }

    public void EnableLerp()
    {
        _destinationReached = false;
        _hasAnimationStarted = true;
    }

    public void ResetLerp()
    {
        _elapsedTime = 0f;
        _currentWayptIndex = 0;
        _hasAnimationEnded = false;
        _objectToLerp.transform.position = _originalPos;
        _objectToLerp.transform.localScale = _originalScale;

        if (_isUI)
            _objectToLerp.transform.localRotation = _originalRotation;
        else
            _objectToLerp.transform.rotation = _originalRotation;

        if (_animationType == ANIMATIONTYPE.CONTINUOUS)
            EnableLerp();
    }

    // || DEBUGGING FUNCTIONS

    //private void OnDrawGizmos()
    //{
    //    float size;
    //    size = _isUI ? 20f : 0.1f;

    //    if (_isUI)
    //        Gizmos.matrix = m_CanvasTransform.localToWorldMatrix;

    //    foreach (var pt in _waypoints)
    //    {
    //        if (_isUI)
    //        {
    //            Gizmos.DrawSphere(pt.Destination.localPosition, size);

    //            foreach (var point in EvaluateSlerpPoints(_startPos, pt.Destination.localPosition, CalculateCenter(), 15))
    //            {
    //                Gizmos.DrawSphere(point, size);
    //            }
    //        }
    //        else
    //        {
    //            Gizmos.DrawSphere(pt.Destination.position, size);

    //            foreach (var point in EvaluateSlerpPoints(_startPos, pt.Destination.position, CalculateCenter(), 15))
    //            {
    //                Gizmos.DrawSphere(point, size);
    //            }
    //        }
    //    }

    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(CalculateCenter(), size);
    //}

    IEnumerator Delay(float delay)
    {
        yield return new WaitForSeconds(delay);
    }

    IEnumerable<Vector3> EvaluateSlerpPoints(Vector3 start, Vector3 end, Vector3 center, int count = 10)
    {
        var startRelativeCenter = start - center;
        var endRelativeCenter = end - center;

        var f = 1f / count;

        for (var i = 0f; i < 1 + f; i += f)
        {
            yield return Vector3.Slerp(startRelativeCenter, endRelativeCenter, i) + center;
        }
    }
}
