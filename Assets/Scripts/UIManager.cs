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
    public GameObject lockedLevelPanel;
    public GameObject unLockedLevelPanel;
    public Button levelPlayButton;

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
        //totalCoins.SetValue(value);
    }
    public void UpdatStarUI(int value)
    {
        //totalStars.SetValue(value);
    }
}
