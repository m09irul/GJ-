using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour {
    private Mana mana;
    private float barMaskWidth;
    private RectTransform barMaskRectTransform;
    private RectTransform edgeRectTransform;
    private RawImage barRawImage;

    private bool active = false;

    public System.Action OnManaFinished;    // Callback event

    private void Awake() {

        barMaskRectTransform = transform.Find("barMask").GetComponent<RectTransform>();
        barRawImage = transform.Find("barMask").Find("bar").GetComponent<RawImage>();
        edgeRectTransform = transform.Find("edge").GetComponent<RectTransform>();

        barMaskWidth = barMaskRectTransform.sizeDelta.x;

        mana = new Mana();

        gameObject.SetActive(false);     // Hide bar on start
    }

    public void Activate() 
    {
        mana.ResetMana();               // Fill mana
        active = true;
        gameObject.SetActive(true);     // Show bar
    }

    private void Update() 
    {
        if (!active) return;

        mana.Update();

        // Update UI
        Rect uvRect = barRawImage.uvRect;
        uvRect.x += .35f * Time.deltaTime;
        barRawImage.uvRect = uvRect;

        Vector2 barMaskSizeDelta = barMaskRectTransform.sizeDelta;
        barMaskSizeDelta.x = mana.GetManaNormalized() * barMaskWidth;
        barMaskRectTransform.sizeDelta = barMaskSizeDelta;

        edgeRectTransform.anchoredPosition = new Vector2(mana.GetManaNormalized() * barMaskWidth, 0);
        edgeRectTransform.gameObject.SetActive(mana.GetManaNormalized() < 1f);

        // Mana finished?
        if (mana.manaAmount <= 0f)
        {
            active = false;
            gameObject.SetActive(false);  // Hide bar when finished

            OnManaFinished?.Invoke();     // Fire callback
        }
    }
}
public class Mana {

    public const int MANA_MAX = 700;

    public float manaAmount;
    private float manaRegenAmount;

    public Mana() {
        manaAmount = 0;   // Start empty
        manaRegenAmount = 700 / 5f;
    }

    public void ResetMana() {
        manaAmount = MANA_MAX;  // Refill at pickup
    }

    public void Update() {
        manaAmount -= manaRegenAmount * Time.deltaTime;
        manaAmount = Mathf.Clamp(manaAmount, 0f, MANA_MAX);
    }

    public float GetManaNormalized() {
        return manaAmount / MANA_MAX;
    }
}
