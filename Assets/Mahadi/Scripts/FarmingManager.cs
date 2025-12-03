using UnityEngine;

public class FarmingManager : MonoBehaviour
{
    [Header("Cooking Settings")]
    public InventoryManager inventoryManager;
    public Transform cookingSlotParent;

    [Tooltip("Empty container prefab that holds the slot UI")]
    public GameObject cookingSlotContainerPrefab;

    [Tooltip("Actual UI component that shows icon, timer, slider")]
    public FarmingItemUI cookingSlotPrefab;

    [Header("Recipe List")]
    public FarmingItemInfo[] allFarmingRecipes;


    [HideInInspector] public GameManager gameManager;
    private void Start()
    {
        gameManager = GameManager.Instance;
    }
    public void StartCooking(FarmingItemInfo recipe)
    {
        if (gameManager == null) return;
        if (gameManager.currentCoin < recipe.itemCost) return;

        gameManager.currentCoin -= recipe.itemCost;
        // STEP 1: Create the container slot
        GameObject container = Instantiate(cookingSlotContainerPrefab, cookingSlotParent);

        // STEP 2: Create the UI inside the container
        FarmingItemUI slotUI = Instantiate(cookingSlotPrefab, container.transform);

        // STEP 3: Initialize the slot
        slotUI.StartCooking(recipe, inventoryManager);

        Debug.Log("Started cooking: " + recipe.itemName);
    }
}
