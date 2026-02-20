using UnityEngine;
using UnityEngine.Rendering;

public class Inventory : MonoBehaviour
{
    [Header("GameObject that holds the items")]
    [SerializeField] private GameObject _rightHandSlot;
    //[SerializeField] private GameObject _leftHandSlot;

    [Header("The items")]
    [SerializeField] private GameObject _primaryItem;
    [SerializeField] private GameObject _secondaryItem;
    [SerializeField] private GameObject _currentItem;

    void Start()
    {
        _primaryItem = null;
        _secondaryItem = null;
    }

    // Highlights an object in the world
    public void HighlightObject(GameObject go)
    {

    }

    // Returns the primary item being held
    public GameObject ReturnPrimaryItem()
    {
        return _primaryItem;
    }

    // Returns the secondary item being held
    public GameObject ReturnSecondaryItem()
    {
        return _secondaryItem;
    }

    // Returns the current item being held
    public GameObject ReturnCurrentItem()
    {
        return _currentItem;
    }

    // Removes item from left hand (if applicable), then puts it into the right hand
    public void PutItemInPrimary(GameObject item, Entity entityUsingItem)
    {
        // Remove current item if any
        DropItem(_primaryItem);

        _primaryItem = item;
        item.transform.SetParent(_rightHandSlot.transform);
        item.GetComponent<Item>().PickUp(entityUsingItem);
        item.SetActive(false);
        EquipPrimary();
    }

    // Removes item from right hand (if applicable), then puts it into the left hand
    public void PutItemInSecondary(GameObject item, Entity entityUsingItem)
    {
        // Remove current item if any
        DropItem(_secondaryItem);

        _secondaryItem = item;
        item.transform.SetParent(_rightHandSlot.transform);
        item.GetComponent<Item>().PickUp(entityUsingItem);
        item.SetActive(false);
        EquipSecondary();
    }

    // Equip item in the primary slot
    public void EquipPrimary()
    {
        if (_currentItem != null)
        {
            _currentItem.SetActive(false);
        }
        _currentItem = _primaryItem;
        if (_currentItem != null)
        {
            _currentItem.SetActive(true);
        }
    }

    // Equip item in the secondary slot
    public void EquipSecondary()
    {
        if (_currentItem != null)
        {
            _currentItem.SetActive(false);
        }
        _currentItem = _secondaryItem;
        if (_currentItem != null)
        {
            _currentItem.SetActive(true);
        }
    }

    // Drops an item into the world
    public void DropItem(GameObject item)
    {
        if (item == null)
        {
            return;
        }

        // Remove item from current hand
        if (_currentItem == item)
        {
            _currentItem = null;
        }

        // Remove from primary hand
        if (item == _primaryItem)
        {
            _primaryItem = null;
        }
        // Remove from secondary hand
        else if (item == _secondaryItem)
        {
            _secondaryItem = null;
        }

        // Position slightly in front of player
        Vector3 dropPos = transform.position + transform.forward * 1.0f + Vector3.up * 0.5f;
        item.GetComponent<Item>().Drop(dropPos, Vector3.forward * 5.0f);
        item.transform.SetParent(null);
    }
}