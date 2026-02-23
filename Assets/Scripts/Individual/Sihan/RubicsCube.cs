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
        base.OnCollisionEnter(other);

        if (isInFlight)
        {
            GameObject blocker = Instantiate(cubeBlockerPrefab, transform.position, transform.rotation);

            if (blocker.TryGetComponent<RubicsCubeBlocker>(out RubicsCubeBlocker blockerScript))
            {
                blockerScript._smallScale = transform.localScale.x;
            }
        }
    }
}
