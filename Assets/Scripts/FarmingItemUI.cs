using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using System.Collections;

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
    private bool completionPending;


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
        timerTween?.Kill(false);

        timerSlider.value = recipe.cookingTime;
        UpdateTimer(recipe.cookingTime);

        timerTween = DOVirtual.Float(
            recipe.cookingTime,
            0f,
            recipe.cookingTime,
            UpdateTimer
        )
        .SetEase(Ease.Linear)
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

        // 🔥 Defer final decision by one frame
        if (!completionPending)
        {
            completionPending = true;
            StartCoroutine(ResolveAfterFrame());
        }
    }
    IEnumerator ResolveAfterFrame()
    {
        yield return null; // wait one frame

        completionPending = false;

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
                StartCoroutine(DestroyNextFrame());
            });
    }
    private IEnumerator DestroyNextFrame()
    {
        // 🔥 REMOVE FROM MANAGER FIRST
        onFinishedAll?.Invoke();

        // then wait one frame for safety
        yield return null;

        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        timerTween?.Kill(this);
    }
}
