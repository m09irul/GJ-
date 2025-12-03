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

    [SerializeField] GameObject[] allDialogues;
    [HideInInspector] public int currentDialogueIndex = -1;
    [HideInInspector] public bool isTutorialModeOn = false;
    int previousDialogueIndex = 0;

    [SerializeField] GameObject playOptionPanel;

    [SerializeField] GameObject nextButton;

    string[] tutorial_voices = { AllStringConstant.TUT_0, AllStringConstant.TUT_1, AllStringConstant.TUT_2, AllStringConstant.TUT_3,
                                 AllStringConstant.TUT_4, AllStringConstant.TUT_5, AllStringConstant.TUT_6, AllStringConstant.TUT_7,
                                 AllStringConstant.TUT_8, AllStringConstant.TUT_9, AllStringConstant.TUT_10,AllStringConstant.TUT_11,
                                 AllStringConstant.TUT_12 }; //13 tutorial texts.. 
    IEnumerator Start()
    {
        foreach (var item in allDialogues)
        {
            item.SetActive(false);
        }
        nextButton.SetActive(false);

        yield return new WaitForSeconds(2);

        if(PlayerPrefs.GetInt(AllStringConstant.HAS_PLAYED_LEVEL1_ONCE, 0) == 1)
        {
            Time.timeScale = 0;
            playOptionPanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            playOptionPanel.SetActive(false);

            PlayWithTutorial();
        }
    }

    public void PlayWithTutorial()
    {
        isTutorialModeOn = true;

        Time.timeScale = 1;
        playOptionPanel.SetActive(false);

        TriggerUpdate();


    }

    public void PlayByYourself()
    {
        isTutorialModeOn = false;

        Time.timeScale = 1;

        gameObject.SetActive(false);

    }

    public void TriggerUpdate()
    {
        StopAllCoroutines();
        StartCoroutine(UpdateDialogue());
    }
    IEnumerator UpdateDialogue()
    {
        nextButton.SetActive(true);

        Time.timeScale = 0;

        currentDialogueIndex++;

        if (currentDialogueIndex > 12)
        {
            PlayByYourself();

        }

        else if(currentDialogueIndex == 3)
        {
            nextButton.SetActive(false);

            Time.timeScale = 1;

            allDialogues[previousDialogueIndex].SetActive(false);

            yield return new WaitForSecondsRealtime(3f);

            nextButton.SetActive(true);

            allDialogues[currentDialogueIndex].SetActive(true);
            //play voice over
          //  AudioManager.instance.stop(tutorial_voices[currentDialogueIndex - 1]);
          //  AudioManager.instance.play(tutorial_voices[currentDialogueIndex]);
        }
        else if (currentDialogueIndex == 5 || currentDialogueIndex == 9)
        {
            nextButton.SetActive(false);

            Time.timeScale = 1;

            allDialogues[previousDialogueIndex].SetActive(false);
            allDialogues[currentDialogueIndex].SetActive(true);
            //play voice over
            //AudioManager.instance.stop(tutorial_voices[currentDialogueIndex - 1]);
           // AudioManager.instance.play(tutorial_voices[currentDialogueIndex]);
        }
        else if (currentDialogueIndex == 7 || currentDialogueIndex == 11)
        {
            nextButton.SetActive(false);

            Time.timeScale = 1;

            allDialogues[previousDialogueIndex].SetActive(false);

            //now interactions will be based on gameplay.. 
            
        }
        else if (currentDialogueIndex == 8 || currentDialogueIndex == 12)
        {
            currentDialogueIndex -= 4;
            TriggerUpdate();

        }
        else
        {
            allDialogues[previousDialogueIndex].SetActive(false);
            allDialogues[currentDialogueIndex].SetActive(true);
            //play voice over
          //  if(currentDialogueIndex != 0)
            //    AudioManager.instance.stop(tutorial_voices[currentDialogueIndex - 1]);
          //  AudioManager.instance.play(tutorial_voices[currentDialogueIndex]);
        }


        previousDialogueIndex = currentDialogueIndex;
    }

    public void UpdateDialogueIndexFromAnotherScript()
    {
        previousDialogueIndex = currentDialogueIndex;
        allDialogues[currentDialogueIndex].SetActive(true);
        //play voice over
       // AudioManager.instance.stop(tutorial_voices[currentDialogueIndex-1]);
      //  AudioManager.instance.play(tutorial_voices[currentDialogueIndex]);
        nextButton.SetActive(true);
    }


}
