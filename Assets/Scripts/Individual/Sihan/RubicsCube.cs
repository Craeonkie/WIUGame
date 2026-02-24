using UnityEngine;

public class RubicsCube : ThrowableItem
{
    [SerializeField] private GameObject cubeBlockerPrefab;
    [SerializeField] private float torqueIntensity;

    new void Start()
    {
        base.Start();
    }

    // Actually unparent and launch the item
    public override void Throw()
    {
        base.Throw();

        GetComponent<Rigidbody>()?.AddTorque(Random.insideUnitSphere.normalized * torqueIntensity, ForceMode.Impulse);
    }

    protected override void OnCollisionEnter(Collision other)
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
            GameObject blocker = Instantiate(cubeBlockerPrefab, transform.position, transform.rotation);

            if (blocker.TryGetComponent<RubicsCubeBlocker>(out RubicsCubeBlocker blockerScript))
            {
                blockerScript._smallScale = transform.localScale.x;
            }

            Destroy(gameObject);
            isInFlight = false;
        }
    }
}
