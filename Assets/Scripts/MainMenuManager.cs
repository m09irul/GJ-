using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{

    Animator menuAnimator;
    UIManager uIManager;

    void Start()
    {
        AudioManager.instance.play("MainMenuBG");


        menuAnimator = GetComponent<Animator>();
        uIManager = UIManager.Instance;

        uIManager.totalCoins.SetValue(SessionManager.Instance.saved_coin);
        uIManager.totalStars.SetValue(SessionManager.Instance.saved_star);

    }

    public void OnPressPlay()
    {
        uIManager.mainMenuPanel.SetActive(false);
        uIManager.chapterSelectionPanel.SetActive(true);
    }
    public void OnPressOption()
    {
        uIManager.mainMenuPanel.SetActive(false);
        PlayMenuPanelAnimation("Option menu open");
    }
    public void OnPressHelp()
    {
        uIManager.mainMenuPanel.SetActive(false);
        PlayMenuPanelAnimation("Help menu open");
    }
    public void OnPressCredit()
    {
        uIManager.mainMenuPanel.SetActive(false);
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
