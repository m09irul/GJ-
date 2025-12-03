using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public SegmentedBarUI confidenceBar;
    public BountyBarUI bountyBar;
    public CoinUI totalCoins;
    public StarsUI totalStars;

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

    void UpdateConfidenceUI(int value)
    {
        confidenceBar.SetValue(value);
    }
}
