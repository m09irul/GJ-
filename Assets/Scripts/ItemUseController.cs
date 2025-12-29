using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemUseController : MonoBehaviour
{
    public PlayerController player;
    public InventoryManager inventory;

    private InventoryItemInfo currentItem;

    public void SelectItem(InventoryItemInfo item)
    {
        currentItem = item;
    }

    public void DeselectItem()
    {
        currentItem = null;
        player.StopThrowPreview();
    }

    public void StartPreview()
    {
        if (currentItem == null) return;
        player.StartThrowPreview();
    }

    public void UseItem()
    {
        if (currentItem == null) return;

        GameObject prefab = PrefabDatabase.Instance.GetPrefab(currentItem.itemID);
        if (!prefab) return;

        player.ThrowItem(prefab);
        inventory.RemoveItem(currentItem.itemID, 1);

        if (currentItem.quantity <= 0)
            DeselectItem();
    }
}

