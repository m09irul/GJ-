using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject optionMenuPanel;
    public GameObject chapterSelectionPanel;
    public GameObject levelSelectionPanel;
    public GameObject confirmLevelPanel;
    public GameObject helpMenuPanel;
    public GameObject creditMenuPanel;
    public Animator menuAnimator;
    void Start()
    {
        menuAnimator = GetComponent<Animator>();
    }

    public void OnPressPlay()
    {
        mainMenuPanel.SetActive(false);
        chapterSelectionPanel.SetActive(true);
    }
    public void OnPressOption()
    {
        mainMenuPanel.SetActive(false);
        PlayMenuPanelAnimation("Option menu open");
    }
    public void OnPressHelp()
    {
        mainMenuPanel.SetActive(false);
        PlayMenuPanelAnimation("Help menu open");
    }
    public void OnPressCredit()
    {
        mainMenuPanel.SetActive(false);
        PlayMenuPanelAnimation("Credit menu open");
    }
    public void PlayMenuPanelAnimation(string name)
    {
        menuAnimator.Play(name, 0);
    }
    public void ExitButton()
    {
        Application.Quit();
    }
}
