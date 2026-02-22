using UnityEngine;

public class J_ManualGravity : MonoBehaviour
{
    private CapsuleCollider _capsuleCollider;
    private Rigidbody _rb;
    [SerializeField] private float _gravityStrength = 9.81f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        J_BossBehaviour.OnTransportPlayer += ChangeGravity;
    }

    private void OnDisable()
    {
        J_BossBehaviour.OnTransportPlayer -= ChangeGravity;
    }

    private void FixedUpdate()
    {
        if (_capsuleCollider == null)
            return;

        Vector3 colliderCenter = _capsuleCollider.transform.TransformPoint(_capsuleCollider.center);

        // Direction from capsule center to player (this is the "up" direction for the player)
        Vector3 gravityUp = (_rb.transform.position - colliderCenter).normalized;
        Vector3 gravityDown = -gravityUp;

        // Apply strong gravity toward capsule center
        _rb.AddForce(gravityDown * _gravityStrength * _rb.mass, ForceMode.Force);
    }   

    private void ChangeGravity(CapsuleCollider capsuleCollider)
    {
        _capsuleCollider = capsuleCollider;
       

        if (capsuleCollider != null)
        {
            _rb.useGravity = false;
        }
    }
}
