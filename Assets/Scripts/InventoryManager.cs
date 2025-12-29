using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Transform itemsParent;

    // itemID → InventoryItem (UI holds the data)
    private readonly Dictionary<int, InventoryItem> inventory = new();

    public InventoryItem SelectedItem { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // --------------------------------------------------
    // SELECTION
    // --------------------------------------------------
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
        UIManager.Instance.OnItemSelected();
    }

    public void DeselectCurrent()
    {
        if (SelectedItem == null)
            return;

        SelectedItem.SetSelected(false);
        SelectedItem = null;
        UIManager.Instance.OnItemDeselected();
    }

    // --------------------------------------------------
    // ADD ITEM (STACK FIRST)
    // --------------------------------------------------
    public bool AddItem(int itemID, Sprite icon)
    {
        if(inventory.Count == 0)
            UIManager.Instance.ToggleInventoryButton();

        if (inventory.TryGetValue(itemID, out InventoryItem existingItem))
        {
            existingItem.GetItemInfo().quantity++;
            existingItem.Refresh();
            return true;
        }

        InventoryItemInfo newData = new InventoryItemInfo
        {
            itemID = itemID,
            itemIcon = icon,
            quantity = 1
        };

        InventoryItem uiItem = Instantiate(PrefabDatabase.Instance.GetPrefab(5), itemsParent).GetComponent<InventoryItem>();
        uiItem.Init(newData);

        inventory.Add(itemID, uiItem);
        return true;
    }

    // --------------------------------------------------
    // REMOVE ITEM
    // --------------------------------------------------
    public bool RemoveItem(int itemID, int amount = 1)
    {
        if (!inventory.TryGetValue(itemID, out InventoryItem item))
            return false;

        item.GetItemInfo().quantity -= amount;

        if (item.GetItemInfo().quantity <= 0)
        {
            RemoveItemCompletely(itemID);
        }
        else
        {
            item.Refresh();
        }

        return true;
    }

    // --------------------------------------------------
    // INTERNAL
    // --------------------------------------------------
    private void RemoveItemCompletely(int itemID)
    {
        if (!inventory.TryGetValue(itemID, out InventoryItem item))
            return;

        if (SelectedItem == item)
            DeselectCurrent();

        Destroy(item.gameObject);
        inventory.Remove(itemID);

        if(inventory.Count == 0)
            UIManager.Instance.ToggleInventoryButton();
    }
}

[System.Serializable]
public class InventoryItemInfo
{
    public int itemID;
    public Sprite itemIcon;
    public int quantity;
}
