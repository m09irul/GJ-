/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour {
    [SerializeField] GameObject pauseButton;
    GameController m_gameController;
    private Mana mana;
    private float barMaskWidth;
    private RectTransform barMaskRectTransform;
    private RectTransform edgeRectTransform;
    private RawImage barRawImage;

    private void Awake() {

        pauseButton.SetActive(false);
        m_gameController = FindObjectOfType<GameController>();

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

        if (mana.manaAmount <= 0)
        {
            m_gameController.StartGame();
            pauseButton.SetActive(true);
            gameObject.SetActive(false);
        }
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