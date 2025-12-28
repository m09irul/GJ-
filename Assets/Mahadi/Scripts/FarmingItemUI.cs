using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class FarmingItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text stackText;
    [SerializeField] private Slider timerSlider;

    private FarmingItemInfo recipe;
    private InventoryManager inventoryManager;
    private int queuedCount = 1;
    private Tween timerTween;
    private Action onFinishedAll;
    void Start()
    {
        inventoryManager = InventoryManager.Instance;
    }

    public void Init(
        FarmingItemInfo item,
        Action onFinished
    )
    {
        recipe = item;
        onFinishedAll = onFinished;

        iconImage.sprite = recipe.itemIcon;
        UpdateStackUI();

        timerSlider.maxValue = recipe.cookingTime;

        PlaySpawnAnimation();
        StartNextCooking();
    }

    //CALLED WHEN SAME ITEM IS ADDED AGAIN
    public void AddToQueue()
    {
        queuedCount++;
        UpdateStackUI();
        PlayStackBump();
    }

    private void StartNextCooking()
    {
        timerSlider.value = recipe.cookingTime;

        timerTween = DOVirtual.Float(
            recipe.cookingTime,
            0f,
            recipe.cookingTime,
            UpdateTimer
        ).SetEase(Ease.Linear)
        .SetUpdate(true)
        .OnComplete(FinishOneCooking);
    }

    private void UpdateTimer(float value)
    {
        timerSlider.value = value;
        timerText.text = $"{Mathf.Ceil(value)}s";
    }

    private void FinishOneCooking()
    {
        inventoryManager.AddItem(
            recipe.itemID,
            recipe.itemIcon
        );

        queuedCount--;
        UpdateStackUI();

        if (queuedCount > 0)
        {
            StartNextCooking();
        }
        else
        {
            PlayCompleteAnimation();
        }
    }

    private void UpdateStackUI()
    {
        stackText.gameObject.SetActive(queuedCount > 1);
        stackText.text = $"x{queuedCount}";
    }

    //ANIMATIONS
    private void PlaySpawnAnimation()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }

    private void PlayStackBump()
    {
        stackText.transform
            .DOPunchScale(Vector3.one * 0.3f, 0.2f, 6, 0.8f);
    }

    private void PlayCompleteAnimation()
    {
        transform
            .DOScale(0f, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                onFinishedAll?.Invoke();
                Destroy(gameObject);
            });
    }

    private void OnDestroy()
    {
        timerTween?.Kill();
    }
}
