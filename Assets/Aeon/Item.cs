using System.Collections.Generic;
using UnityEngine;

public abstract class Item : Interactable
{
    public string itemName;

    [Header("Image that will be displayed in UI")]
    public Sprite image;

    [Header("Description")]
    public string description;

    [Header("(Ensure Animation clips are named the same as their states!)")]
    [Header("Primary")]
    public Animation[] primary;

    [Header("Secondary")]
    public Animation[] secondary;

    [Header("Special")]
    public Animation[] special;

    protected AnimationHandler _animationHandler;
    protected Entity _entityUsingItem;

    protected InputType _inputType;
    protected Animation _currentAnimation;

    [SerializeField] protected bool _isActing;
    [SerializeField] protected bool _resetAnimationChain;
    [SerializeField] protected bool _chainingAnimation;
    [SerializeField] protected int _currentAnimationChain;

    // Update is called once per frame
    protected void Update()
    {

    }

    // Called by animation handler
    public virtual void TryToAct(InputType inputType, bool isBeingHeld, bool wasPressedThisFrame)
    {

    }

    // Start an animation
    public virtual void PerformAction()
    {

    }

    // End an animation (usually goes back to idle)
    public virtual void EndAction()
    {
        
    }

    // Runs every update to follow up the ongoing action
    protected virtual void HandleActionEnd()
    {

    }

    // Picks up an item rigidbody wise and appends it to the player's hand slot
    public void PickUp()
    {
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        // Disable physics
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        if (TryGetComponent<Collider>(out Collider col))
        {
            col.isTrigger = true;
        }
    }

    // Drops an item rigidbody wise
    public void Drop()
    {
        // Parent to hand slot
        transform.SetParent(null);
        _currentAnimationChain = 0;
        _isActing = false;
        EndAction();
        _animationHandler = null;
        _entityUsingItem = null;

        // Disable physics
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        if (TryGetComponent<Collider>(out Collider col))
        {
            col.isTrigger = false;
        }
    }

    // Set current animation handler (usually upon pickup)
    public void SetAnimationHandler(AnimationHandler handler)
    {
        _animationHandler = handler;
    }

    // Set current entity wielding this (usually upon pickup)
    public void SetEntity(Entity entity)
    {
        _entityUsingItem = entity;
    }
}