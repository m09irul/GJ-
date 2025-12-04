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

    void Start()
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
        for (int i = 0; i < levelsOfChapter1.Length; i++)
        {
            levelsOfChapter1[i].OnlevelPressed += CheckLevelPlayableStatus;
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
            uIManager.levelPlayButton.onClick.AddListener(()=> LevelLoader.instance.loadLevelWithIndex(levelNo + 1));
        }
        else
        {

            uIManager.lockedLevelPanel.SetActive(true);
            uIManager.unLockedLevelPanel.SetActive(false);
            uIManager.levelPlayButton.gameObject.SetActive(false);
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
