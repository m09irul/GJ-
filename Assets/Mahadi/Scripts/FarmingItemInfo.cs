using UnityEngine;

[CreateAssetMenu(fileName = "FarmingItem", menuName = "Game/Farming Item")]
public class FarmingItemInfo : ScriptableObject
{
    [Header("Identification")]
    public int itemID;
    public string itemName;
    public Sprite itemIcon;

    [Header("Economy")]
    public float cookingTime = 5f;
    public int itemCost;
    public int inventorySpace = 1;
}
