
using UnityEngine;
using TMPro;


public class MainMenuManager : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI totalPoint;

    private void Start()
    {
        totalPoint.text = AllStringConstant.TOTAL_POINTS_TEXT + PlayerPrefs.GetInt(AllStringConstant.TOTAL_POINTS, 0).ToString();
    }

    public void OnPlay_Press()
    {
        LevelLoader.instance.loadLevelWithIndex(2);
    }
}
