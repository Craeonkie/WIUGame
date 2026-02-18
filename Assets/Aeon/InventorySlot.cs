//// InventorySlot.cs — attach to EVERY slot UI element
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//public class InventorySlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
//{
//    public enum SlotType { Primary, Secondary, Inventory }
//    public SlotType slotType;

//    [Header("Visuals")]
//    public Image iconImage; // Assign child "Icon" Image component

//    [SerializeField] private InventoryUIManager inventoryUI; // Optional: assign in Inspector for speed

//    private void Start()
//    {
//        ClearSlot();
//    }

//    public void SetItem(Item item)
//    {
//        iconImage.enabled = (item != null);
//        if (item != null)
//        {
//            iconImage.sprite = item.image;
//        }
//    }

//    public void ClearSlot() => SetItem(null);

//    // ===== DRAG EVENTS =====
//    public void OnPointerDown(PointerEventData eventData)
//    {
//        // Only start drag if this slot has an item
//        if (inventoryUI != null && iconImage.enabled)
//        {
//            inventoryUI.BeginDrag(this);
//        }
//    }

//    public void OnPointerUp(PointerEventData eventData)
//    {
//        if (inventoryUI == null) return;

//        // Raycast to find drop target under cursor
//        PointerEventData pointerData = new PointerEventData(EventSystem.current)
//        {
//            position = eventData.position
//        };

//        var results = new List<RaycastResult>();
//        EventSystem.current.RaycastAll(pointerData, results);

//        InventorySlot dropTarget = this; // Default: drop on self (no-op)
//        foreach (var result in results)
//        {
//            if (result.gameObject.TryGetComponent<InventorySlot>(out var slot))
//            {
//                dropTarget = slot;
//                break;
//            }
//        }

//        inventoryUI.EndDrag(dropTarget);
//    }
//}