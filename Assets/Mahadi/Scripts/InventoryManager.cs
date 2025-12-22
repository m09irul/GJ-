using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxInventorySlots = 10;

    [Header("UI")]
    [SerializeField] private InventoryItem itemPrefab;
    [SerializeField] private Transform[] itemHolders;

    private readonly List<InventoryItemInfo> inventoryItems = new();
    private InventoryItemInfo[] slotAssignments;
    private InventoryItem[] slotUI;
    public InventoryItem SelectedItem { get; private set; }

    [SerializeField] private GameObject throwButton;
    public GameObject throwItem;

    private void Awake()
    {
        slotAssignments = new InventoryItemInfo[itemHolders.Length];
        slotUI = new InventoryItem[itemHolders.Length];
    }
    public void SelectItem(InventoryItem item)
    {
        if (SelectedItem == item)
        {
            DeselectCurrent();
            return;
        }

        DeselectCurrent();

        SelectedItem = item;
        item.SetSelected(true);

        if (throwButton != null)
            throwButton.SetActive(true);
    }

    public void DeselectCurrent()
    {
        if (SelectedItem != null)
        {
            SelectedItem.SetSelected(false);
            SelectedItem = null;
        }

        if (throwButton != null)
            throwButton.SetActive(false);
    }
    public void OnThrowButtonDown()
    {
        GameManager.Instance.player
            .GetComponent<PlayerController>()
            .StartThrowPreview();
    }

    public void OnThrowButtonUp()
    {
        if (SelectedItem == null) return;

        InventoryItemInfo info = SelectedItem.GetItemInfo();

        GameObject prefab = throwItem;
        if (!prefab) return;

        PlayerController player = GameManager.Instance.player.GetComponent<PlayerController>();

        // STOP PREVIEW
        player.StopThrowPreview();

        // THROW
        player.ThrowItem(prefab);

        // REMOVE ITEM
        RemoveItem(info.itemID, 1);

        DeselectCurrent();
    }

    // ------------------------------------------------------------------
    // ADD ITEM (STACK FIRST)
    // ------------------------------------------------------------------
    public bool AddItem(int itemID, string itemName, Sprite icon, int space, float cost, int amount = 1)
    {
        InventoryItemInfo existing = FindItem(itemID);
        if (existing != null)
        {
            existing.quantity += amount;
            RefreshUI(existing);
            return true;
        }

        int freeSlot = FindFreeSlot();
        if (freeSlot == -1)
        {
            Debug.LogWarning("Inventory full.");
            return false;
        }

        InventoryItemInfo newItem = new()
        {
            itemID = itemID,
            itemName = itemName,
            itemIcon = icon,
            inventorySpace = space,
            itemCost = cost,
            quantity = amount
        };

        inventoryItems.Add(newItem);

        InventoryItem ui = Instantiate(itemPrefab, itemHolders[freeSlot]);
        ui.Init(newItem, this);

        slotAssignments[freeSlot] = newItem;
        slotUI[freeSlot] = ui;

        return true;
    }

    // ------------------------------------------------------------------
    // REMOVE ITEM
    // ------------------------------------------------------------------
    public bool RemoveItem(int itemID, int amount = 1)
    {
        InventoryItemInfo item = FindItem(itemID);
        if (item == null) return false;

        item.quantity -= amount;

        if (item.quantity <= 0)
        {
            RemoveItemCompletely(item);
        }
        else
        {
            RefreshUI(item);
        }

        return true;
    }

    // ------------------------------------------------------------------
    // INTERNAL
    // ------------------------------------------------------------------
    private void RemoveItemCompletely(InventoryItemInfo item)
    {
        int slot = FindSlot(item);
        if (slot != -1)
        {
            Destroy(slotUI[slot].gameObject);
            slotAssignments[slot] = null;
            slotUI[slot] = null;
        }

        inventoryItems.Remove(item);
    }

    private InventoryItemInfo FindItem(int itemID)
    {
        return inventoryItems.Find(i => i.itemID == itemID);
    }

    private int FindFreeSlot()
    {
        for (int i = 0; i < slotAssignments.Length; i++)
            if (slotAssignments[i] == null)
                return i;

        return -1;
    }

    private int FindSlot(InventoryItemInfo item)
    {
        for (int i = 0; i < slotAssignments.Length; i++)
            if (slotAssignments[i] == item)
                return i;

        return -1;
    }

    private void RefreshUI(InventoryItemInfo item)
    {
        int slot = FindSlot(item);
        if (slot != -1 && slotUI[slot] != null)
            slotUI[slot].Refresh();
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