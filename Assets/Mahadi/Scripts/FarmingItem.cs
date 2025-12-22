using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class FarmingItem : MonoBehaviour
{
    [SerializeField] private FarmingItemInfo recipe;
    [SerializeField] private FarmingManager farmingManager;

    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text costText;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (!farmingManager)
            farmingManager = GetComponentInParent<FarmingManager>();
    }

    private void Start()
    {
        iconImage.sprite = recipe.itemIcon;
        nameText.text = recipe.itemName;
        timeText.text = $"{recipe.cookingTime}s";
        costText.text = recipe.itemCost.ToString();

        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        Debug.Log(34343434);
        farmingManager.StartCooking(recipe);
    }
}
