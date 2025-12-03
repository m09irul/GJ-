using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FarmingItemUI : MonoBehaviour
{
    [Header("References")]
    public Image iconImage;
    public TMP_Text timerText;
    public Slider timerSlider;

    [HideInInspector] public FarmingItemInfo recipe;
    [HideInInspector] public InventoryManager inventoryManager;

    private float remainingTime;
    private bool isCooking = false;

    public void StartCooking(FarmingItemInfo item, InventoryManager manager)
    {
        recipe = item;
        inventoryManager = manager;

        remainingTime = recipe.cookingTime;
        timerSlider.maxValue = recipe.cookingTime;
        timerSlider.value = recipe.cookingTime;

        if (iconImage) iconImage.sprite = recipe.itemIcon;

        isCooking = true;
    }

    void Update()
    {
        if (!isCooking) return;

        remainingTime -= Time.deltaTime;
        timerSlider.value = remainingTime;

        timerText.text = Mathf.Ceil(remainingTime).ToString("0") + "s";

        if (remainingTime <= 0)
        {
            FinishCooking();
        }
    }

    void FinishCooking()
    {
        isCooking = false;

        // Add to inventory
        inventoryManager.AddItem(
            recipe.itemID,
            recipe.itemName,
            recipe.itemIcon,
            recipe.outputQuantity,
            recipe.inventorySpace,
            recipe.itemCost
        );

        Destroy(gameObject); // remove finished slot UI
    }
}
