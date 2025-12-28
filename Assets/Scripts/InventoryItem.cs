using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private GameObject selectionHighlight;

    private InventoryItemInfo itemInfo;
    private InventoryManager inventoryManager;
    private bool isSelected;

    public void Init(InventoryItemInfo info)
    {
        itemInfo = info;
        inventoryManager = InventoryManager.Instance;

        iconImage.sprite = info.itemIcon;
        Refresh();

        selectionHighlight.SetActive(false);
    }

    public void Refresh()
    {
        quantityText.text = itemInfo.quantity.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        inventoryManager.SelectItem(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        selectionHighlight.SetActive(selected);

        if (selected)
            PlaySelectAnim();
        else
            PlayDeselectAnim();
    }

    public InventoryItemInfo GetItemInfo()
    {
        return itemInfo;
    }

    // Animations
    private void PlaySelectAnim()
    {
        transform.DOScale(1.1f, 0.15f).SetEase(Ease.OutBack).SetLink(gameObject);
    }

    private void PlayDeselectAnim()
    {
        transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad).SetLink(gameObject);
    }
    private void OnDestroy()
{
    // Kill all tweens linked to this object
    DOTween.Kill(transform);
}
}
