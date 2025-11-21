using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static bool gameIsPaused = false;

     public void Pause()
    {
        Time.timeScale = 0f;
        
        gameIsPaused = true;
    }

    public void resume()
    {
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    public void loadNextLevel()
    {
        LevelLoader.instance.loadNextLevel();
    }

    public void loadMainMenu()
    {
        //SceneManager.LoadScene();
        //SceneManager.LoadScene(1);
        LevelLoader.instance.loadLevelWithIndex(1);
        Time.timeScale = 1f;
    }
    public void Restart()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        LevelLoader.instance.loadLevelWithIndex(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }
}
