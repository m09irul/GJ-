using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Level : MonoBehaviour
{
    public int levelNo;
    public int minRequiredConfidence;
    public int maxRequiredBounty;
    public int requiredStarToUnlock;
    public bool isLevelUnlocked { get; private set; }
    public event Action<int> OnlevelPressed;
    public TextMeshProUGUI levelNoText, levelStarRequiredText;


    void OnEnable()
    {
        if (levelNo <= PlayerPrefs.GetInt(AllStringConstant.UNLOCKED_Chapter1_Level_BUTTON, 1))
        {
            isLevelUnlocked = true;
            GetComponent<Button>().interactable = true;

            //disable lock image.. 
            transform.GetChild(1).gameObject.SetActive(false);

            //active star image..
            transform.GetChild(2).gameObject.SetActive(true);

            // shows text on buttons.. 
            levelNoText.text = levelNo.ToString();

            //add listeners.. 
            GetComponent<Button>().onClick.AddListener(() => Press());


        }
        else
        {
            isLevelUnlocked = false;

            GetComponent<Button>().interactable = false;

            //activate lock image..
            transform.GetChild(1).gameObject.SetActive(true);

            //disable stars..
            transform.GetChild(2).gameObject.SetActive(false);

            // hide text on buttons.. 
            levelNoText.text = AllStringConstant.BLANK;
            levelStarRequiredText.text = requiredStarToUnlock.ToString();
        }

    }

    void Press()
    {
        OnlevelPressed?.Invoke(levelNo);
    }
}
