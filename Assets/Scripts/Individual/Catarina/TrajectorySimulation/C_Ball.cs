using UnityEngine;

public class C_Ball : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private bool _isGhost;
    public void Init(Vector3 velocity, bool isGhost)
    {
        _isGhost = isGhost;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        _rb.linearVelocity = velocity;
    }
    public void OnCollisionEnter(Collision col)
    {
        if (_isGhost) return;
        //can spawn particle effect audio here

    }
}
