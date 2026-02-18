using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField] private PlayerInput playerInput;
    private InputAction _interactAction;

    [Header("GameObject that holds the items")]
    [SerializeField] private GameObject _handSlot;

    [Header("Item Pickup Properties")]
    [SerializeField] private LayerMask interactablesLayer;
    [SerializeField] private float _pickupConeRadius;
    [SerializeField] private float _pickupRange;

    private GameObject _weaponSlot;
    private GameObject _itemSlot;
    private GameObject _holding;

    void Start()
    {
        _interactAction = playerInput.actions["Interact"];
        _weaponSlot = null;
        _itemSlot = null;
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
            if (col.gameObject == _weaponSlot || col.gameObject == _itemSlot)
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
            if (_interactAction.WasPressedThisDynamicUpdate())
            {
                InteractWith(closestInteractable);
            }
        }
    }

    void InteractWith(Interactable interactableObject)
    {
        string tag = interactableObject.tag;

        // Act according to the item's tag
        if (tag == "Weapon")
        {
            // Drop weapon if there is one
            if (_weaponSlot != null)
            {
                _weaponSlot.GetComponent<Interactable>().Drop();
            }

            // Pick up weapon
            _weaponSlot = interactableObject.gameObject;
            interactableObject.PickUp(_handSlot);
        }
        else if (tag == "Item")
        {
            // Drop item is there is one
            if (_itemSlot != null)
            {
                _itemSlot.GetComponent<Interactable>().Drop();
            }

            // Pick up item
            _itemSlot = interactableObject.gameObject;
            interactableObject.PickUp(_handSlot);
        }
        else if (tag == "Interactable")
        {
            interactableObject.InteractWith(gameObject);
            return;
        }
    }

    void HighlightObject(GameObject go)
    {

    }

    public WeaponData ReturnWeaponData()
    {
        if (_weaponSlot != null)
        {
            return _weaponSlot.GetComponent<Weapon>().ReturnWeaponData();
        }
        return null;
    }
}
