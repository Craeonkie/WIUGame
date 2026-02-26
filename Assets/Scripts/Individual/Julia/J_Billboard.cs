using UnityEngine;
public class J_Billboard : MonoBehaviour
{
    [SerializeField] private bool _reverseDirection = false;
    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_mainCamera == null)
            return;

        Vector3 directionToCamera = _mainCamera.transform.position - transform.position;
        directionToCamera.y = 0;

        if (directionToCamera.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_reverseDirection ? -directionToCamera : directionToCamera);
            transform.rotation = targetRotation;
        }
    }
}