using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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

    [Header("Durability")]
    public bool hasDurability;
    public int maxDurability;
    public int currentDurability;
    protected bool canLoseDurabilityThisAttack = false;

    [Header("Energy")]
    [SerializeField] protected bool consumesEnergy;

    [Header("Item Holding Offset")]
    public Vector3 offset;

    [Header("Audio")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip breakingSound;
    [SerializeField] protected AudioClip onHitSound;

    [Header("Exposed for debugging")]
    [SerializeField] protected AnimationHandler _animationHandler;
    public Entity _entityUsingItem;

    protected InputType _inputType;
    protected Animation _currentAnimation;

    [SerializeField] protected bool _isActing;
    [SerializeField] protected bool _resetAnimationChain;
    [SerializeField] protected bool _chainingAnimation;
    [SerializeField] protected int _currentAnimationChain;

    [Header("Destroy on drop parameters")]
    [SerializeField] protected bool isUsedByObjectPool = false;
    [SerializeField] protected bool _destroyUponDrop;
    [SerializeField] protected float _maxTimeBeforeDestroyed;
    [SerializeField] protected bool _expiresAfterDropped;
    [SerializeField] protected float _maxTimeBeforeExpiring;
    protected float _timeBeforeDestroyed;
    protected bool _hasBeenDropped;

    // Update is called once per frame
    protected virtual void Start()
    {
        if (hasDurability)
        {
            currentDurability = maxDurability;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (_hasBeenDropped)
        {
            _timeBeforeDestroyed -= Time.deltaTime;
            if (_timeBeforeDestroyed <= 0)
            {
                if (isUsedByObjectPool && J_SpawnManager.Instance != null)
                    J_SpawnManager.Instance.Release(itemName, gameObject);
                else
                    gameObject.SetActive(false);
            }
        }
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
    public virtual void PickUp(Entity entityUsingItem)
    {
        transform.SetLocalPositionAndRotation(Vector3.zero + offset, Quaternion.identity);
        SetEntity(entityUsingItem);

        // Disable physics
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        for (int i = 0; i < GetComponents<Collider>().Length; i++)
        {
            GetComponents<Collider>()[i].isTrigger = true;
        }
    }

    // Drops an item rigidbody wise
    public virtual void Drop(Vector3 dropPos, Vector3 force)
    {
        // Unparent
        transform.SetParent(null);
        _currentAnimationChain = 0;
        _isActing = false;
        EndAction();
        // Make animation handler stop equipping it, then stop referencing it
        _animationHandler.StopReferencingOldItem();
        SetAnimationHandler(null);
        SetEntity(null);
        transform.gameObject.SetActive(true);

        // Enable physics
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        for (int i = 0; i < GetComponents<Collider>().Length; i++)
        {
            GetComponents<Collider>()[i].isTrigger = false;
        }

        // Position and add force to the item
        transform.position = dropPos;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(force, ForceMode.Impulse);

        if (_destroyUponDrop)
        {
            // Change tag accordingly
            tag = "Untagged";
            _hasBeenDropped = true;
            _timeBeforeDestroyed = _maxTimeBeforeDestroyed;
        }

        if (currentDurability <= 0 && hasDurability)
        {
            if (isUsedByObjectPool && J_SpawnManager.Instance != null)
                J_SpawnManager.Instance.Release(itemName, gameObject);
            else
                gameObject.SetActive(false);
        }
    }

    // Set current animation handler (Upon pickup, drop, equip or unequip)
    public virtual void SetAnimationHandler(AnimationHandler handler)
    {
        _animationHandler = handler;
        _currentAnimationChain = 0;
        _isActing = false;
        EndAction();
    }

    // Set current entity wielding this (Upon pickup or drop)
    protected virtual void SetEntity(Entity entity)
    {
        _entityUsingItem = entity;
    }

    protected virtual bool IsPartOfHierarchy(Transform target, Transform root)
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

    // For the item manager to call
    public virtual void ResetItem()
    {
        currentDurability = maxDurability;
        tag = "Item";
        _hasBeenDropped = false;
    }

    // Set item to false
    public virtual void Break()
    {
        if (breakingSound != null)
        {
            if (breakingSound != null)
            {
                AudioLibrary.Instance.PlaySoundAtPointCustom(breakingSound.name, transform.position);
            }
        }

        if (isUsedByObjectPool && J_SpawnManager.Instance != null)
            J_SpawnManager.Instance.Release(itemName, gameObject);
        else
            gameObject.SetActive(false);
    }

    // Ensure player calls this, then calls drop and break on the item
    public virtual bool CheckIfBroken()
    {
        if (currentDurability <= 0 && hasDurability)
        {
            return true;
        }
        return false;
    }
}