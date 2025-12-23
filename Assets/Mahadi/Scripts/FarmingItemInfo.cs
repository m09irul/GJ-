using UnityEngine;

[CreateAssetMenu(fileName = "FarmingItem", menuName = "Game/Farming Item")]
public class FarmingItemInfo : ScriptableObject
{
    [Header("Identification")]
    public int itemID; // it must match what is in the prefab database to work accordingly :3
    public string itemName;
    public Sprite itemIcon;

    [Header("Economy")]
    public float cookingTime = 5f;
    public int itemCost;
    public int inventorySpace = 1;
}
