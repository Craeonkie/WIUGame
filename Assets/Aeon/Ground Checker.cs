using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [Header("Ground Detection")]
    [SerializeField] private float _sphereCastHeight = 0.08f;
    [SerializeField] private float _extraSphereCastHeight = 0.2f;
    [SerializeField] private LayerMask groundLayers;

    [SerializeField] private GameObject _player;
    [SerializeField] private float _radius;
    [SerializeField] private bool _isGrounded;
    [SerializeField] private bool _isCloseToGround;
    private Vector3 _groundNormal = Vector3.up;

    private void Update()
    {
        // Calculate spherecast origin at bottom center of capsule
        Vector3 origin = transform.position;

        if (Physics.SphereCast(origin + Vector3.up, _radius, Vector3.down, out RaycastHit hit, _sphereCastHeight + _extraSphereCastHeight + 1, groundLayers))
        {
            _isCloseToGround = true;
            _groundNormal = hit.normal;
            if (hit.distance <= _sphereCastHeight + 1)
            {
                _isGrounded = true;
            }
            else
            {
                _isGrounded = false;
            }
        }
        else
        {
            _isGrounded = false;
            _isCloseToGround = false;
            _groundNormal = Vector3.up;
        }
    }

    public bool IsGrounded() => _isGrounded;

    public bool IsAlmostGrounded() => _isCloseToGround;

    public Vector3 GetGroundNormal() => _groundNormal;

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;

        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(origin + Vector3.down * _sphereCastHeight, _radius);
        Gizmos.DrawLine(origin, origin + Vector3.down * _sphereCastHeight);
        Gizmos.DrawWireSphere(origin + Vector3.down * (_sphereCastHeight + _extraSphereCastHeight), _radius);
    }
}
