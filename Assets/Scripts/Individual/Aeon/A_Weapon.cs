using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Weapon : Item
{
    [SerializeField] protected List<Entity> hitEntities;
    [SerializeField] protected float invincibilityLength;
    protected bool isAttacking = false;
    protected bool isBlocking = false;
    protected float currentAttackDamage;

    // Update is called once per frame
    protected new void Update()
    {
        base.Update();
    }

    public void BeginAttack(float attackDamage)
    {
        isAttacking = true;
        currentAttackDamage = attackDamage;
        hitEntities.Clear();
    }

    public void EndAttack()
    {
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
            Entity thisEntity;
            if (other.gameObject.TryGetComponent<Entity>(out thisEntity))
            {
                thisEntity.TakeDamage(currentAttackDamage, invincibilityLength);
            }
            else
            {
                thisEntity = other.gameObject.GetComponentInParent<Entity>();
                if (thisEntity != null)
                {
                    thisEntity.TakeDamage(currentAttackDamage, invincibilityLength);
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
