using UnityEngine;

[CreateAssetMenu(fileName = "FarmingItem", menuName = "Game/Farming Item")]
public class FarmingItemInfo : ScriptableObject
{
    [Header("Identification")]
    public int itemID;
    public string itemName;
    public Sprite itemIcon;

    [Header("Economy")]
    public int outputQuantity = 1;
    public float cookingTime = 5f;
    public float itemCost;
    public int inventorySpace = 1;

    [Header("Rarity")]
    [Range(1, 10)] public float rarity = 1;
}
