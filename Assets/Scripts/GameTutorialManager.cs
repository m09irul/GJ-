using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTutorialManager : MonoBehaviour
{
    public static GameTutorialManager instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }



}
