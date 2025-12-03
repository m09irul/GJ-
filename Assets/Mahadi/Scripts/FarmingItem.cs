using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FarmingItem : MonoBehaviour
{
    public FarmingItemInfo recipe;
    public FarmingManager farmingManager;

    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text timeText;

    private void Awake()
    {
        if (farmingManager == null)
        {
            farmingManager = GetComponentInParent<FarmingManager>();
        }
    }
    void Start()
    {
        iconImage.sprite = recipe.itemIcon;
        nameText.text = recipe.itemName;
        timeText.text = recipe.cookingTime + "s";
    }

  
    public void Cook()
    {
        farmingManager.StartCooking(recipe);
    }
}
