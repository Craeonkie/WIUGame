using UnityEngine;

public class J_Spline : MonoBehaviour
{
    [SerializeField] private Transform _start, _middle, _end;

    [SerializeField] private bool _showGizmos = true;

    private Vector3 CalculatePosition(float value01, Vector3 startPos, Vector3 midPos, Vector3 endPos)
    {
        value01 = Mathf.Clamp01(value01);
        Vector3 startMiddle = Vector3.Lerp(startPos, midPos, value01);
        Vector3 middleEnd = Vector3.Lerp(midPos, endPos, value01);
        return Vector3.Lerp(startMiddle, middleEnd, value01);
    }

    public Vector3 CalculatePosition(float interpolationAmount01)
    {
        return CalculatePosition(interpolationAmount01, _start.position, _middle.position, _end.position);
    }

    public Vector3 CalculatePositionCustomStart(float interpolationAmount01, Vector3 startPosition)
    {
        return CalculatePosition(interpolationAmount01, startPosition, _middle.position, _end.position);
    }

    public Vector3 CalculatePositionCustomEnd(float interpolationAmount01, Vector3 endPosition)
    {
        return CalculatePosition(interpolationAmount01, _start.position, _middle.position, endPosition);
    }

    public void SetPoints(Vector3 startPoint, Vector3 midPointPosition, Vector3 endPoint)
    {
        if (_start == null || _middle == null || _end == null)
            return;

        _start.position = startPoint;
        _middle.position = midPointPosition;
        _end.position = endPoint;
    }

    private void OnDrawGizmos()
    {
        if (_showGizmos && _start != null && _middle != null && _end != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_start.position, 0.1f);
            Gizmos.DrawSphere(_middle.position, 0.1f);
            Gizmos.DrawSphere(_end.position, 0.1f);
            Gizmos.color = Color.magenta;

            int granularity = 5;

            for (int i = 0; i < granularity; ++i)
            {
                Vector3 startPt = i == 0 ? _start.position : CalculatePosition(i / (float)granularity);
                Vector3 endPt = i == granularity ? _end.position : CalculatePosition((i + 1) / (float)(granularity));

                Gizmos.DrawLine(startPt, endPt);
            }
        }
    }
}
