using UnityEditor.Experimental.GraphView;
using UnityEngine;

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

    public static System.Action<Item> OnEquipPrimary, OnEquipSecondary, OnEquipShield;

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
        if (item.TryGetComponent<WeaponWithBlock>(out _))
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
        // UI EVENT CALL
        // SCENARIO 1 --> EQUIP PRIMARY ONLY, BECAUSE SHIELD ICON DOESN'T CHANGE
        // Check if current item is SHIELD
        if (_currentItem != null && _currentItem != _secondaryItem && _primaryItem != null)
        {
            Debug.Log("Scenario P1, Item name: " + _primaryItem.name);

            // Check if it was NOT a shield
            if (_currentItem.GetComponent<WeaponWithBlock>() == null)
            {
                Debug.Log("Equipped!");
                OnEquipPrimary?.Invoke(_currentItem.GetComponent<Item>());
            }
            else
            {
                OnEquipShield?.Invoke(_primaryItem.GetComponent<Item>());
            }
        }
        // SCENARIO 2 --> EQUIP PRIMARY AND SWAP OUT PRIMARY AND SECONDARY IN UI
        else if (_currentItem != null && _currentItem == _secondaryItem && _primaryItem != null)
        {
            Debug.Log("Scenario P2, Primary Item name: " + _primaryItem.name);
            Debug.Log("Scenario P2, Current Item name: " + _currentItem.name);
            Debug.Log("Scenario P2, Secondary Item name: " + _secondaryItem.name);

            // Check if it was NOT a shield
            if (_primaryItem.GetComponent<WeaponWithBlock>() == null)
            {
                Debug.Log("Equipped!");
                OnEquipSecondary?.Invoke(_secondaryItem.GetComponent<Item>());
                OnEquipPrimary?.Invoke(_primaryItem.GetComponent<Item>());
            }
            else
            {
                OnEquipShield?.Invoke(_primaryItem.GetComponent<Item>());
            }
            // ELSE DO NOTHING
        }
        // SCENARIO 3 --> EQUIP PRIMARY ONLY, BECAUSE NO WEAPONS ARE ON HAND
        else if (_currentItem == null && _primaryItem != null)
        {
            Debug.Log("Scenario P3, Item name: " + _primaryItem.name);

            // Check if it was NOT a shield
            if (_primaryItem.GetComponent<WeaponWithBlock>() == null)
            {
                Debug.Log("Equipped!");
                Debug.Log("Primary item has item component: " + _primaryItem.GetComponent<Item>());
                OnEquipPrimary?.Invoke(_primaryItem.GetComponent<Item>());
                if (_secondaryItem == null)
                    OnEquipSecondary?.Invoke(null);
                else
                    OnEquipSecondary?.Invoke(_secondaryItem.GetComponent<Item>());
            }
            else
            {
                OnEquipShield?.Invoke(_primaryItem.GetComponent<Item>());
            }
        }
        // SCENARIO 4 --> EQUIP PRIMARY, BECAUSE CURRENT ITEM IS NULL
        else if (_currentItem == _secondaryItem && _currentItem != null && _primaryItem == null)
        {
            OnEquipSecondary?.Invoke(_secondaryItem.GetComponent<Item>());
            OnEquipPrimary?.Invoke(null);
        }

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
        // UI EVENT CALL
        // SCENARIO 1: CURRENT ITEM IS EMPTY, BUT SECONDARY ITEM EXISTS
        if (_currentItem == null && _secondaryItem != null && _primaryItem == null)
        {
            Debug.Log("Scenario S1, Item Name: " + _secondaryItem.name);

            // Check if the secondary item ISN'T a shield item
            if (_secondaryItem.GetComponent<WeaponWithBlock>() == null)
            {
                Debug.Log("Equipped!");
                OnEquipPrimary?.Invoke(_secondaryItem.GetComponent<Item>()); // SWAP IT TO THE PRIMARY SLOT
                OnEquipSecondary?.Invoke(null);
            }
        }
        // SCENARIO 2: CURRENT ITEM IS EITHER THE PRIMARY ITEM OR SHIELD AND SECONDARY ITEM IS NOT THE CURRENT ITEM AND EXISTS
        else if (_currentItem != null && _secondaryItem != null && _currentItem != _secondaryItem)
        {
            Debug.Log("Scenario S2, Item Name: " + _secondaryItem.name);

            // CHECK IF CURRENT ITEM IS NOT A SHIELD
            if (_currentItem.GetComponent<WeaponWithBlock>() == null)
            {
                Debug.Log("Equipped!");
                OnEquipPrimary?.Invoke(_secondaryItem.GetComponent<Item>());
                OnEquipSecondary?.Invoke(_primaryItem.GetComponent<Item>());
            }
            else
            {
                Debug.Log("Equipped!");
                OnEquipPrimary?.Invoke(_secondaryItem.GetComponent<Item>());
                OnEquipSecondary?.Invoke(null);
                OnEquipShield?.Invoke(_primaryItem.GetComponent<Item>());
            }
        }

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
        }
    }

    // Remove the item from the inventory
    public void RemoveItemFromInventory(GameObject item)
    {
        Debug.Log("remove called!");

        if (item != null)
        {
            Debug.Log("went in here");

            // Remove item from current hand
            if (_currentItem == item)
            {
                // Check if it's the shield
                if (_currentItem.GetComponent<WeaponWithBlock>())
                {
                    Debug.Log("removing shield");
                    OnEquipShield?.Invoke(null);
                }
                else if (_currentItem == _primaryItem)
                {
                    if (_primaryItem.GetComponent<WeaponWithBlock>() != null)
                    {
                        OnEquipShield?.Invoke(null);
                    }
                    else
                    {
                        OnEquipPrimary?.Invoke(null);
                    }

                    if (_secondaryItem != null)
                    {
                        OnEquipSecondary?.Invoke(_secondaryItem.GetComponent<Item>());
                    }
                }
                else if (_currentItem == _secondaryItem)
                {
                    Debug.Log("dropping a throwable");
                    Debug.Log("primary: " + _primaryItem);
                    Debug.Log("secondary: " + _secondaryItem);

                    OnEquipPrimary?.Invoke(null);
                    if (_primaryItem != null && _primaryItem.GetComponent<WeaponWithBlock>() == null)
                        OnEquipSecondary?.Invoke(_primaryItem.GetComponent<Item>());
                }

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