using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]
public class J_NavMeshLinkSpline : MonoBehaviour
{
    [SerializeField] private J_Spline _splineVisualisation;
    [SerializeField] private NavMeshLink _navMeshLinkData;
    [SerializeField, Min(0.01f)] private float _heightOffset = 1;
    [SerializeField, Range(0.25f, 0.75f)] private float _placementOffset = 0.5f;


#if UNITY_EDITOR

    // Update is called once per frame
    void Update()
    {
        if (_splineVisualisation != null && _navMeshLinkData != null)
        {
            Vector3 start = transform.TransformPoint(_navMeshLinkData.startPoint);
            Vector3 end = transform.TransformPoint(_navMeshLinkData.endPoint);
            Vector3 midPointPosition = Vector3.Lerp(start, end, _placementOffset);
            midPointPosition.y += _heightOffset;
            _splineVisualisation.SetPoints(start, midPointPosition, end);
        }        
    }

#endif
}
