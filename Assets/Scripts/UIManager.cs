using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
[DefaultExecutionOrder(-1)]

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public SegmentedBarUI confidenceBar;
    public BountyBarUI bountyBar;
    public CoinUI totalCoins;
    public StarsUI totalStars;
    [Header("Main Menu")]
    public GameObject mainMenuPanel;
    public GameObject optionMenuPanel;
    public GameObject chapterSelectionPanel;
    public GameObject levelSelectionPanel;
    public GameObject confirmLevelPanel;
    public GameObject helpMenuPanel;
    public GameObject creditMenuPanel;
    [Header("Lock-Unlock Menu")]
    public GameObject lockedLevelPanel;
    [Space]
    public GameObject unLockedLevelPanel;
    public TextMeshProUGUI objectiveTextUI, obstacleTextUI;
    public Image starsStatImg, packageQualityStatImg;
    [Space]
    public Button levelPlayButton;
    public Sprite[] starsImg, packageQualityImg;
    public SegmentedBarUI confidenceBarReqUI;
    public BountyBarUI bountyBarReqUI;
    [Header("In-game HUD")]
    public GameObject playFieldCanvas;
    public GameObject hudPanel;
    [Space]
    public Button jumpButton;
    public Button pick_deliverButton;
    public Button throwButton;
    public Button inventoryButton;
    public GameObject inventoryPanel;
    public GameObject farmingPanel;
    [Header("Win Menu")]
    public GameObject winPanel;
    public Image starGot;
    public Image packageQualityGot;
    public BountyBarUI bountyRemaining;
    public SegmentedBarUI confidenceRemaining;
    public TextMeshProUGUI timeTook;
    [Space]
    public Volume postProcessVolume;
    ColorAdjustments color;
    Vignette vignette;
    public GameObject gameOverPanel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void UpdateGameOverUI()
    {
        Time.timeScale = .02f;

        postProcessVolume.profile.TryGet(out color);
        postProcessVolume.profile.TryGet(out vignette);

        StartCoroutine(WastedRoutine());
    }

    public void PlayBustedEffect()
    {
        StopAllCoroutines();

    }
    public void OnInventoryButtonClick()
    {
        inventoryPanel.SetActive(true);
        AudioManager.instance.stop("inventory open");


    }
    IEnumerator WastedRoutine()
    {
        float t = 0f;
        float duration = 3f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / duration;

            color.saturation.value = Mathf.Lerp(0, -80, lerp);
            color.postExposure.value = Mathf.Lerp(0, -0.5f, lerp);

            if (vignette != null)
                vignette.intensity.value = Mathf.Lerp(0, 0.35f, lerp);

            yield return null;
        }

        gameOverPanel.SetActive(true);
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(5f);

        GameManager.Instance.OnRestartPress();
    }



    public void OnItemSelected()
    {
        throwButton.gameObject.SetActive(true);
    }
    public void ToggleInventoryButton()
    {
        inventoryButton.gameObject.SetActive(!inventoryButton.IsActive());
    }


    public void OnItemDeselected()
    {
        throwButton.gameObject.SetActive(false);
    }
    public void OnThrowButtonDown()
    {
        GameManager.Instance.player.StartThrowPreview();
    }

    public void OnThrowButtonUp()
    {
        GameManager.Instance.ThrowItem();
    }
    public void UpdateConfidenceUI(int value)
    {
        confidenceBar.SetValue(value);
    }
    public void UpdatBountyUI(int value)
    {
        bountyBar.SetValue(value);
    }
    public void UpdatCoinUI(int value)
    {
        totalCoins.SetValue(value);
    }
    public void UpdatStarUI(int value)
    {
        totalStars.SetValue(value);
    }
    public void UpdatLockLevelUI(int value)
    {
        totalStars.SetValue(value);
    }
    public void UpdateUnlockLevelUI(string objecTiveText, string obstaclesText, int starStat, int packageQualityStat)
    {
        objectiveTextUI.text = objecTiveText;
        obstacleTextUI.text = obstaclesText;
        starsStatImg.sprite = starsImg[starStat];
        packageQualityStatImg.sprite = packageQualityImg[packageQualityStat];

        levelPlayButton.gameObject.SetActive(true);
    }
    public void UpdateLockLevelUI(int confidenceReq, int bountyReq)
    {
        confidenceBarReqUI.SetValue(confidenceReq);
        bountyBarReqUI.SetValue(bountyReq);

        levelPlayButton.gameObject.SetActive(false);
    }
    public void UpdateLevelCompletionUI(int confidence, int bounty, int starStat, int packageQualityStat, string m_time)
    {
        winPanel.SetActive(true);

        confidenceRemaining.SetValue(confidence);
        bountyRemaining.SetValue(bounty);
        starGot.sprite = starsImg[starStat];
        packageQualityGot.sprite = packageQualityImg[packageQualityStat - 1];
        timeTook.text = m_time;
    }
}
