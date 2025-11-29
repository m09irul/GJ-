using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance { get; private set; }

    public Animator my_animator;
    public float transitionTime = 1f;

    private void Awake()
    {
        instance = this;
    }

    public void loadNextLevel()
    {
        StartCoroutine(Load_Level(SceneManager.GetActiveScene().buildIndex + 1));
    }
    public void loadLevelWithIndex(int index)
    {
        StartCoroutine(Load_Level(index));
    }

    IEnumerator Load_Level(int sceneIndex)
    {
        
        my_animator.SetTrigger("fade");    

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(sceneIndex);
    }
}
