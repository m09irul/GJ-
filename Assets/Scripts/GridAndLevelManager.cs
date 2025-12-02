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
    [SerializeField] GameObject[] chaptes;
    [SerializeField] GameObject[] levelsOfChapter1;

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
        string buttonText = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().text;

        LevelLoader.instance.loadLevelWithIndex(1 + int.Parse(buttonText));
    }


    public void OnChapter_1_Press()
    {
        int unlockedLevel = PlayerPrefs.GetInt(AllStringConstant.UNLOCKED_Chapter1_Level_BUTTON, 0);

        ManageLevelButtons(unlockedLevel, levelsOfChapter1);
    }

    /// <summary>
    /// get the saved info for levels and perfrom actions.. 
    /// </summary>
    private void ManageLevelButtons(int unlockedLevel, GameObject[] levelsOfGrid_X)
    {
        for (int i = 0; i < levelsOfChapter1.Length; i++)
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
                ManageStars(levelsOfGrid_X);

                // shows text on buttons.. 
                levelsOfGrid_X[i].GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();

                //add listeners.. 
                levelsOfGrid_X[i].GetComponent<Button>().onClick.AddListener(() => LoadLevel());
            }


        }
    }

    private void ManageStars(GameObject[] levelsOfGrid_X)
    {
        for (int i = 0; i < levelsOfGrid_X.Length; i++)
        {
            int star = PlayerPrefs.GetInt(AllStringConstant.LEVEL + (1 + (i + 1)).ToString(), 0);

            levelsOfGrid_X[i].transform.GetChild(2).GetComponent<Image>().sprite = stars[star];
        }
        
    }
}
