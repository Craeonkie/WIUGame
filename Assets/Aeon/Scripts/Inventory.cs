using UnityEngine;
using UnityEngine.Rendering;

public class Inventory : MonoBehaviour
{
    [Header("GameObject that holds the items")]
    [SerializeField] private GameObject _rightHandSlot;
    [SerializeField] private GameObject _leftHandSlot;

    [Header("Item Pickup Properties")]
    [SerializeField] private LayerMask interactablesLayer;
    [SerializeField] private float _pickupConeRadius;
    [SerializeField] private float _pickupRange;

    [SerializeField] private GameObject _primaryItem;
    [SerializeField] private GameObject _secondaryItem;

    private bool pickUpItem;
    private bool dropItem;

    void Start()
    {
        _primaryItem = null;
        _secondaryItem = null;
    }

    void Update()
    {
        // Cast a sphere around the player (or use a raycast forward if preferred)
        Collider[] hits = Physics.OverlapSphere(transform.position, _pickupRange, interactablesLayer);

        GameObject closest = null;
        Interactable closestInteractable = null;
        float closestDist = _pickupRange;

        foreach (Collider col in hits)
        {
            bool alreadyHolding = false;

            if (col.gameObject == _primaryItem || col.gameObject == _secondaryItem)
            {
                alreadyHolding = true;
            }

            if (alreadyHolding)
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, col.transform.position);
            float angle = Vector3.Angle(transform.forward, col.transform.position - transform.position);
            if (dist <= closestDist && angle <= _pickupConeRadius && col.gameObject.TryGetComponent<Interactable>(out closestInteractable))
            {
                closestDist = dist;
                closest = col.gameObject;
            }
        }

        if (closest != null)
        {
            HighlightObject(closestInteractable.gameObject);
            if (pickUpItem)
            {
                InteractWith(closestInteractable);
            }
        }

        if (dropItem)
        {
            if (_primaryItem != null)
            {
                DropItem(_primaryItem);
            }
        }

        pickUpItem = false;
        dropItem = false;
    }

    void InteractWith(Interactable interactableObject)
    {
        string tag = interactableObject.tag;

        // Act according to the item's tag
        if (tag == "Weapon")
        {
            PutItemInPrimary(interactableObject.gameObject);
        }
        else if (tag == "Item")
        {
            PutItemInSecondary(interactableObject.gameObject);
        }
        else if (tag == "Interactable")
        {
            interactableObject.InteractWith();
            return;
        }
    }

    // Highlights an object in the world
    void HighlightObject(GameObject go)
    {

    }

    // Returns the primary item being held
    public GameObject ReturnCurrentPrimaryItem()
    {
        return _primaryItem;
    }

    // Returns the secondary item being held
    public GameObject ReturnCurrentSecondaryItem()
    {
        return _secondaryItem;
    }

    // Removes item from left hand or inventory slot (if applicable), then puts it into the right hand
    public void PutItemInPrimary(GameObject item)
    {
        // Remove from current location first
        DropItem(_primaryItem);

        _primaryItem = item;
        item.transform.SetParent(_rightHandSlot.transform);
        item.GetComponent<Item>().PickUp();
        item.SetActive(true);
    }

    // Removes item from right hand or inventory slot (if applicable), then puts it into the left hand
    public void PutItemInSecondary(GameObject item)
    {
        // Remove from current location first
        DropItem(_secondaryItem);

        _secondaryItem = item;
        item.transform.SetParent(_leftHandSlot.transform);
        item.GetComponent<Item>().PickUp();
        item.SetActive(true);
    }

    // Drops an item into the world
    public void DropItem(GameObject item)
    {
        // Remove from primary hand
        if (item == _primaryItem)
        {
            _primaryItem.GetComponent<Item>().Drop();
            _primaryItem.transform.SetParent(null);
            _primaryItem = null;
        }
        // Remove from secondary hand
        else if (item == _secondaryItem)
        {
            _secondaryItem.GetComponent<Item>().Drop();
            _secondaryItem.transform.SetParent(null);
            _secondaryItem = null;
        }

        // Position slightly in front of player
        Vector3 dropPos = transform.position + transform.forward * 1.0f + Vector3.up * 0.5f;
        item.transform.SetParent(null);
        item.transform.position = dropPos;
        item.transform.rotation = Quaternion.identity;

        if (item.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(Vector3.up * 3.0f, ForceMode.Impulse);
        }
    }

    public void TryToInteract()
    {
        pickUpItem = true;
    }

    public void TryToDropItem()
    {
        dropItem = true;
    }
}