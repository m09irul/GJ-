using UnityEngine;
using System.Collections.Generic;

public class FarmingManager : MonoBehaviour

{
    public static FarmingManager Instance { get; private set; }

    [SerializeField] private Transform cookingSlotParent;
    [SerializeField] private GameObject[] farmingItems;

    private GameManager gameManager;

    // itemID → active cooking UI
    private Dictionary<int, FarmingItemUI> activeCooking = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        gameManager = GameManager.Instance;

        for (int i = 0; i < farmingItems.Length; i++)
        {
            if (i < gameManager.currentFarmableItem)
                farmingItems[i].SetActive(true);
            else
                farmingItems[i].SetActive(false);
        }

    }

    public void StartCooking(FarmingItemInfo recipe)
    {
        //  if (!gameManager) return;
        // if (gameManager.currentCoin < recipe.itemCost) return;

        // gameManager.currentCoin -= recipe.itemCost;

        //STACK IF ALREADY COOKING
        if (activeCooking.TryGetValue(recipe.itemID, out FarmingItemUI existingSlot))
        {
            if (existingSlot == null)
            {
                activeCooking.Remove(recipe.itemID);
            }
            else
            {
                existingSlot.AddToQueue();
                return;
            }
        }

        FarmingItemUI slotUI = Instantiate(
            PrefabDatabase.Instance.GetPrefab(6),
            cookingSlotParent
        ).GetComponent<FarmingItemUI>();

        slotUI.Init(
            recipe,
            () => activeCooking.Remove(recipe.itemID)
        );

        activeCooking.Add(recipe.itemID, slotUI);
    }
}
