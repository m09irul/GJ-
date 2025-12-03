using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance;
    public int saved_confidence{get; private set;}
    public int saved_bounty{get; private set;}
    public int saved_coin{get; private set;}
    public int saved_star{get; private set;}
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
        }

        //Load from save file
        saved_confidence = PlayerPrefs.GetInt(AllStringConstant.CONFIDENCE, 5);
        saved_bounty = PlayerPrefs.GetInt(AllStringConstant.BOUNTY, 0);
        saved_coin = PlayerPrefs.GetInt(AllStringConstant.COIN, 0);
        saved_star = PlayerPrefs.GetInt(AllStringConstant.STAR, 0);
    }


    // Update is called once per frame
    void Update()
    {

    }
}
