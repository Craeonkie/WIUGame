using UnityEngine;
using UnityEngine.Rendering;

public class Inventory : MonoBehaviour
{
    [Header("GameObject that holds the items")]
    public GameObject _primarySlot;
    public GameObject _shieldSlot;
    public GameObject _throwableItemSlot;

    [Header("The items")]
    [SerializeField] private GameObject _primaryItem;
    [SerializeField] private GameObject _secondaryItem;
    [SerializeField] private GameObject _currentItem;

    public static System.Action<string, float, float> OnEquip;

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
        if (item.TryGetComponent<WeaponWithBlock>(out WeaponWithBlock thisWeapon))
        {
            item.transform.SetParent(_shieldSlot.transform);
        }
        else
        {
            item.transform.SetParent(_primarySlot.transform);
        }
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
        if (item.TryGetComponent<ThrowableItem>(out _))
        {
            item.transform.SetParent(_throwableItemSlot.transform);
        }
        else
        {
            item.transform.SetParent(_primarySlot.transform);
        }
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

            // Check item type
            string itemType = "";
            if (_currentItem.GetComponent<StandardWeapon>() || _currentItem.GetComponent<StabWeapon>()) itemType = "Weapon";
            else if (_currentItem.GetComponent<WeaponWithBlock>()) itemType = "Shield";

            var item = _currentItem.GetComponent<Item>();

            OnEquip?.Invoke(itemType, item.currentDurability, item.maxDurability);
        }
        else
        {
            OnEquip?.Invoke(null, 0, 0);
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

    // Position the item in front of the entity with this inventory after dropping it
    public void DropItem(GameObject item)
    {
        if (item != null)
        {
            RemoveItemFromInventory(item);

            // Position slightly in front of entity
            Vector3 dropPos = transform.position + transform.forward * 1.0f + Vector3.up * 0.5f;
            item.GetComponent<Item>().Drop(dropPos, Vector3.forward * 5.0f);
            item.transform.SetParent(null);

            OnEquip?.Invoke(null, 0, 0);
        }
    }

    // Remove the item from the inventory
    public void RemoveItemFromInventory(GameObject item)
    {
        if (item != null)
        {
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
        }
    }
}