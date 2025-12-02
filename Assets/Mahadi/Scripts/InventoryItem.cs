using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    public InventoryItemInfo itemInfo;
    public Image iconImage;
    public TMP_Text quantityText;

    // Reference to the inventory manager (set when instantiated)
    public InventoryManager inventoryManager;

    public void SetInfo(InventoryItemInfo info)
    {
        itemInfo = info;
        if (iconImage != null && info.itemIcon != null) iconImage.sprite = info.itemIcon;
        if (quantityText != null) quantityText.text = info.quantity.ToString();
    }

    public void RemoveOneItem()
    {
        if (itemInfo == null)
        {
            Debug.LogWarning("No item info assigned to this UI element.");
            return;
        }

        itemInfo.quantity--;

        if (quantityText != null)
            quantityText.text = itemInfo.quantity.ToString();

        if (itemInfo.quantity <= 0)
        {
            if (inventoryManager != null)
            {
                inventoryManager.RemoveItem(itemInfo.itemID, 1);
            }
            else
            {
                Debug.LogError("InventoryManager reference not set on InventoryItem prefab!");
            }
        }
    }
}