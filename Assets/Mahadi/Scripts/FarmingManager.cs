using UnityEngine;

public class FarmingManager : MonoBehaviour
{
    [Header("Cooking Settings")]
    public InventoryManager inventoryManager;
    public Transform cookingSlotParent;     // UI area where cooking slots appear
    public FarmingItemUI cookingSlotPrefab; // Prefab for each cooking slot

    [Header("Recipe List")]
    public FarmingItemInfo[] allFarmingRecipes;

    public void StartCooking(FarmingItemInfo recipe)
    {
        FarmingItemUI newSlot = Instantiate(cookingSlotPrefab, cookingSlotParent);
        newSlot.StartCooking(recipe, inventoryManager);

        Debug.Log("Started cooking: " + recipe.itemName);
    }
}
