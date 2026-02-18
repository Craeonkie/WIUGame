using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : Interactable
{
    [SerializeField] private WeaponData myWeaponData;
    [SerializeField] private List<GameObject> hitEntities;
    private bool isAttacking = false;
    private float currentDurability;
    private float currentAttackDuration;
    private float currentAttackDamage;

    // Update is called once per frame
    protected void Update()
    {
        if (currentAttackDuration > 0.0f)
        {
            currentAttackDuration -= Time.deltaTime;
            if (currentAttackDuration <= 0.0f)
            {
                isAttacking = false;
                currentAttackDuration = 0.0f;
                hitEntities.Clear();
            }
        }
    }

    public void Attack(float attackDamage, float duration)
    {
        isAttacking = true;
        currentAttackDuration = duration;
        currentAttackDamage = attackDamage;
        hitEntities.Clear();
    }

    // Probably drop and break this in some way
    void Break()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (isAttacking && !hitEntities.Contains(other.gameObject))
        {
            if (other.gameObject.TryGetComponent<Entity>(out Entity thisEntity))
            {
                thisEntity.TakeDamage(currentAttackDamage);
            }
            hitEntities.Add(other.gameObject);
        }
    }

    public WeaponData ReturnWeaponData()
    {
        return myWeaponData;
    }
}
