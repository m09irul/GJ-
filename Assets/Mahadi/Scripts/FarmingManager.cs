using UnityEngine;
using System.Collections.Generic;

public class FarmingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Transform cookingSlotParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject cookingSlotContainerPrefab;
    [SerializeField] private FarmingItemUI cookingSlotPrefab;

    private GameManager gameManager;

    // itemID → active cooking UI
    private Dictionary<int, FarmingItemUI> activeCooking = new();

    private void Awake()
    {
        gameManager = GameManager.Instance;
    }

    public void StartCooking(FarmingItemInfo recipe)
    {
      //  if (!gameManager) return;
       // if (gameManager.currentCoin < recipe.itemCost) return;

       // gameManager.currentCoin -= recipe.itemCost;

        //STACK IF ALREADY COOKING
        if (activeCooking.TryGetValue(recipe.itemID, out FarmingItemUI existingSlot))
        {
            existingSlot.AddToQueue();
            return;
        }

        //CREATE NEW SLOT
        GameObject container = Instantiate(cookingSlotContainerPrefab, cookingSlotParent);
        FarmingItemUI slotUI = Instantiate(cookingSlotPrefab, container.transform);

        slotUI.Init(
            recipe,
            inventoryManager,
            container.transform,
            () => activeCooking.Remove(recipe.itemID)
        );

        activeCooking.Add(recipe.itemID, slotUI);
    }
}
