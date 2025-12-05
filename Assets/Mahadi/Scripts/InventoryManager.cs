using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int maxInventorySlots = 10;

    [Header("Current Inventory")]
    public List<InventoryItemInfo> inventoryItems = new List<InventoryItemInfo>();
    public InventoryItem itemPrefab;

    [Header("Item Holder")]
    public Transform[] itemHolders; // UI slot objects (containers)

    // Internal slot tracking
    private InventoryItemInfo[] slotAssignments; // Which item is in which slot
    private InventoryItem[] slotUI;              // The UI instance created in that slot

    private void Awake()
    {
        slotAssignments = new InventoryItemInfo[itemHolders.Length];
        slotUI = new InventoryItem[itemHolders.Length];
    }

    private void Start()
    {
        // Sample test items
        AddItem(1, "Potion", null, 10, 1, 10f);
        AddItem(1, "Potion", null, 10, 1, 10f);
        AddItem(2, "Potion", null, 10, 1, 10f);
        AddItem(3, "Potion", null, 10, 1, 10f);
        AddItem(4, "Potion", null, 10, 1, 10f);
        AddItem(5, "Potion", null, 10, 1, 10f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            AddItem(1, "Potion", null, 1, 1, 10f);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveItem(1, 1);
        }
    }

    // ----------------------------------------------------------------------
    // ADD ITEM
    // ----------------------------------------------------------------------
    public bool AddItem(int itemID, string itemName, Sprite icon, int quantity, int space, float cost)
    {
        // Check if already exists (stack)
        int existingIndex = FindItemIndex(itemID);
        if (existingIndex != -1)
        {
            inventoryItems[existingIndex].quantity += quantity;
            UpdateItemDisplay(existingIndex);
            return true;
        }

        // Check capacity
        if (inventoryItems.Count >= maxInventorySlots)
        {
            Debug.LogWarning("Inventory is full.");
            return false;
        }

        // Find free UI slot
        int freeSlot = FindFreeSlot();
        if (freeSlot == -1)
        {
            Debug.LogWarning("No free UI slot.");
            return false;
        }

        // Create item data
        InventoryItemInfo newItem = new InventoryItemInfo
        {
            itemID = itemID,
            itemName = itemName,
            itemIcon = icon,
            quantity = quantity,
            inventorySpace = space,
            itemCost = cost
        };

        inventoryItems.Add(newItem);

        // Create UI element in the slot
        InventoryItem ui = Instantiate(itemPrefab, itemHolders[freeSlot]);
        ui.inventoryManager = this;
        ui.SetInfo(newItem);

        // Track slot usage
        slotAssignments[freeSlot] = newItem;
        slotUI[freeSlot] = ui;

        return true;
    }

    // ----------------------------------------------------------------------
    // REMOVE ITEM
    // ----------------------------------------------------------------------
    public bool RemoveItem(int itemID, int quantityToRemove = 1)
    {
        int index = FindItemIndex(itemID);
        if (index == -1)
        {
            Debug.LogWarning("Item not found.");
            return false;
        }

        InventoryItemInfo item = inventoryItems[index];
        item.quantity -= quantityToRemove;

        if (item.quantity <= 0)
        {
            int slot = FindSlotOfItem(item);
            if (slot != -1)
                ClearSlot(slot);

            inventoryItems.RemoveAt(index);
        }
        else
        {
            UpdateItemDisplay(index);
        }

        return true;
    }

    // ----------------------------------------------------------------------
    // CLEAR SLOT — ONLY DESTROY THE UI WE CREATED, NOT OTHER CHILDREN
    // ----------------------------------------------------------------------
    private void ClearSlot(int slot)
    {
        slotAssignments[slot] = null;

        if (slotUI[slot] != null)
        {
            Destroy(slotUI[slot].gameObject); // delete only the UI prefab
            slotUI[slot] = null;
        }
    }

    // ----------------------------------------------------------------------
    // UTILITY
    // ----------------------------------------------------------------------
    int FindItemIndex(int itemID)
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].itemID == itemID)
                return i;
        }
        return -1;
    }

    int FindFreeSlot()
    {
        for (int i = 0; i < slotAssignments.Length; i++)
        {
            if (slotAssignments[i] == null)
                return i;
        }
        return -1;
    }

    int FindSlotOfItem(InventoryItemInfo item)
    {
        for (int i = 0; i < slotAssignments.Length; i++)
        {
            if (slotAssignments[i] == item)
                return i;
        }
        return -1;
    }

    void UpdateItemDisplay(int inventoryIndex)
    {
        var item = inventoryItems[inventoryIndex];
        int slot = FindSlotOfItem(item);

        if (slot != -1 && slotUI[slot] != null)
        {
            slotUI[slot].SetInfo(item);
        }
    }

    // Optional: Clear all inventory
    public void ClearInventory()
    {
        for (int i = 0; i < slotAssignments.Length; i++)
            ClearSlot(i);

        inventoryItems.Clear();
    }
}

// ----------------------------------------------------------------------
// DATA STRUCT
// ----------------------------------------------------------------------
[System.Serializable]
public class InventoryItemInfo
{
    public int itemID;
    public string itemName;
    public Sprite itemIcon;
    public int quantity;
    public int inventorySpace;
    public float itemCost;
}
