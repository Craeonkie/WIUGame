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
    public void PickUp(Entity entityUsingItem)
    {
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _entityUsingItem = entityUsingItem;

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
    public void Drop(Vector3 dropPos, Vector3 force)
    {
        // Unparent
        transform.SetParent(null);
        _currentAnimationChain = 0;
        _isActing = false;
        EndAction();
        // Make animation handler stop equipping it, then stop referencing it
        _animationHandler.UnequipItem();
        _animationHandler = null;
        _entityUsingItem = null;

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
        transform.position = dropPos;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(force, ForceMode.Impulse);
    }

    // Set current animation handler (Upon pickup, drop, equip or unequip)
    public void SetAnimationHandler(AnimationHandler handler)
    {
        _animationHandler = handler;
        _currentAnimationChain = 0;
        _isActing = false;
        EndAction();
    }

    // Set current entity wielding this (Upon pickup or drop)
    public void SetEntity(Entity entity)
    {
        _entityUsingItem = entity;
    }

    protected bool IsPartOfHierarchy(Transform target, Transform root)
    {
        Transform current = root;
        while (current != null)
        {
            if (current == target)
            {
                return true;
            }
            current = current.parent;
        }

        return target.IsChildOf(root);
    }
}