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
    public ItemSO bagitem;
    public ItemSO hatchetitem;

    [Header("Save Data")]
    public List<ItemSO> allItemTypes = new List<ItemSO>();

    [Header("Inventory Audio")]
    public AudioSource inventoryAudioSource;
    public AudioClip inventoryOpenSound;
    public AudioClip inventoryCloseSound;

    [Range(0f, 1f)]
    public float inventorySoundVolume = 1f;


    [Header("UI References")]
    public GameObject hotbarObj;
    public GameObject inventorySlotParent;
    public Image dragIcon;
    public GameObject container;
    public GameObject hud1;
    public GameObject hud2;
    public bool IsOpen => container.activeSelf;

    [Header("Control Prompts")]
    public GameObject hatchetAttackPrompt;
    public GameObject flashlightRmbPrompt;
    public GameObject flashlightOnPrompt;
    public GameObject flashlightOffPrompt;
    public GameObject qPrompt;
    public GameObject ePrompt;
    public GameObject tabPrompt;

    [Header("Pickup Settings")]
    public float pickupRange = 3f;
    private Item lookedAtItem = null;
    private Outline currentOutline;

    [Header("Hotbar Settings")]
    private int equippedHotbarIndex = 0;
    public float equipOpacity = 0.9f;
    public float normalOpacity = 0.58f;

    [Header("Equipment")]
    public Transform flashlightHand;
    public Transform bagHand;
    public Transform bagHolder;
    public Transform hatchetHand;

    public GameObject currentHandItem;
    private GameObject bagObject;

    [Header("Inventory Data")]
    private List<Slot> inventorySlots = new List<Slot>();
    private List<Slot> hotbarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();

    [Header("Drag & Drop")]
    private Slot draggedSlot = null;
    private bool isDragging = false;

    public void Awake()
    {
        inventorySlots.AddRange(
            inventorySlotParent.GetComponentsInChildren<Slot>()
        );

        hotbarSlots.AddRange(
            hotbarObj.GetComponentsInChildren<Slot>()
        );

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

            if (!IsHoldingItem(bagitem))
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

        //HandleItemUse();
        UpdatePickupDropPrompts();
    }

    private void UpdateHatchetPrompt()
    {
        if (hatchetAttackPrompt == null)
            return;

        bool holdingHatchet = IsHoldingItem(hatchetitem);

        hatchetAttackPrompt.SetActive(holdingHatchet);
    }

    private void UpdatePickupDropPrompts()
    {
        if (PauseManager.Instance != null &&
            PauseManager.Instance.IsPaused)
        {
            if (ePrompt != null)
                ePrompt.SetActive(false);

            if (qPrompt != null)
                qPrompt.SetActive(false);

            if (hatchetAttackPrompt != null)
                hatchetAttackPrompt.SetActive(false);

            if (flashlightRmbPrompt != null)
                flashlightRmbPrompt.SetActive(false);

            if (flashlightOnPrompt != null)
                flashlightOnPrompt.SetActive(false);

            if (flashlightOffPrompt != null)
                flashlightOffPrompt.SetActive(false);

            if (tabPrompt != null)
                tabPrompt.SetActive(false);

            return;
        }

        if (ePrompt != null)
        {
            ePrompt.SetActive(
                lookedAtItem != null &&
                !IsOpen
            );
        }

        if (qPrompt != null)
        {
            qPrompt.SetActive(
                currentHandItem != null &&
                !IsOpen
            );
        }

        if (tabPrompt != null)
        {
            tabPrompt.SetActive(
                IsHoldingItem(bagitem) &&
                !IsOpen
            );
        }

        UpdateHatchetPrompt();
    }

    private Transform GetHandForItem(ItemSO item)
    {
        if (item == flashlightitem)
            return flashlightHand;

        if (item == bagitem)
            return bagHand;

        if (item == hatchetitem)
            return hatchetHand;

        return null;
    }

    public bool IsHoldingTwoHandedItem()
    {
        if (equippedHotbarIndex < 0 ||
            equippedHotbarIndex >= hotbarSlots.Count)
            return false;

        Slot equippedSlot = hotbarSlots[equippedHotbarIndex];

        if (!equippedSlot.HasItem())
            return false;

        ItemSO equippedItem = equippedSlot.GetItem();

        if (equippedItem == null)
            return false;

        return equippedItem.isTwoHanded;
    }

    public bool IsHoldingItem()
    {
        return currentHandItem != null;
    }

    public bool IsHoldingItem(ItemSO item)
    {
        if (equippedHotbarIndex < 0 ||
            equippedHotbarIndex >= hotbarSlots.Count)
            return false;

        Slot equippedSlot = hotbarSlots[equippedHotbarIndex];

        if (!equippedSlot.HasItem())
            return false;

        return equippedSlot.GetItem() == item;
    }

    public bool HasItem(ItemSO item)
    {
        foreach (Slot slot in allSlots)
        {
            if (slot.HasItem() &&
                slot.GetItem() == item)
            {
                return true;
            }
        }

        return false;
    }

    public void AddItem(ItemSO ItemToAdd, int amount)
    {
        // Bag always goes to hotbar slot 5
        if (ItemToAdd == bagitem)
        {
            int bagSlotIndex = 4; // Slot 5

            if (bagSlotIndex < hotbarSlots.Count)
            {
                Slot bagSlot = hotbarSlots[bagSlotIndex];

                if (!bagSlot.HasItem())
                {
                    bagSlot.SetItem(ItemToAdd, amount);
                    return;
                }

                if (bagSlot.GetItem() == ItemToAdd)
                {
                    bagSlot.SetItem(
                        ItemToAdd,
                        bagSlot.GetAmount() + amount
                    );

                    return;
                }
            }

            Debug.LogWarning(
                "Could not place bag into hotbar slot 5."
            );

            return;
        }

        int remaining = amount;

        // Try stacking in the hotbar first
        foreach (Slot slot in hotbarSlots)
        {
            if (slot.HasItem() &&
                slot.GetItem() == ItemToAdd)
            {
                int currentAmount = slot.GetAmount();
                int maxStack = ItemToAdd.maxStackSize;

                if (currentAmount < maxStack)
                {
                    int spaceLeft = maxStack - currentAmount;
                    int amountToAdd =
                        Mathf.Min(spaceLeft, remaining);

                    slot.SetItem(
                        ItemToAdd,
                        currentAmount + amountToAdd
                    );

                    remaining -= amountToAdd;

                    if (remaining <= 0)
                        return;
                }
            }
        }

        // Try empty hotbar slots
        foreach (Slot slot in hotbarSlots)
        {
            if (!slot.HasItem())
            {
                int amountToPlace =
                    Mathf.Min(
                        ItemToAdd.maxStackSize,
                        remaining
                    );

                slot.SetItem(
                    ItemToAdd,
                    amountToPlace
                );

                remaining -= amountToPlace;

                if (remaining <= 0)
                    return;
            }
        }

        // If hotbar is full, put the item in the inventory
        foreach (Slot slot in inventorySlots)
        {
            if (slot.HasItem() &&
                slot.GetItem() == ItemToAdd)
            {
                int currentAmount = slot.GetAmount();
                int maxStack = ItemToAdd.maxStackSize;

                if (currentAmount < maxStack)
                {
                    int spaceLeft = maxStack - currentAmount;

                    int amountToAdd =
                        Mathf.Min(
                            spaceLeft,
                            remaining
                        );

                    slot.SetItem(
                        ItemToAdd,
                        currentAmount + amountToAdd
                    );

                    remaining -= amountToAdd;

                    if (remaining <= 0)
                        return;
                }
            }
        }

        // Try empty inventory slots
        foreach (Slot slot in inventorySlots)
        {
            if (!slot.HasItem())
            {
                int amountToPlace =
                    Mathf.Min(
                        ItemToAdd.maxStackSize,
                        remaining
                    );

                slot.SetItem(
                    ItemToAdd,
                    amountToPlace
                );

                remaining -= amountToPlace;

                if (remaining <= 0)
                    return;
            }
        }

        if (remaining > 0)
        {
            Debug.Log(
                "Inventory Is Full, could not add " +
                remaining +
                " of " +
                ItemToAdd.itemName
            );
        }
    }

    private void StartDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Slot hovered = GetHoveredSlot();

            if (hovered != null &&
                hovered.HasItem())
            {
                // Don't allow the bag to be moved
                if (hovered.GetItem() == bagitem)
                    return;

                draggedSlot = hovered;
                isDragging = true;

                dragIcon.sprite =
                    hovered.GetItem().icon;

                dragIcon.color =
                    new Color(1, 1, 1, 0.5f);

                dragIcon.enabled = true;
            }
        }
    }

    private void EndDrag()
    {
        if (Input.GetMouseButtonUp(0) &&
            isDragging)
        {
            Slot hovered = GetHoveredSlot();

            if (hovered != null)
            {
                HandleDrop(
                    draggedSlot,
                    hovered
                );

                dragIcon.enabled = false;
                draggedSlot = null;
                isDragging = false;
            }
        }
    }

    private Slot GetHoveredSlot()
    {
        foreach (Slot s in allSlots)
        {
            if (s.hovering)
                return s;
        }

        return null;
    }

    private void HandleDrop(Slot from, Slot to)
    {
        if (from == null ||
            to == null)
            return;

        // Don't allow the bag to be moved
        if (from.HasItem() &&
            from.GetItem() == bagitem)
            return;

        if (to.HasItem() &&
            to.GetItem() == bagitem)
            return;

        if (from == to)
            return;

        int fromHotbarIndex =
            hotbarSlots.IndexOf(from);

        // Stacking
        if (to.HasItem() &&
            to.GetItem() == from.GetItem())
        {
            int max =
                to.GetItem().maxStackSize;

            int space =
                max - to.GetAmount();

            if (space > 0)
            {
                int move =
                    Mathf.Min(
                        space,
                        from.GetAmount()
                    );

                to.SetItem(
                    to.GetItem(),
                    to.GetAmount() + move
                );

                from.SetItem(
                    from.GetItem(),
                    from.GetAmount() - move
                );

                if (from.GetAmount() <= 0)
                    from.ClearSlot();

                // If the destination is a hotbar slot, equip it
                int hotbarIndex =
                    hotbarSlots.IndexOf(to);

                if (hotbarIndex >= 0)
                {
                    equippedHotbarIndex =
                        hotbarIndex;

                    EquipHandItem();
                }
            }

            return;
        }

        // Different Item
        if (to.HasItem())
        {
            ItemSO tempItem =
                to.GetItem();

            int tempAmount =
                to.GetAmount();

            to.SetItem(
                from.GetItem(),
                from.GetAmount()
            );

            from.SetItem(
                tempItem,
                tempAmount
            );
        }
        else
        {
            // Empty Slot
            to.SetItem(
                from.GetItem(),
                from.GetAmount()
            );

            from.ClearSlot();
        }

        // Check if the destination is a hotbar slot
        int hotbarIndexAfterDrop =
            hotbarSlots.IndexOf(to);

        if (hotbarIndexAfterDrop >= 0)
        {
            // Make this hotbar slot the selected/equipped slot
            equippedHotbarIndex =
                hotbarIndexAfterDrop;

            // Immediately equip the item
            EquipHandItem();

            // Update the hotbar visual
            UpdateHotBarOpacity();
        }
        else if (fromHotbarIndex ==
                 equippedHotbarIndex)
        {
            EquipHandItem();
            UpdateHotBarOpacity();
        }
    }

    private void UpdateDragItemPosition()
    {
        if (isDragging)
        {
            dragIcon.transform.position =
                Input.mousePosition;
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

            ItemSO pickedUpItem = lookedAtItem.item;

            AddItem(
                pickedUpItem,
                lookedAtItem.amount
            );

            ObjectPrerequisite[] prerequisites = FindObjectsByType<ObjectPrerequisite>(FindObjectsSortMode.None);

            foreach (ObjectPrerequisite prerequisite in prerequisites)
            {
                if (pickedUpItem == hatchetitem)
                    prerequisite.ObtainRequirement("Hatchet");

                if (pickedUpItem == flashlightitem)
                    prerequisite.ObtainRequirement("Flashlight");

                if (pickedUpItem == bagitem)
                    prerequisite.ObtainRequirement("Bag");
            }

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.CompletePickup(
                    pickedUpItem,
                    hatchetitem,
                    flashlightitem
                );
            }

            PersistentWorldItem worldItem = lookedAtItem.GetComponentInParent<PersistentWorldItem>();

            if (worldItem == null)
            {
                Debug.LogWarning(
                    "Pickup failed to find PersistentWorldItem on: " +
                    lookedAtItem.name
                );
            }
            else if (PlayFabSaveManager.Instance == null)
            {
                Debug.LogWarning(
                    "PlayFabSaveManager.Instance is NULL when picking up: " +
                    worldItem.worldID
                );
            }
            else
            {
                Debug.Log("Registering collected world object: " + worldItem.worldID);

                PlayFabSaveManager.Instance.RegisterCollectedWorldObject(
                    worldItem.worldID
                );
            }

            Destroy(lookedAtItem.gameObject);
            lookedAtItem = null;

            int foundSlot = -1;

            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                if (hotbarSlots[i].HasItem() &&
                    hotbarSlots[i].GetItem() == pickedUpItem)
                {
                    foundSlot = i;
                    break;
                }
            }

            if (foundSlot >= 0)
            {
                equippedHotbarIndex = foundSlot;
                UpdateHotBarOpacity();
                EquipHandItem();
            }
        }
    }
    private void DetectLookedAtItem()
    {
        if (Camera.main == null)
            return;

        Ray ray = new Ray(
            Camera.main.transform.position,
            Camera.main.transform.forward
        );

        Item newItem = null;

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            pickupRange
        ))
        {
            newItem =
                hit.collider.GetComponentInParent<Item>();
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
            currentOutline =
                lookedAtItem.GetComponent<Outline>();

            if (currentOutline != null)
                currentOutline.enabled = true;
        }
    }

    private void UpdateHotBarOpacity()
    {
        for (int i = 0;
             i < hotbarSlots.Count;
             i++)
        {
            Image icon =
                hotbarSlots[i].GetComponent<Image>();

            if (icon != null)
            {
                if (i == equippedHotbarIndex)
                {
                    icon.color =
                        new Color(
                            0,
                            0,
                            0,
                            equipOpacity
                        );
                }
                else
                {
                    icon.color =
                        new Color(
                            0,
                            0,
                            0,
                            normalOpacity
                        );
                }
            }
        }
    }

    private void HandleHotBarSelection()
    {
        // Don't switch hotbar while logbook is open
        if (PauseManager.Instance != null &&
            PauseManager.Instance.IsLogbookOpen)
            return;

        // Don't switch hotbar while inventory is open
        if (IsOpen)
            return;

        // Don't switch hotbar while paused
        if (PauseManager.Instance != null &&
            PauseManager.Instance.IsPaused)
            return;
            
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            KeyCode key = KeyCode.Alpha1 + i;

            if (Input.GetKeyDown(key))
            {
                equippedHotbarIndex = i;

                UpdateHotBarOpacity();
                EquipHandItem();

                return;
            }
        }
        float scroll = Input.mouseScrollDelta.y;

        if (scroll == 0f)
            return;

        int direction = scroll > 0f ? 1 : -1;

        equippedHotbarIndex += direction;

        // Wrap around
        if (equippedHotbarIndex >= hotbarSlots.Count)
            equippedHotbarIndex = 0;

        if (equippedHotbarIndex < 0)
            equippedHotbarIndex = hotbarSlots.Count - 1;

        UpdateHotBarOpacity();
        EquipHandItem();
    }

    public void HandleDropEquippedItem()
    {
        if (!Input.GetKeyDown(KeyCode.Q))
            return;

        if (equippedHotbarIndex < 0 ||
            equippedHotbarIndex >= hotbarSlots.Count)
            return;

        Slot equippedSlot =
            hotbarSlots[equippedHotbarIndex];

        if (!equippedSlot.HasItem())
            return;

        ItemSO itemSO =
            equippedSlot.GetItem();

        GameObject prefab =
            itemSO.itemPrefab;

        if (prefab == null)
            return;

        bool flashlightState = false;

        if (currentHandItem != null)
        {
            FlashlightScript heldFlashlight =
                currentHandItem.GetComponentInChildren<FlashlightScript>();

            if (heldFlashlight != null)
            {
                flashlightState =
                    heldFlashlight.IsOn;
            }
        }

        GameObject dropped =
            Instantiate(
                prefab,
                Camera.main.transform.position +
                Camera.main.transform.forward,
                Quaternion.identity
            );

        Item item =
            dropped.GetComponent<Item>();

        if (item != null)
        {
            item.item = itemSO;
            item.amount =
                equippedSlot.GetAmount();

            item.flashlightOn =
                flashlightState;
        }

        FlashlightScript droppedFlashlight =
            dropped.GetComponentInChildren<FlashlightScript>();

        if (droppedFlashlight != null)
        {
            droppedFlashlight.SetState(
                flashlightState
            );
        }

        equippedSlot.ClearSlot();

        if (itemSO == bagitem)
        {
            CloseInventory();

            if (bagObject != null)
            {
                Destroy(bagObject);
                bagObject = null;
            }
        }

        EquipHandItem();

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.CompleteDrop();
        }
    }

    private void HideFlashlightPrompt()
    {
        if (flashlightRmbPrompt != null)
            flashlightRmbPrompt.SetActive(false);
    }

    private void EquipHandItem()
    {
        HideFlashlightPrompt();

        if (equippedHotbarIndex < 0 ||
            equippedHotbarIndex >= hotbarSlots.Count)
            return;

        // Destroy currently held item
        if (currentHandItem != null)
        {
            Destroy(currentHandItem);
            currentHandItem = null;
        }

        // Make sure the normal bag is visible on the BagHolder
        if (bagObject != null)
        {
            AttachBagToHolder();
            bagObject.SetActive(true);
        }

        Slot equippedSlot =
            hotbarSlots[equippedHotbarIndex];

        if (!equippedSlot.HasItem())
        {
            CloseInventory();
            UpdateHatchetPrompt();
            return;
        }

        ItemSO item =
            equippedSlot.GetItem();

        if (item.handItemPrefab == null)
        {
            CloseInventory();
            return;
        }

        // BAG
        if (item == bagitem)
        {
            if (bagHand == null)
            {
                Debug.LogWarning("Bag Hand transform is not assigned.");
                return;
            }

            if (bagObject == null)
            {
                AttachBagToHolder();
            }

            if (bagObject != null)
            {
                bagObject.SetActive(false);
            }

            currentHandItem = Instantiate(
                item.handItemPrefab,
                bagHand
            );

            currentHandItem.transform.localPosition = Vector3.zero;
            currentHandItem.transform.localRotation = Quaternion.identity;
            currentHandItem.transform.localScale = Vector3.one;

            foreach (Collider col in
                currentHandItem.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            foreach (Rigidbody rb in
                currentHandItem.GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.detectCollisions = false;
            }

            Item bagHandItem =
                currentHandItem.GetComponentInChildren<Item>();

            if (bagHandItem != null)
            {
                bagHandItem.enabled = false;
            }

            UpdateHatchetPrompt();

            return;
        }

        if (item != bagitem && IsOpen)
        {
            CloseInventory();
        }

        // Get the correct hand transform
        Transform selectedHand =
            GetHandForItem(item);

        if (selectedHand == null)
        {
            Debug.LogWarning(
                "No hand transform assigned for: " +
                item.itemName
            );

            return;
        }

        // Spawn the hand item on the correct hand
        currentHandItem =
            Instantiate(
                item.handItemPrefab,
                selectedHand
            );

        currentHandItem.transform.localPosition =
            Vector3.zero;

        currentHandItem.transform.localRotation =
            Quaternion.identity;

        currentHandItem.transform.localScale =
            Vector3.one;

        // Setup flashlight control UI
        FlashlightScript flashlight =
            currentHandItem.GetComponentInChildren<FlashlightScript>();

        if (flashlight != null)
        {
            flashlight.SetControlUI(
                flashlightRmbPrompt,
                flashlightOnPrompt,
                flashlightOffPrompt
            );
        }

        // Disable all colliders
        foreach (
            Collider col in
            currentHandItem.GetComponentsInChildren<Collider>()
        )
        {
            col.enabled = false;
        }

        // Enable melee hitbox if this item has one
        MeleeDamage meleeDamage =
            currentHandItem.GetComponentInChildren<MeleeDamage>();

        if (meleeDamage != null)
        {
            BoxCollider meleeCollider =
                meleeDamage.GetComponent<BoxCollider>();

            if (meleeCollider != null)
            {
                meleeCollider.enabled = true;
                meleeCollider.isTrigger = true;
            }
        }

        // Disable physics
        foreach (
            Rigidbody rb in
            currentHandItem.GetComponentsInChildren<Rigidbody>()
        )
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable Item.cs
        Item itemComponent =
            currentHandItem.GetComponentInChildren<Item>();

        if (itemComponent != null)
        {
            itemComponent.enabled = false;
        }

        UpdateHatchetPrompt();
    }

    /*private void HandleItemUse()
    {
        if (container.activeInHierarchy)
            return;

        if (currentHandItem == null)
            return;

        IUsableItem usable =
            currentHandItem.GetComponent<IUsableItem>();

        if (usable == null)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            usable.OnUsePrimary();

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.CompleteFlashlightUse();
            }
        }
    }*/

    public void OpenInventory()
    {
        container.SetActive(true);

        if (hud1 != null)
            hud1.SetActive(false);

        if (hud2 != null)
            hud2.SetActive(false);

        Cursor.lockState =
            CursorLockMode.None;

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

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;

        Movement.Instance.SetLookEnabled(true);
    }

    
    public void ToggleInventory()
    {
        if (IsOpen)
        {
            if (inventoryAudioSource != null &&
                inventoryCloseSound != null)
            {
                inventoryAudioSource.PlayOneShot(
                    inventoryCloseSound,
                    inventorySoundVolume
                );
            }

            CloseInventory();
        }
        else
        {
            if (inventoryAudioSource != null &&
                inventoryOpenSound != null)
            {
                inventoryAudioSource.PlayOneShot(
                    inventoryOpenSound,
                    inventorySoundVolume
                );
            }

            OpenInventory();

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.CompleteInventoryBG();
            }
        }
    }

    public void RagdollDropHeldItem()
    {
        if (currentHandItem == null)
            return;

        GameObject heldItem =
            currentHandItem;

        currentHandItem = null;

        // Get the currently equipped item
        if (equippedHotbarIndex < 0 ||
            equippedHotbarIndex >= hotbarSlots.Count)
        {
            Destroy(heldItem);
            return;
        }

        Slot equippedSlot =
            hotbarSlots[equippedHotbarIndex];

        if (!equippedSlot.HasItem())
        {
            Destroy(heldItem);
            return;
        }

        ItemSO itemSO =
            equippedSlot.GetItem();

        if (itemSO == null ||
            itemSO.itemPrefab == null)
        {
            Destroy(heldItem);
            return;
        }

        // Don't spawn another normal bag here
        // The RagdollController handles the bag separately
        if (itemSO == bagitem)
        {
            Destroy(heldItem);
            return;
        }

        // Save flashlight state
        bool flashlightState = false;

        FlashlightScript heldFlashlight =
            heldItem.GetComponentInChildren<FlashlightScript>();

        if (heldFlashlight != null)
        {
            flashlightState =
                heldFlashlight.IsOn;
        }

        // Remember where the item was being held
        Vector3 dropPosition =
            heldItem.transform.position;

        Quaternion dropRotation =
            heldItem.transform.rotation;

        // Destroy the hand version
        Destroy(heldItem);

        // Spawn the normal world prefab
        GameObject droppedItem =
            Instantiate(
                itemSO.itemPrefab,
                dropPosition,
                dropRotation
            );

        // Setup Item component
        Item itemComponent =
            droppedItem.GetComponentInChildren<Item>();

        if (itemComponent != null)
        {
            itemComponent.item =
                itemSO;

            itemComponent.amount =
                equippedSlot.GetAmount();
        }

        // Restore flashlight state
        FlashlightScript droppedFlashlight =
            droppedItem.GetComponentInChildren<FlashlightScript>();

        if (droppedFlashlight != null)
        {
            droppedFlashlight.SetState(
                flashlightState
            );
        }

        // Enable all colliders
        foreach (
            Collider col in
            droppedItem.GetComponentsInChildren<Collider>()
        )
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        // Find Rigidbody
        Rigidbody rb =
            droppedItem.GetComponentInChildren<Rigidbody>();

        if (rb == null)
        {
            rb =
                droppedItem.GetComponent<Rigidbody>();
        }

        if (rb == null)
        {
            rb =
                droppedItem.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = true;

        // Better collision with the floor
        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        if (rb.mass <= 0)
        {
            rb.mass = 1f;
        }

        // Yank the item away from the player
        Vector3 yankDirection =
            transform.forward * 3f;

        yankDirection +=
            Vector3.up * 2f;

        rb.AddForce(
            yankDirection,
            ForceMode.Impulse
        );

        // Add some rotation
        rb.AddTorque(
            Random.insideUnitSphere * 3f,
            ForceMode.Impulse
        );
    }

    private void AttachBagToHolder()
    {
        if (bagHolder == null || bagitem == null)
            return;

        if (bagObject != null)
            Destroy(bagObject);

        bagObject = Instantiate(
            bagitem.handItemPrefab,
            bagHolder
        );

        bagObject.transform.localPosition = Vector3.zero;
        bagObject.transform.localRotation = Quaternion.identity;
        bagObject.transform.localScale = Vector3.one;

        foreach (Collider col in
            bagObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        foreach (Rigidbody rb in
            bagObject.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.detectCollisions = false;
        }

        Item bagItemComponent =
            bagObject.GetComponentInChildren<Item>();

        if (bagItemComponent != null)
        {
            bagItemComponent.enabled = false;
            bagItemComponent.item = bagitem;
        }

        bagObject.SetActive(true);
    }

    public void RagdollDropBag()
    {
        if (bagObject == null)
            return;

        GameObject bag =
            bagObject;

        // Remove the bag from the BagHolder
        bag.transform.SetParent(null);

        // Enable colliders
        Collider[] colliders =
            bag.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        // Find or create Rigidbody
        Rigidbody rb =
            bag.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb =
                bag.GetComponentInChildren<Rigidbody>();
        }

        if (rb == null)
        {
            rb =
                bag.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.detectCollisions = true;

        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        // Yank the bag away
        Vector3 yankDirection =
            transform.forward * 3f;

        yankDirection +=
            Vector3.up * 2f;

        rb.AddForce(
            yankDirection,
            ForceMode.Impulse
        );

        // Spin the bag
        rb.AddTorque(
            Random.insideUnitSphere * 3f,
            ForceMode.Impulse
        );

        // The attached bag no longer exists
        bagObject = null;
    }

    private void SaveInventoryChanges()
    {
        if (PlayFabSaveManager.Instance != null)
            PlayFabSaveManager.Instance.SaveGame();
    }
    public int GetEquippedHotbarIndex()
    {
        return equippedHotbarIndex;
    }

    public List<PlayFabSaveManager.InventorySlotSaveData> GetInventorySaveData()
    {
        List<PlayFabSaveManager.InventorySlotSaveData> savedSlots =
            new List<PlayFabSaveManager.InventorySlotSaveData>();

        SaveSlots(hotbarSlots, true, savedSlots);
        SaveSlots(inventorySlots, false, savedSlots);

        return savedSlots;
    }

    private void SaveSlots(
        List<Slot> slots,
        bool isHotbar,
        List<PlayFabSaveManager.InventorySlotSaveData> savedSlots)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            Slot slot = slots[i];

            if (slot == null || !slot.HasItem())
                continue;

            ItemSO item = slot.GetItem();

            if (item == null || string.IsNullOrEmpty(item.itemID))
            {
                Debug.LogWarning("Cannot save an item without an Item ID.");
                continue;
            }

            savedSlots.Add(new PlayFabSaveManager.InventorySlotSaveData
            {
                itemID = item.itemID,
                amount = slot.GetAmount(),
                isHotbar = isHotbar,
                slotIndex = i
            });
        }
    }

    public void RestoreInventory(
        List<PlayFabSaveManager.InventorySlotSaveData> savedSlots,
        int savedEquippedIndex)
    {
        foreach (Slot slot in allSlots)
        {
            if (slot != null)
                slot.ClearSlot();
        }

        if (bagObject != null)
        {
            Destroy(bagObject);
            bagObject = null;
        }

        if (currentHandItem != null)
        {
            Destroy(currentHandItem);
            currentHandItem = null;
        }

        if (savedSlots != null)
        {
            foreach (PlayFabSaveManager.InventorySlotSaveData saved in savedSlots)
            {
                if (saved == null || saved.amount <= 0)
                    continue;

                List<Slot> targetSlots =
                    saved.isHotbar ? hotbarSlots : inventorySlots;

                if (saved.slotIndex < 0 || saved.slotIndex >= targetSlots.Count)
                    continue;

                ItemSO item = allItemTypes.Find(
                    candidate => candidate != null &&
                                candidate.itemID == saved.itemID
                );

                if (item == null)
                {
                    Debug.LogWarning(
                        "Could not restore inventory item ID: " + saved.itemID
                    );
                    continue;
                }

                targetSlots[saved.slotIndex].SetItem(item, saved.amount);
            }
        }

        if (hotbarSlots.Count > 0)
        {
            equippedHotbarIndex = Mathf.Clamp(
                savedEquippedIndex,
                0,
                hotbarSlots.Count - 1
            );
        }
        else
        {
            equippedHotbarIndex = 0;
        }

        UpdateHotBarOpacity();
        EquipHandItem();
    }


      // TUTORIAL CHECKPOINT SNAPSHOT
  

    [System.Serializable]
    public class SlotSnapshot
    {
        public ItemSO item;
        public int amount;
    }

    [System.Serializable]
    public class TutorialInventorySnapshot
    {
        public List<SlotSnapshot> hotbar = new List<SlotSnapshot>();
        public List<SlotSnapshot> inventory = new List<SlotSnapshot>();
        public int equippedHotbarIndex;
        public bool flashlightOn;
    }

    public TutorialInventorySnapshot CaptureTutorialSnapshot()
    {
        TutorialInventorySnapshot snapshot = new TutorialInventorySnapshot();
        snapshot.equippedHotbarIndex = equippedHotbarIndex;

        foreach (Slot slot in hotbarSlots)
        {
            snapshot.hotbar.Add(new SlotSnapshot
            {
                item = slot != null ? slot.GetItem() : null,
                amount = slot != null ? slot.GetAmount() : 0
            });
        }

        foreach (Slot slot in inventorySlots)
        {
            snapshot.inventory.Add(new SlotSnapshot
            {
                item = slot != null ? slot.GetItem() : null,
                amount = slot != null ? slot.GetAmount() : 0
            });
        }

        // The flashlight only has a live on/off state while equipped.
        if (currentHandItem != null)
        {
            FlashlightScript flashlight =
                currentHandItem.GetComponentInChildren<FlashlightScript>(true);

            if (flashlight != null)
                snapshot.flashlightOn = flashlight.IsOn;
        }

        return snapshot;
    }

    public void RestoreTutorialSnapshot(TutorialInventorySnapshot snapshot)
    {
        if (snapshot == null)
        {
            Debug.LogWarning("Tutorial inventory snapshot is missing.");
            return;
        }

        // Close the inventory before rebuilding equipment/UI state.
        if (container != null && container.activeSelf)
            CloseInventory();

        // Destroy the current held model and normal bag visual.
        if (currentHandItem != null)
        {
            Destroy(currentHandItem);
            currentHandItem = null;
        }

        if (bagObject != null)
        {
            Destroy(bagObject);
            bagObject = null;
        }

        // RagdollDropBag detaches its visual and clears bagObject.
        // Remove those detached bag visuals too, but leave actual world
        // pickups alone (they have PersistentWorldItem components).
        Item[] sceneItems = FindObjectsByType<Item>(FindObjectsSortMode.None);
        foreach (Item sceneItem in sceneItems)
        {
            if (sceneItem == null || sceneItem.item != bagitem)
                continue;

            if (sceneItem.GetComponentInParent<PersistentWorldItem>() != null)
                continue;

            // Do not remove a bag item asset from another scene.
            if (sceneItem.gameObject.scene != gameObject.scene)
                continue;

            Destroy(sceneItem.gameObject);
        }

        // Clear all slots without invoking any PlayFab operations.
        foreach (Slot slot in allSlots)
        {
            if (slot != null)
                slot.ClearSlot();
        }

        // Restore hotbar slot contents and quantities.
        int hotbarCount = Mathf.Min(hotbarSlots.Count, snapshot.hotbar.Count);
        for (int i = 0; i < hotbarCount; i++)
        {
            SlotSnapshot saved = snapshot.hotbar[i];
            if (saved != null && saved.item != null && saved.amount > 0)
                hotbarSlots[i].SetItem(saved.item, saved.amount);
        }

        // Restore regular inventory slot contents and quantities.
        int inventoryCount = Mathf.Min(inventorySlots.Count, snapshot.inventory.Count);
        for (int i = 0; i < inventoryCount; i++)
        {
            SlotSnapshot saved = snapshot.inventory[i];
            if (saved != null && saved.item != null && saved.amount > 0)
                inventorySlots[i].SetItem(saved.item, saved.amount);
        }

        // Restore selected hotbar slot.
        equippedHotbarIndex = hotbarSlots.Count > 0
            ? Mathf.Clamp(snapshot.equippedHotbarIndex, 0, hotbarSlots.Count - 1)
            : 0;

        // Cancel any drag operation that may have been active at death.
        draggedSlot = null;
        isDragging = false;
        if (dragIcon != null)
            dragIcon.enabled = false;

        // Rebuild the bag-on-back visual if the restored inventory contains it.
        if (HasItem(bagitem) && bagHolder != null)
            AttachBagToHolder();

        // Recreate the equipped hand model from the restored hotbar.
        EquipHandItem();

        // Restore flashlight state after the held flashlight is recreated.
        if (currentHandItem != null)
        {
            FlashlightScript flashlight =
                currentHandItem.GetComponentInChildren<FlashlightScript>(true);

            if (flashlight != null)
                flashlight.SetState(snapshot.flashlightOn);
        }

        UpdateHotBarOpacity();
        UpdateHatchetPrompt();
        UpdatePickupDropPrompts();

        Debug.Log("Tutorial inventory snapshot restored.");
    }

}