using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour {
    private Mana mana;
    private float barMaskWidth;
    private RectTransform barMaskRectTransform;
    private RectTransform edgeRectTransform;
    private RawImage barRawImage;

    private void Awake() {

        barMaskRectTransform = transform.Find("barMask").GetComponent<RectTransform>();
        barRawImage = transform.Find("barMask").Find("bar").GetComponent<RawImage>();
        edgeRectTransform = transform.Find("edge").GetComponent<RectTransform>();

        barMaskWidth = barMaskRectTransform.sizeDelta.x;

        mana = new Mana();

    }

    private void Update() {
        mana.Update();

        Rect uvRect = barRawImage.uvRect;
        uvRect.x += .35f * Time.deltaTime;
        barRawImage.uvRect = uvRect;

        Vector2 barMaskSizeDelta = barMaskRectTransform.sizeDelta;
        barMaskSizeDelta.x = mana.GetManaNormalized() * barMaskWidth;
        barMaskRectTransform.sizeDelta = barMaskSizeDelta;

        edgeRectTransform.anchoredPosition = new Vector2(mana.GetManaNormalized() * barMaskWidth, 0);

        edgeRectTransform.gameObject.SetActive(mana.GetManaNormalized() < 1f);

    }

}


public class Mana {

    public const int MANA_MAX = 700;

    public float manaAmount;
    private float manaRegenAmount;

    public Mana() {
        manaAmount = MANA_MAX;
        manaRegenAmount = 700/5f;
    }

    public void Update() {
        manaAmount -= manaRegenAmount * Time.deltaTime;
        manaAmount = Mathf.Clamp(manaAmount, 0f, MANA_MAX);


    }


    public float GetManaNormalized() {
        return manaAmount / MANA_MAX;
    }

}