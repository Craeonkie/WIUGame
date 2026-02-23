using UnityEngine;

public class J_Pillow : Interactable
{
    [SerializeField] private Transform _stackedPillowPosition;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private BoxCollider _collider;
    public static System.Action<J_Pillow> OnInteracted;
    private J_Pillow _pillowAbove;
    private bool _atDestination;

    private void OnEnable()
    {
        _atDestination = true;
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;
    }

    private void OnDisable()
    {
        
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
        _collider.isTrigger = true;
    }

    public void GetDropped()
    {
        _collider.isTrigger = false;
        _rb.useGravity = true;
        _rb.isKinematic = false;
    }

    public void GetStacked()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        _collider.isTrigger = false;
    }

    public bool HasPillowAbove() { return _pillowAbove != null; }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_atDestination && collision.gameObject.CompareTag("PlayerTag"))
        {
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(100000, 0.0f);
        }

        // TODO: DESTORY WEAPONS AND SHIELDS THAT ARE COLLIDED WITH BY THIS PILLOW UNLESS YOU WANT THE PLAYER TO MANUALLY MOVE THE PILLOW THEMSELVES
        // OR: PUSH THE WEAPONS AND SHIELDS ON TOP OF THE PILLOW (parent it)
    }
}
