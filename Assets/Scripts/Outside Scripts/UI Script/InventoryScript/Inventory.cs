using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;

public class Inventory : MonoBehaviour
{
    [Header("Test Items")]
    public ItemSO flashlightitem;
    public ItemSO sphereitem;


    [Header("UI References")]
    public GameObject hotbarObj;
    public GameObject inventorySlotParent;
    public Image dragIcon;
    public GameObject container;
    public GameObject hud1;
    public GameObject hud2;
    public bool IsOpen => container.activeSelf;


    [Header("Pickup Settings")]
    public float pickupRange = 3f;  
    private Item lookedAtItem = null;
    private Outline currentOutline;
    public GameObject pickupUI;


    [Header("Hotbar Settings")]
    private int equippedHotbarIndex = 0;
    public float equipOpacity = 0.9f;
    public float normalOpacity = 0.58f;


    [Header("Equipment")]
    public Transform hand;
    private GameObject currentHandItem;
    


    [Header("Inventory Data")]
    private List<Slot> inventorySlots = new List<Slot>();
    private List<Slot> hotbarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();


    [Header("Drag & Drop")]
    private Slot draggedSlot = null;
    private bool isDragging = false;

    public void Awake()
    {
        inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
        hotbarSlots.AddRange(hotbarObj.GetComponentsInChildren<Slot>());
        
        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotbarSlots);
    }

    void Update()
    {     
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (PauseManager.Instance != null &&
                PauseManager.Instance.IsPaused)
                return;

            ToggleInventory();
        }

            DetectLookedAtItem();
            Pickup();

            StartDrag();
            UpdateDragItemPosition();
            EndDrag();

            HandleHotBarSelection();
            HandleDropEquippedItem();
            UpdateHotBarOpacity();

            HandleItemUse();
    }

    public bool IsHoldingItem()
    {
        return currentHandItem != null;
    }

    public void AddItem(ItemSO ItemToAdd, int amount)
    {
        int remaining = amount;

        foreach(Slot slot in allSlots)
        {
            if(slot.HasItem() && slot.GetItem() == ItemToAdd)
            {
                int currentAmount = slot.GetAmount();
                int maxStack = ItemToAdd.maxStackSize;

                if(currentAmount < maxStack)
                {
                    int spaceLeft = maxStack - currentAmount;
                    int amountToAdd = Mathf.Min(spaceLeft, remaining);

                    slot.SetItem(ItemToAdd, currentAmount + amountToAdd);
                    remaining -= amountToAdd;

                    if(remaining <= 0)
                    return;
                }
            }
        }
        foreach(Slot slot in allSlots)
        {
            if (!slot.HasItem())
            {
                int amountToPlace = Mathf.Min(ItemToAdd.maxStackSize, remaining);
                slot.SetItem(ItemToAdd, amountToPlace);
                remaining -= amountToPlace;

                if(remaining <= 0)
                return;
            }
        }
        if(remaining > 0)
        {
            Debug.Log("Inventory Is Full, could not add " + remaining + " of "+ ItemToAdd.itemName);
        }
    }

    private void StartDrag()
    {
        if(Input.GetMouseButtonDown(0))
        {
          Slot   hovered = GetHoveredSlot();

          if(hovered != null && hovered.HasItem())
            {
              draggedSlot = hovered;
              isDragging = true;

                dragIcon.sprite = hovered.GetItem().icon;
                dragIcon.color = new Color(1,1,1,0.5f);
                dragIcon.enabled = true;
            }
        }
    }

    private void EndDrag()
    {
        if(Input.GetMouseButtonUp(0) && isDragging)
        {
            Slot hovered = GetHoveredSlot();

            if(hovered != null)
            {
                HandleDrop(draggedSlot, hovered);

                dragIcon.enabled = false;
                draggedSlot = null;
                isDragging = false;
            }
        }
    }
    private Slot GetHoveredSlot()
    {
        foreach(Slot s in allSlots)
        {
            if(s.hovering)
            return s;
        }
        return null;
    }

    private void HandleDrop(Slot from, Slot to)
    {
        if(from == to) return;

        // Stacking
        if(to.HasItem() && to.GetItem() == from.GetItem())
        {
            int max = to.GetItem().maxStackSize;
            int space = max - to.GetAmount();

            if(space > 0)
            {
                int move = Mathf.Min(space, from.GetAmount());
                to.SetItem(to.GetItem(), to.GetAmount() + move);
                from.SetItem(from.GetItem(), from.GetAmount() - move);

                if(from.GetAmount() <= 0)
                   from.ClearSlot();
            }
            return;

        }
        // Different Item
        if (to.HasItem())
        {
            ItemSO tempItem = to.GetItem();
            int tempAmount = to.GetAmount();

            to.SetItem(from.GetItem(), from.GetAmount());
            from.SetItem(tempItem, tempAmount);
            return;
        }
        // Empty Slot
        to.SetItem(from.GetItem(), from.GetAmount());
        from.ClearSlot();
    }
    private void UpdateDragItemPosition()
    {
        if (isDragging)
        {
            dragIcon.transform.position = Input.mousePosition;
        }   
    }

    private void Pickup()
    {
        if (lookedAtItem != null && Input.GetKeyDown(KeyCode.E))
        {
            if (currentOutline != null)
            {
                currentOutline.enabled = false;
                currentOutline = null;
            }

            AddItem(lookedAtItem.item, lookedAtItem.amount);
            Destroy(lookedAtItem.gameObject);

            lookedAtItem = null;

            if (pickupUI != null)
                pickupUI.SetActive(false);

            EquipHandItem();
        }
    }

    private void DetectLookedAtItem()
    {
        if (Camera.main == null)
            return;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        Item newItem = null;

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            newItem = hit.collider.GetComponentInParent<Item>();
        }

        if (newItem == lookedAtItem)
            return;

        if (currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
        }

        lookedAtItem = newItem;

        if (lookedAtItem != null)
        {
            currentOutline = lookedAtItem.GetComponent<Outline>();

            /*Debug.Log("Found item: " + lookedAtItem.name);
            Debug.Log("pickupUI assigned? " + (pickupUI != null));*/

            if (currentOutline != null)
                currentOutline.enabled = true;

            if (pickupUI != null)
                pickupUI.SetActive(true);
        }
        else
        {
            if (pickupUI != null)
                pickupUI.SetActive(false);
        }
    }

    private void UpdateHotBarOpacity()
    {
        for(int i = 0; i < hotbarSlots.Count; i++)
        {
            Image icon = hotbarSlots[i].GetComponent<Image>();
            if(icon != null)
            {
                if (i == equippedHotbarIndex)
                {
                    icon.color = new Color(0, 0, 0, equipOpacity);
                }
                else
                {
                    icon.color = new Color(0, 0, 0, normalOpacity);
                }
            }
        }
    }

    private void HandleHotBarSelection()
{
    for (int i = 0; i < hotbarSlots.Count; i++)
    {
        if (Input.GetKeyDown(KeyCode.Alpha1 + i))
        {
            equippedHotbarIndex = i;
            UpdateHotBarOpacity();
            EquipHandItem();
            return;
        }
    }
}

    public void HandleDropEquippedItem()
    {
        if (!Input.GetKeyDown(KeyCode.G)) return;

        Slot equippedSlot = hotbarSlots[equippedHotbarIndex];
        if (!equippedSlot.HasItem()) return;

        ItemSO itemSO = equippedSlot.GetItem();
        GameObject prefab = itemSO.itemPrefab;

        if (prefab == null) return;

        // Save the flashlight state before destroying the held item
        bool flashlightState = false;

        if (currentHandItem != null)
        {
            FlashlightScript heldFlashlight = currentHandItem.GetComponentInChildren<FlashlightScript>();

            if (heldFlashlight != null)
            {
                flashlightState = heldFlashlight.IsOn;
            }
        }

        // Spawn the dropped item
        GameObject dropped = Instantiate(
            prefab,
            Camera.main.transform.position + Camera.main.transform.forward,
            Quaternion.identity
        );

        Item item = dropped.GetComponent<Item>();

        if (item != null)
        {
            item.item = itemSO;
            item.amount = equippedSlot.GetAmount();
            item.flashlightOn = flashlightState;
        }

        // Restore the flashlight state
        FlashlightScript droppedFlashlight = dropped.GetComponentInChildren<FlashlightScript>();

        if (droppedFlashlight != null)
        {
            droppedFlashlight.SetState(flashlightState);
        }

        equippedSlot.ClearSlot();
        EquipHandItem();
    }

    private void EquipHandItem()
    {
        if (equippedHotbarIndex < 0 || equippedHotbarIndex >= hotbarSlots.Count)
        return;

        if (currentHandItem != null)
        {
            Destroy(currentHandItem);
            currentHandItem = null;
        }

        Slot equippedSlot = hotbarSlots[equippedHotbarIndex];
        if(!equippedSlot.HasItem()) return;

        ItemSO item = equippedSlot.GetItem();
        if(item.handItemPrefab == null) return;

        currentHandItem = Instantiate(item.handItemPrefab, hand);
        currentHandItem.transform.localPosition = Vector3.zero;
        currentHandItem.transform.localRotation = Quaternion.identity;

        // Disable all colliders
        foreach (Collider col in currentHandItem.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // Disable physics
        foreach (Rigidbody rb in currentHandItem.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable Item.cs  
        Item itemComponent = currentHandItem.GetComponentInChildren<Item>();

        if (itemComponent != null)
        {
            itemComponent.enabled = false;
        }
    }

    private void HandleItemUse()
    {
        if (container.activeInHierarchy) return; 

        if (currentHandItem == null) return;

        IUsableItem usable = currentHandItem.GetComponent<IUsableItem>();

        if (usable == null) return;

        if (Input.GetMouseButtonDown(1))
        {
            usable.OnUsePrimary();
        }
    }
    
    public void OpenInventory()
{
    container.SetActive(true);

    if (hud1 != null)
        hud1.SetActive(false);

    if (hud2 != null)
        hud2.SetActive(false);

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    Movement.Instance.SetLookEnabled(false);
}

    public void CloseInventory()
{
    container.SetActive(false);

    if (hud1 != null)
        hud1.SetActive(true);

    if (hud2 != null)
        hud2.SetActive(true);

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    Movement.Instance.SetLookEnabled(true);
}

    public void ToggleInventory()
    {
        if (IsOpen)
            CloseInventory();
        else
            OpenInventory();
    }
    
}
