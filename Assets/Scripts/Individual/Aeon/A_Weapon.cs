using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : Item
{
    [SerializeField] protected List<Entity> hitEntities;
    [SerializeField] protected float invincibilityLength;
    protected bool isAttacking = false;
    protected bool isBlocking = false;
    protected float currentAttackDamage;

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

    public void BlockDamage()
    {
        currentDurability -= _currentAnimation.durabilityUsedByAttacking;
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
                    currentDurability -= _currentAnimation.durabilityUsedByAttacking;
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
                        currentDurability -= _currentAnimation.durabilityUsedByAttacking;
                    }
                }
            }
            hitEntities.Add(other.gameObject.GetComponent<Entity>());
        }
    }

    public override void ResetItem()
    {
        currentDurability = maxDurability;
        tag = "Weapon";
        _hasBeenDropped = false;
    }
}
