using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class ChapterAndLevelManager : MonoBehaviour
{
    [Tooltip("no star -> 3 star")]
    [SerializeField] Sprite[] stars;
    [SerializeField] GameObject[] chaptes;
    [SerializeField] Level[] levelsOfChapter1;

    UIManager uIManager;
    GameManager gameManager;

    void Awake()
    {
        uIManager = UIManager.Instance;
        gameManager = GameManager.Instance;
    }
    void CheckLevelPlayableStatus()
    {
        string buttonText = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().text;

        LevelLoader.instance.loadLevelWithIndex(1 + int.Parse(buttonText));
    }
    void LoadLevel()
    {
        string buttonText = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().text;

        LevelLoader.instance.loadLevelWithIndex(1 + int.Parse(buttonText));
    }


    public void OnChapter_1_Press()
    {
        uIManager.levelSelectionPanel.SetActive(true);
        uIManager.chapterSelectionPanel.SetActive(false);
        ManageLevelButtons();
    }

    /// <summary>
    /// get the saved info for levels and perfrom actions.. 
    /// </summary>
    private void ManageLevelButtons()
    {
        var totalStar = 0;

        for (int i = 0; i < levelsOfChapter1.Length; i++)
        {
            levelsOfChapter1[i].OnlevelPressed += CheckLevelPlayableStatus;

            totalStar += ManageStars(i + 1);
        }
        PlayerPrefs.SetInt(AllStringConstant.STAR, totalStar);
        uIManager.totalStars.SetValue(totalStar);

        SetLevelUnlockStat(totalStar);

        RefreshUI();
    }

    void SetLevelUnlockStat(int totalStar)
    {
        for (int i = 0; i < levelsOfChapter1.Length; i++)
        {
            if(totalStar >= levelsOfChapter1[i].requiredStarToUnlock)
                PlayerPrefs.SetInt(AllStringConstant.UNLOCKED_Chapter1_Level_BUTTON, i + 1);
        }
    }
    void RefreshUI()
    {
        for (int i = 0; i < levelsOfChapter1.Length; i++)
        {
            levelsOfChapter1[i].gameObject.SetActive(false);
            levelsOfChapter1[i].gameObject.SetActive(true);
        }
    }
    void CheckLevelPlayableStatus(int levelNo)
    {
        uIManager.confirmLevelPanel.SetActive(true);
        uIManager.UpdateConfidenceUI(gameManager.currentConfidence);
        uIManager.UpdatBountyUI(gameManager.currentBounty);

        //requirment met
        if (gameManager.currentConfidence >= levelsOfChapter1[levelNo - 1].minRequiredConfidence &&
            gameManager.currentBounty <= levelsOfChapter1[levelNo - 1].maxRequiredBounty)
        {

            uIManager.lockedLevelPanel.SetActive(false);
            uIManager.unLockedLevelPanel.SetActive(true);
            uIManager.levelPlayButton.gameObject.SetActive(true);
            uIManager.levelPlayButton.onClick.AddListener(() => LevelLoader.instance.loadLevelWithIndex(levelNo + 1));
        }
        else
        {

            uIManager.lockedLevelPanel.SetActive(true);
            uIManager.unLockedLevelPanel.SetActive(false);
            uIManager.levelPlayButton.gameObject.SetActive(false);
        }
    }
    private int ManageStars(int levelIndex)
    {
        var star = LevelSaveManager.GetStars(levelIndex);

        levelsOfChapter1[levelIndex - 1].transform.GetChild(2).GetComponent<Image>().sprite = uIManager.starsImg[star];

        return star;
    }
}
