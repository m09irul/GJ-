using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class GridAndLevelManager : MonoBehaviour
{
    [Tooltip("no star -> 3 star")]
    [SerializeField] Sprite[] stars;
    [SerializeField] GameObject[] grids;
    [SerializeField] GameObject[] levelsOfGrid1;
    [SerializeField] GameObject[] levelsOfGrid2;
    [SerializeField] GameObject[] levelsOfGrid3;
    [SerializeField] GameObject[] levelsOfGrid4;
    [SerializeField] TextMeshProUGUI totalPoint;

    private void Start()
    {
        totalPoint.text = AllStringConstant.TOTAL_POINTS_TEXT + PlayerPrefs.GetInt(AllStringConstant.TOTAL_POINTS, 0).ToString();
    }
    /// <summary>
    /// when level are clicked..
    /// </summary>
    void LoadLevel()
    {
        string buttonTag = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.tag;
        string buttonText = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().text;

        switch (buttonTag)
        {
            case AllStringConstant.GRID_1_BUTTON:

                LevelLoader.instance.loadLevelWithIndex(1 + int.Parse(buttonText) + 0);

                break;

            case AllStringConstant.GRID_2_BUTTON:

                LevelLoader.instance.loadLevelWithIndex(1 + int.Parse(buttonText) + 10);

                break;

            case AllStringConstant.GRID_3_BUTTON:

                LevelLoader.instance.loadLevelWithIndex(1 + int.Parse(buttonText) + 20);

                break;

            case AllStringConstant.GRID_4_BUTTON:

                LevelLoader.instance.loadLevelWithIndex(1 + int.Parse(buttonText) + 30);

                break;
        }
    }

    /// <summary>
    /// when grid menu opens system will check for saved value and will perform action based on that..
    /// </summary>
    public void OnGridMenuStart()
    {
        int unlockedGrid = PlayerPrefs.GetInt(AllStringConstant.UNLOCKED_GRID_BUTTON, 0);

        for (int i = 0; i < grids.Length; i++)
        {
            if(i > unlockedGrid)
            {
                grids[i].GetComponent<Button>().interactable = false;
            }
            else
                grids[i].GetComponent<Button>().interactable = true;
        }
    }

    public void OnGrid_1_Press()
    {
        int unlockedLevel = PlayerPrefs.GetInt(AllStringConstant.UNLOCKED_GRID1_Level_BUTTON, 0);

        ManageLevelButtons(unlockedLevel, levelsOfGrid1);
    }

    public void OnGrid_2_Press()
    {
        int unlockedLevel = PlayerPrefs.GetInt(AllStringConstant.UNLOCKED_GRID2_Level_BUTTON, 0);

        ManageLevelButtons(unlockedLevel, levelsOfGrid2);

        
    }

    public void OnGrid_3_Press()
    {
        int unlockedLevel = PlayerPrefs.GetInt(AllStringConstant.UNLOCKED_GRID3_Level_BUTTON, 0);

        ManageLevelButtons(unlockedLevel, levelsOfGrid3);
    }

    public void OnGrid_4_Press()
    {
        int unlockedLevel = PlayerPrefs.GetInt(AllStringConstant.UNLOCKED_GRID4_Level_BUTTON, 0);

        ManageLevelButtons(unlockedLevel, levelsOfGrid4);
    }

    /// <summary>
    /// get the saved info for levels and perfrom actions.. 
    /// </summary>
    private void ManageLevelButtons(int unlockedLevel, GameObject[] levelsOfGrid_X)
    {
        //get the tag..
        string tag = levelsOfGrid_X[0].tag;

        for (int i = 0; i < levelsOfGrid1.Length; i++)
        {
            // if levels are locked
            if (i > unlockedLevel)
            {
                levelsOfGrid_X[i].GetComponent<Button>().interactable = false;

                //activate lock image..
                levelsOfGrid_X[i].transform.GetChild(1).gameObject.SetActive(true);

                //disable stars..
                levelsOfGrid_X[i].transform.GetChild(2).gameObject.SetActive(false);

                // hide text on buttons.. 
                levelsOfGrid_X[i].GetComponentInChildren<TextMeshProUGUI>().text = AllStringConstant.BLANK;
            }
            else
            {
                levelsOfGrid_X[i].GetComponent<Button>().interactable = true;

                //disable lock image.. 
                levelsOfGrid_X[i].transform.GetChild(1).gameObject.SetActive(false);

                //active star image..
                levelsOfGrid_X[i].transform.GetChild(2).gameObject.SetActive(true);
                //manages how many star need to be shown..
                ManageStars(tag, levelsOfGrid_X);

                // shows text on buttons.. 
                levelsOfGrid_X[i].GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();

                //add listeners.. 
                levelsOfGrid_X[i].GetComponent<Button>().onClick.AddListener(() => LoadLevel());
            }


        }
    }

    /// <summary>
    /// save levels info.. [ save system = LevelX ] (X = build number of levels - 1)
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="levelsOfGrid_X"></param>
    private void ManageStars(string tag, GameObject[] levelsOfGrid_X)
    {
        switch (tag)
        {
            case AllStringConstant.GRID_1_BUTTON:

                for (int i = 0; i < levelsOfGrid_X.Length; i++)
                {
                   int star = PlayerPrefs.GetInt(AllStringConstant.LEVEL + (1 + (i + 1) + 0).ToString(), 0);

                    levelsOfGrid_X[i].transform.GetChild(2).GetComponent<Image>().sprite = stars[star];
                }

                break;

            case AllStringConstant.GRID_2_BUTTON:

                for (int i = 0; i < levelsOfGrid_X.Length; i++)
                {
                    int star = PlayerPrefs.GetInt(AllStringConstant.LEVEL + (1 + (i + 1) + 10).ToString(), 0);

                    levelsOfGrid_X[i].transform.GetChild(2).GetComponent<Image>().sprite = stars[star];
                }

                break;

            case AllStringConstant.GRID_3_BUTTON:

                for (int i = 0; i < levelsOfGrid_X.Length; i++)
                {
                    int star = PlayerPrefs.GetInt(AllStringConstant.LEVEL + (1 + (i + 1) + 20).ToString(), 0);

                    levelsOfGrid_X[i].transform.GetChild(2).GetComponent<Image>().sprite = stars[star];
                }

                break;

            case AllStringConstant.GRID_4_BUTTON:

                for (int i = 0; i < levelsOfGrid_X.Length; i++)
                {
                    int star = PlayerPrefs.GetInt(AllStringConstant.LEVEL + (1 + (i + 1) + 30).ToString(), 0);

                    levelsOfGrid_X[i].transform.GetChild(2).GetComponent<Image>().sprite = stars[star];
                }

                break;
        }
    }
}
