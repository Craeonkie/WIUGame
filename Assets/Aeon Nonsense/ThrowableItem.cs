using System.Collections.Generic;
using UnityEngine;

public class ThrowableItem : Item
{
    [SerializeField] protected float throwPowerForward = 10.0f;
    [SerializeField] protected float throwPowerUp = 10.0f;
    [SerializeField] protected float throwDistanceForward = 1.0f;
    [SerializeField] protected bool breakOnHit = false;
    [SerializeField] protected float lifeTime = 10.0f;
    protected float lifeTimeLeft;

    [SerializeField] protected List<GameObject> hitEntities;
    [SerializeField] protected bool isAiming = false;
    [SerializeField] protected bool isThrowing = false;
    [SerializeField] protected bool isInFlight = false;
    AnimatorStateInfo animatorStateInfo;

    // Update is called once per frame
    protected new void Update()
    {
        base.Update();

        // Runs when animation handler is done acting but own isActing is still true, running only once
        if (isThrowing && _animationHandler != null && _animationHandler.ReturnAnimator().GetCurrentAnimatorStateInfo(0).normalizedTime > 0.45f && _animationHandler.ReturnAnimator().GetCurrentAnimatorStateInfo(0).IsName("Throw") && _isActing)
        {
            Throw();
            _isActing = false;
        }

        if (isInFlight)
        {
            if (lifeTimeLeft > 0)
            {
                lifeTimeLeft -= Time.deltaTime;
                if (lifeTimeLeft <= 0)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    // Try to perform action
    public override void TryToAct(InputType inputType, bool isBeingHeld, bool wasPressedThisFrame)
    {
        if (inputType == InputType.Primary && !isThrowing && !isInFlight)
        {
            // Input was released
            if (isAiming && isBeingHeld == false)
            {
                // Throw the item and release it at the end of the animation
                PerformThrowAction();
            }
            // Input was just pressed
            if (wasPressedThisFrame)
            {
                isAiming = true;
                // Aiming is handled on player end
                //PerformAction();
            }
            return;
        }
    }

    //// Performing priming
    //public override void PerformAction()
    //{
    //    // Set Animator
    //    _animationHandler.PerformAction(primary[0]);
    //    _isActing = true;
    //}

    // Performing throw action
    public void PerformThrowAction()
    {
        // Set Animator
        _animationHandler.PerformAction(primary[0]);
        _isActing = true;
        isThrowing = true;
    }

    // Actually unparent and launch the item
    public void Throw()
    {
        isInFlight = true;
        lifeTimeLeft = lifeTime;

        // Unparent
        transform.SetParent(null);
        _currentAnimationChain = 0;
        _isActing = false;
        EndAction();

        // Enable physics
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        if (TryGetComponent<Collider>(out Collider col))
        {
            col.isTrigger = false;
        }

        // Position and add force to the item
        transform.position += _entityUsingItem.transform.forward * throwDistanceForward;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(_entityUsingItem.transform.forward * throwPowerForward, ForceMode.Impulse);
        rb.AddForce(_entityUsingItem.transform.up * throwPowerUp, ForceMode.Impulse);

        // Make animation handler stop equipping it, then stop referencing it
        _animationHandler.UnequipItemButFinishAnimation();
        _animationHandler = null;
        _entityUsingItem = null;
    }

    protected void OnCollisionEnter(Collision other)
    {
        if (isInFlight && !hitEntities.Contains(other.gameObject) && !IsPartOfHierarchy(other.transform, transform.root))
        {
            if (other.gameObject.TryGetComponent<Entity>(out Entity thisEntity))
            {
                thisEntity.TakeDamage(primary[0].damage);
            }
            hitEntities.Add(other.gameObject);
        }
        if (isInFlight && breakOnHit)
        {
            gameObject.SetActive(false);
        }
    }
}
