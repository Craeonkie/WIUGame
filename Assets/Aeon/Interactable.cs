using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    // This should be something that goes into an item manager instead...
    // Spawns object in regardless of whether or not it is already in the scene
    public void Spawn(Vector3 newPosition)
    {
        gameObject.SetActive(true);
        transform.position = newPosition;
    }

    // Respawns object if it's not in the scene
    public void SpawnIfNotInScene(Vector3 newPosition)
    {
        if (!gameObject.activeSelf)
        {
            Spawn(newPosition);
        }
    }

    // Function to run when the item is interacted with
    public void InteractWith(GameObject handSlot)
    {

    }

    // An item's primary use
    public void PrimaryUse()
    {

    }

    // An item's secondary use
    public void SecondaryUse()
    {

    }

    // An item's special use
    public void SpecialUse()
    {

    }

    // Picks up an item rigidbody wise and appends it to the player's hand slot
    public void PickUp(GameObject _handSlot)
    {
        // Parent to hand slot
        transform.SetParent(_handSlot.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

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

        // Launch it potentially
    }
}
