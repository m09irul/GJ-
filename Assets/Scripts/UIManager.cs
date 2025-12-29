using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public GameObject hudPanel;
    [Space]
    public Button jumpButton;
    public Button throwButton;
    public Button inventoryButton;
    public GameObject inventoryPanel; 
    public GameObject farmingPanel;



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
}
