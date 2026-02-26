using UnityEngine;

public class J_Pillow : Interactable
{
    [SerializeField] private Transform _stackedPillowPosition;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private BoxCollider _boundaryCollider;
    [SerializeField] private BoxCollider _bounceCollider;
    [SerializeField] private float _bounceForce;
    public static System.Action<J_Pillow> OnInteracted;
    private J_Pillow _pillowAbove;
    private bool _atDestination;

    private void OnEnable()
    {
        _atDestination = true;
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;
    }

    public void ReachDestination()
    {
        _atDestination = true;
        _rb.useGravity = true;
    }

    public void Stack(J_Pillow pillowToStack)
    {
        _pillowAbove = pillowToStack;
        pillowToStack.transform.parent = _stackedPillowPosition;
        pillowToStack.transform.localPosition = Vector3.zero;
    }

    public void RemoveFromStack() { 
        _pillowAbove.transform.parent = null;
        _pillowAbove = null;
    }

    public void GetCarried()
    {
        if (transform.parent != null)
        {
            J_Pillow pillowBelow = transform.parent.parent.GetComponent<J_Pillow>();
            pillowBelow.RemoveFromStack();
        }

        _rb.useGravity = false;
        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero;
        _boundaryCollider.isTrigger = true;
    }

    public void GetDropped()
    {
        _boundaryCollider.isTrigger = false;
        _rb.useGravity = true;
        _rb.isKinematic = false;
    }

    public void GetStacked()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        _boundaryCollider.isTrigger = false;
    }

    public bool HasPillowAbove() { return _pillowAbove != null; }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if this pillow landed on top of the game object
        if (!_atDestination && collision.gameObject.CompareTag("PlayerTag"))
        {
            if (_rb.linearVelocity.y < 0f)
                collision.gameObject.GetComponent<PlayerController>().TakeDamage(100000, 0.0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if player was the one landing on the pillow
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();

            // Check if player is falling
            if (rb.linearVelocity.y < 0f)
            {
                rb.AddForce(Vector3.up * _bounceForce, ForceMode.Impulse);
            }
        }
    }
}
