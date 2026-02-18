using UnityEngine;
using UnityEngine.Rendering;

public class Inventory : MonoBehaviour
{
    [Header("GameObject that holds the items")]
    [SerializeField] private GameObject _rightHandSlot;
    [SerializeField] private GameObject _leftHandSlot;

    [Header("The items")]
    [SerializeField] private GameObject _primaryItem;
    [SerializeField] private GameObject _secondaryItem;
    [SerializeField] private GameObject _currentItem;

    void Start()
    {
        _primaryItem = null;
        _secondaryItem = null;
    }

    public void InteractWith(Interactable interactableObject, AnimationHandler animationHandler)
    {
        string tag = interactableObject.tag;

        // Act according to the item's tag
        if (tag == "Weapon")
        {
            PutItemInPrimary(interactableObject.gameObject);
            animationHandler.SetItem((Item)interactableObject);
        }
        else if (tag == "Item")
        {
            PutItemInSecondary(interactableObject.gameObject);
            animationHandler.SetItem((Item)interactableObject);
        }
        else if (tag == "Interactable")
        {
            interactableObject.InteractWith();
            return;
        }
    }

    // Highlights an object in the world
    public void HighlightObject(GameObject go)
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

    // Returns the item being held
    public GameObject ReturnCurrentItem()
    {
        return _currentItem;
    }

    // Removes item from left hand or inventory slot (if applicable), then puts it into the right hand
    public void PutItemInPrimary(GameObject item)
    {
        // Remove from current location first
        DropItem(_primaryItem);

        _primaryItem = item;
        _currentItem = item;
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
        if (item == null)
        {
            return;
        }

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
}