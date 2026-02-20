using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : Item
{
    [SerializeField] protected List<GameObject> hitEntities;
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
        if (isAttacking && !hitEntities.Contains(other.gameObject) && !IsPartOfHierarchy(other.transform, transform.root))
        {
            if (other.gameObject.TryGetComponent<Entity>(out Entity thisEntity))
            {
                thisEntity.TakeDamage(currentAttackDamage);
            }
            hitEntities.Add(other.gameObject);
        }
    }
}
