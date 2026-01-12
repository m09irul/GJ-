using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[DefaultExecutionOrder(-5)]
public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance;
    private bool loadFromLevelComplete = false;

    //public int saved_confidence { get; private set; }
    public int saved_confidence { get; private set; }
    public int saved_bounty { get; private set; }
    public int saved_coin { get; private set; }
    public int saved_star { get; private set; }
    public int saved_Farming_item { get; private set; }
    private void Awake()
    {

        // Enforce single instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
        else
        {
            Instance = this; // Set the current instance
            DontDestroyOnLoad(gameObject); // Persist across scenes

            SceneManager.sceneLoaded += OnSceneLoaded;

        }

        LoadSaveFile();
    }

    private void LoadSaveFile()
    {
        //Load from save file
        //saved_confidence = PlayerPrefs.GetInt(AllStringConstant.CONFIDENCE, 5);
        saved_confidence = 5;
        saved_bounty = PlayerPrefs.GetInt(AllStringConstant.BOUNTY, 0);
        saved_coin = PlayerPrefs.GetInt(AllStringConstant.COIN, 0);
        saved_star = PlayerPrefs.GetInt(AllStringConstant.STAR, 0);
        saved_Farming_item = PlayerPrefs.GetInt(AllStringConstant.FARMING_ITEM, 1);
    }
    public void SaveTotalData(int confidence, int bounty, int coin)
    {
        //PlayerPrefs.SetInt(AllStringConstant.CONFIDENCE, confidence);
        PlayerPrefs.SetInt(AllStringConstant.BOUNTY, bounty);
        PlayerPrefs.SetInt(AllStringConstant.COIN, coin);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!loadFromLevelComplete)
            return;

        LoadSaveFile();

        if (scene.buildIndex == 1)
        {
            loadFromLevelComplete = false; // reset immediately

            var mainMenuManager = FindObjectOfType<MainMenuManager>();
            if (mainMenuManager != null)
            {
                mainMenuManager.OnPressPlay();
            }
        }
    }



    public void OnLevelComplete()
    {
        loadFromLevelComplete = true;
        LevelLoader.instance.loadLevelWithIndex(1);
    }
}
