using System.Collections;
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
    public Transform[] itemHolders; // Should be an array of empty slot containers (e.g., UI panels)

    void Start()
    {
        // Example: Add a test item on start
        AddItem(1, "Potion", null, 10, 1, 10f);
        AddItem(1, "Potion", null, 10, 1, 10f);
        AddItem(2, "Potion", null, 10, 1, 10f);
        AddItem(3, "Potion", null, 10, 1, 10f);
        AddItem(4, "Potion", null, 10, 1, 10f);
        AddItem(5, "Potion", null, 10, 1, 10f);
    }

    void Update()
    {
        // Press 'I' to add a test item
        if (Input.GetKeyDown(KeyCode.I))
        {
            AddItem(1, "Potion", null, 1, 1, 10f);
        }
        // Press 'R' to remove a test item
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveItem(1, 1);
        }
    }

    // Adds an item to the inventory (stacks if possible)
    public bool AddItem(int itemID, string itemName, Sprite icon, int quantity, int space, float cost)
    {
        // Check if item already exists
        int existingIndex = FindItemIndexInInventory(itemID);
        if (existingIndex != -1)
        {
            inventoryItems[existingIndex].quantity += quantity;
            UpdateItemDisplay(inventoryItems[existingIndex]);
            Debug.Log($"Stacked {quantity} more of {itemName}. Total: {inventoryItems[existingIndex].quantity}");
            return true;
        }

        // Check inventory capacity
        if (inventoryItems.Count >= maxInventorySlots)
        {
            Debug.LogWarning("Inventory is full! Cannot add new item.");
            return false;
        }

        // Find first empty UI slot
        int emptyHolderIndex = FindEmptyItemHolderIndex();
        if (emptyHolderIndex == -1)
        {
            Debug.LogWarning("No empty item holder slot available in UI!");
            return false;
        }

        // Create new item data
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

        // Instantiate UI prefab in the empty slot
        InventoryItem newItemUI = Instantiate(itemPrefab, itemHolders[emptyHolderIndex]);
        newItemUI.inventoryManager = this; // ← Assign reference to manager
        newItemUI.SetInfo(newItem);        // ← Initialize UI visuals

        Debug.Log($"Added {quantity}x {itemName} to inventory.");
        return true;
    }

    // Removes quantity of an item (or entire stack if quantity <= 0)
    public bool RemoveItem(int itemID, int quantityToRemove = 1)
    {
        int itemIndex = FindItemIndexInInventory(itemID);
        if (itemIndex == -1)
        {
            Debug.LogWarning($"Item ID {itemID} not found in inventory.");
            return false;
        }

        InventoryItemInfo item = inventoryItems[itemIndex];
        item.quantity -= quantityToRemove;

        if (item.quantity <= 0)
        {
            // Remove UI from its holder
            int holderIndex = FindItemHolderIndexForItem(item);
            if (holderIndex != -1)
            {
                foreach (Transform child in itemHolders[holderIndex])
                {
                    Destroy(child.gameObject);
                }
            }

            inventoryItems.RemoveAt(itemIndex);
            Debug.Log($"Removed item ID {itemID} from inventory.");
        }
        else
        {
            // Update existing UI
            UpdateItemDisplay(item);
        }

        return true;
    }

    // Gets total quantity of an item by ID
    public int GetItemCount(int itemID)
    {
        foreach (var item in inventoryItems)
        {
            if (item.itemID == itemID)
                return item.quantity;
        }
        return 0;
    }

    // Clears entire inventory and UI
    public void ClearInventory()
    {
        inventoryItems.Clear();
        foreach (Transform holder in itemHolders)
        {
            foreach (Transform child in holder)
            {
                Destroy(child.gameObject);
            }
        }
    }

    // ─── Helper Methods ───────────────────────────────────────────────────────

    // Returns index in inventoryItems, or -1 if not found
    int FindItemIndexInInventory(int itemID)
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].itemID == itemID)
                return i;
        }
        return -1;
    }

    // Returns first itemHolder index with no children, or -1 if all full
    int FindEmptyItemHolderIndex()
    {
        for (int i = 0; i < itemHolders.Length; i++)
        {
            if (itemHolders[i].childCount == 0)
                return i;
        }
        return -1;
    }

    // Returns itemHolder index that is currently displaying this item (by reference)
    int FindItemHolderIndexForItem(InventoryItemInfo item)
    {
        for (int i = 0; i < itemHolders.Length; i++)
        {
            InventoryItem ui = itemHolders[i].GetComponentInChildren<InventoryItem>();
            if (ui != null && ui.itemInfo == item)
                return i;
        }
        return -1;
    }

    // Updates the UI display for a specific item
    void UpdateItemDisplay(InventoryItemInfo item)
    {
        int holderIndex = FindItemHolderIndexForItem(item);
        if (holderIndex != -1)
        {
            InventoryItem ui = itemHolders[holderIndex].GetComponentInChildren<InventoryItem>();
            if (ui != null)
            {
                ui.SetInfo(item); // ← Ensures visual update
            }
        }
    }
}

// Serializable data container for inventory items
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