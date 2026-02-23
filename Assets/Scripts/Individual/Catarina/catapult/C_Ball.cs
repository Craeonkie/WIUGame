using UnityEngine;

public class C_Ball : MonoBehaviour
{
    bool hasBeenPickedUp = false;
   
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private bool _isGhost;
    [SerializeField] private float _DestroyLag = 0f;
    public void Init(Vector3 velocity, bool isGhost)
    {
        _isGhost = isGhost;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        _rb.linearVelocity = velocity;
    }
    public void OnCollisionEnter(Collision col)
    {
        if (_isGhost || !hasBeenPickedUp) return;
        //can spawn particle effect audio here
        Destroy(this.gameObject, _DestroyLag);
    }
}
