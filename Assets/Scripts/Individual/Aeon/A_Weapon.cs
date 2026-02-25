using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : Item
{
    [SerializeField] protected List<Entity> hitEntities;
    [SerializeField] protected float invincibilityLength;
    protected bool isAttacking = false;
    protected bool isBlocking = false;
    protected float currentAttackDamage;
    public static System.Action<float, float> OnDurabilityChange;

    protected new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected new void Update()
    {
        base.Update();
    }

    public void BeginAttack(float attackDamage)
    {
        isAttacking = true;
        canLoseDurabilityThisAttack = true;
        currentAttackDamage = attackDamage;
        hitEntities.Clear();
    }

    public void EndAttack()
    {
        canLoseDurabilityThisAttack = false;
        isAttacking = false;
    }

    public void BeginBlocking()
    {
        isBlocking = true;
    }

    public void EndBlocking()
    {
        isBlocking = false;
    }

    public bool IsBlocking()
    {
        return isBlocking;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isAttacking && !hitEntities.Contains(other.gameObject.GetComponent<Entity>()) && !IsPartOfHierarchy(other.transform, transform.root))
        {
            if (other.gameObject.TryGetComponent<Entity>(out Entity thisEntity))
            {
                thisEntity.TakeDamage(currentAttackDamage, invincibilityLength);
                if (canLoseDurabilityThisAttack)
                {
                    canLoseDurabilityThisAttack = false;
                    currentDurability -= _currentAnimation.durabilityUsed;
                    OnDurabilityChange?.Invoke(currentDurability, maxDurability);
                }
            }
            else
            {
                thisEntity = other.gameObject.GetComponentInParent<Entity>();
                if (thisEntity != null)
                {
                    thisEntity.TakeDamage(currentAttackDamage, invincibilityLength);
                    if (canLoseDurabilityThisAttack)
                    {
                        canLoseDurabilityThisAttack = false;
                        currentDurability -= _currentAnimation.durabilityUsed;
                        OnDurabilityChange?.Invoke(currentDurability, maxDurability);
                    }
                }
            }
            hitEntities.Add(other.gameObject.GetComponent<Entity>());
        }
    }

    // Drops an item rigidbody wise and in this case, breaks the item if it has a durability and it's too low
    public override void Drop(Vector3 dropPos, Vector3 force)
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

        if (_destroyUponDrop)
        {
            // Change tag accordingly
            tag = "Untagged";
            _hasBeenDropped = true;
            _timeBeforeDestroyed = _maxTimeBeforeDestroyed;
        }

        if (currentDurability <= 0 && hasDurability)
        {
            gameObject.SetActive(false);
        }
    }

    public override void ResetItem()
    {
        currentDurability = maxDurability;
        tag = "Weapon";
        _hasBeenDropped = false;
    }
}
