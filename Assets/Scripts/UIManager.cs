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

    void Awake()
    {
        Instance = this;
    }

    // void Start()
    // {
    //     GameManager.Instance.OnConfidenceChanged += UpdateConfidenceUI;

    //     // Set initial UI from loaded data
    //     UpdateConfidenceUI(GameManager.Instance.currentConfidence);
    // }

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
