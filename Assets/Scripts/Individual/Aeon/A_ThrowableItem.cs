using System.Collections.Generic;
using UnityEngine;

public class ThrowableItem : Item
{
    [SerializeField] protected float throwPowerForward = 10.0f;
    [SerializeField] protected float throwPowerUp = 10.0f;
    [SerializeField] protected Vector3 throwOffsetForward = new Vector3(0.0f, 0.0f, 1.0f);
    [SerializeField] protected bool breakOnHit = false;
    [SerializeField] protected float lifeTime = 10.0f;
    [SerializeField] protected float invincibilityTimeApplied = 0.0f;
    protected float lifeTimeLeft;

    [SerializeField] protected List<Entity> hitEntities;
    [SerializeField] protected bool isAiming = false;
    [SerializeField] protected bool isThrowing = false;
    [SerializeField] protected bool isInFlight = false;
    AnimatorStateInfo animatorStateInfo;

    [SerializeField] protected bool _OneTimeUse = true;

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

    protected void FixedUpdate()
    {
        // Simulate trajectory
        if (isAiming)
        {
            //_projection.SimulateTrajectory(_entityUsingItem.transform.forward * throwPowerForward + _entityUsingItem.transform.up * throwPowerUp);
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

        // Append the item to the player's hand instead
        if (_entityUsingItem.TryGetComponent<PlayerController>(out PlayerController player))
        {
            transform.SetParent(player.GetComponent<Inventory>()._primarySlot.transform);
            transform.SetLocalPositionAndRotation(Vector3.zero + offset, Quaternion.identity);
        }
    }

    // Actually unparent and launch the item
    public virtual void Throw()
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
        if (!(PlayerController)_entityUsingItem || ((PlayerController)_entityUsingItem).IsTopDownCameraInUse())
        {
            transform.position += _entityUsingItem.transform.right * throwOffsetForward.x + _entityUsingItem.transform.up * throwOffsetForward.y +_entityUsingItem.transform.forward * throwOffsetForward.z;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(_entityUsingItem.transform.forward * throwPowerForward + _entityUsingItem.transform.up * throwPowerUp, ForceMode.Impulse);
        }
        else
        {
            transform.position += ((PlayerController)_entityUsingItem).cameraTarget.transform.right * throwOffsetForward.x + ((PlayerController)_entityUsingItem).cameraTarget.transform.up * throwOffsetForward.y + ((PlayerController)_entityUsingItem).cameraTarget.transform.forward * throwOffsetForward.z;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(((PlayerController)_entityUsingItem).cameraTarget.transform.forward * throwPowerForward + ((PlayerController)_entityUsingItem).cameraTarget.transform.up * throwPowerUp, ForceMode.Impulse);
        }

        // Make the player's inventory stop referencing this item
        if (_entityUsingItem.TryGetComponent<PlayerController>(out PlayerController player))
        {
            player.GetComponent<Inventory>().RemoveItemFromInventory(gameObject);
        }

        // Make animation handler stop equipping it, then stop referencing it
        _animationHandler.StopReferencingItemButFinishAnimation();
        SetAnimationHandler(null);
        SetEntity(null);

        // Change tag accordingly
        if (_OneTimeUse)
            tag = "Untagged";
    }

    public void ChangeOneTimeUse(bool _IsOneTimeUse)
    {
        _OneTimeUse = _IsOneTimeUse;
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        if (isInFlight && !hitEntities.Contains(other.gameObject.GetComponent<Entity>()) && !IsPartOfHierarchy(other.transform, transform.root))
        {
            Entity thisEntity;
            if (other.gameObject.TryGetComponent<Entity>(out thisEntity))
            {
                thisEntity.TakeDamage(primary[0].damage, invincibilityTimeApplied);
            }
            else
            {
                thisEntity = other.gameObject.GetComponentInParent<Entity>();
                if (thisEntity != null)
                {
                    thisEntity.TakeDamage(primary[0].damage, invincibilityTimeApplied);
                }
            }
            hitEntities.Add(other.gameObject.GetComponent<Entity>());
        }

        if (isInFlight && breakOnHit)
        {
            if (isUsedByObjectPool && J_SpawnManager2.Instance != null)
                J_SpawnManager2.Instance.ReleaseItem(itemName, gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}