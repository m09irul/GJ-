using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    [Header("Manipulate It")]
    [Tooltip("the puzzles that will be shown in game from the sprite path.. ")]
    [SerializeField] List<int> indexOfPuzzles = new List<int>();
    [Tooltip("the buttons which will be mystery button in game and user have to match them by hearing.. ")]
    [SerializeField] List<int> mysteryIndexOfButtons = new List<int>();
    [Space]
    [Space]


    [SerializeField] TextMeshProUGUI moveText;
    [SerializeField] Animator game_animator;
    [SerializeField] Color32 correctColor;
    [SerializeField] Color32 wrongColor;
    [SerializeField] Color32 selectedColor;
    [SerializeField] Color32 normalColor;
    [SerializeField] Sprite coverImage;
    [SerializeField] Sprite mysteryCoverImage;


    public Sprite[] puzzels;
    
    public List<Sprite> gamePuzzles = new List<Sprite>();

    public List<Button> btns = new List<Button>();

    bool firstGuess, secondGuess;

    int countGuesses, countCorrectGuesses, gameGuesses, firstGuessIndex, secondGuessIndex;

    string firstGuesspuzzle, secondGuessPuzzle;

    public GameObject backButton;
    /// <summary>
    /// load all resources..
    /// </summary>
    private void Awake()
    {
        puzzels = Resources.LoadAll<Sprite>(AllStringConstant.GAME_RESOURCES_PATH);
    }


    void Start()
    {
        //PlayerPrefs.DeleteAll();

        GetButtons();
        AddGamePuzzles();
        Shuffle(gamePuzzles);

        gameGuesses = gamePuzzles.Count / 2;

        ShowImages();

    }

    public void StartGame()
    {
        game_animator.Play(AllStringConstant.GAME_START_ANIM);
        HideButtons();
        AddListener();
    }

    /// <summary>
    /// wrap images after displaying them at start.. 
    /// </summary>
    private void HideButtons()
    {
        for (int i = 0; i < btns.Count; i++)
        {
            btns[i].image.sprite = coverImage;
            btns[i].image.color = normalColor;
            btns[i].interactable = true;
        }
    }

    /// <summary>
    /// shows image for a amount of seconds at the start.. 
    /// </summary>
    private void ShowImages()
    {
      
        for (int i = 0; i < btns.Count; i++)
        {
            if (mysteryIndexOfButtons.Count != 0)
            {
                foreach (int number in mysteryIndexOfButtons)
                {
                    if (i == number)
                    {
                        btns[i].image.sprite = mysteryCoverImage;
                        break;
                    }
                    else
                        btns[i].image.sprite = gamePuzzles[i];
                }
            }
            else
                btns[i].image.sprite = gamePuzzles[i];

            btns[i].image.color = selectedColor;
            btns[i].interactable = false;


        }
    }

    /// <summary>
    /// adds same buttons twice..
    /// </summary>
    private void AddGamePuzzles()
    {
        int looper = btns.Count;

        if(indexOfPuzzles.Count > looper/2f)
        {
            Debug.LogError("index of puzzles list size must be half of buttons size..");
        }

        int IndexOfIndexPuzzleList = 0;
        int puzzleIndex = 0;

        for (int i = 0; i < looper; i++)
        {
            puzzleIndex = indexOfPuzzles[IndexOfIndexPuzzleList];

            gamePuzzles.Add(puzzels[puzzleIndex]);

            //resolve index out of bound..
            if (IndexOfIndexPuzzleList == indexOfPuzzles.Count - 1)
                IndexOfIndexPuzzleList = -1;

            IndexOfIndexPuzzleList++;
        }
    }

    private void AddListener()
    {
        foreach (Button btn in btns)
        {
            btn.onClick.AddListener(() => PickAPuzzle());
        }
    }


    /// <summary>
    /// Get available active buttons from scene..
    /// </summary>
    private void GetButtons()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(AllStringConstant.PUZZLE_BUTTON_TAG);

        for (int i = 0; i < objects.Length; i++)
        {
            btns.Add(objects[i].GetComponent<Button>());
            btns[i].image.sprite = coverImage;
        } 
    }

    /// <summary>
    /// actions when image is selected..
    /// </summary>
    private void PickAPuzzle()
    {
        if (!firstGuess)
        {
            
            firstGuess = true;

            //get button index..
            firstGuessIndex = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name);

            btns[firstGuessIndex].interactable = false;
            btns[firstGuessIndex].gameObject.GetComponent<Image>().color = selectedColor;
            
            //get the name to play audio..
            firstGuesspuzzle = gamePuzzles[firstGuessIndex].name;

            StartCoroutine(TurnPuzzleButtonUp(btns[firstGuessIndex].GetComponent<Animator>(),
                                              btns[firstGuessIndex], gamePuzzles[firstGuessIndex], firstGuessIndex));



            AudioManager.instance.play(gamePuzzles[firstGuessIndex].name);

            
        }
        else if (!secondGuess)
        {
            
            secondGuess = true;

            secondGuessIndex = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name);

            btns[secondGuessIndex].interactable = false;
            btns[secondGuessIndex].gameObject.GetComponent<Image>().color = selectedColor;


            //get the name to play audio..
            secondGuessPuzzle = gamePuzzles[secondGuessIndex].name;

            StartCoroutine(TurnPuzzleButtonUp(btns[secondGuessIndex].GetComponent<Animator>(),
                                             btns[secondGuessIndex], gamePuzzles[secondGuessIndex], secondGuessIndex));

            

            AudioManager.instance.play(gamePuzzles[secondGuessIndex].name);

            countGuesses++;

            StartCoroutine(CheckIfPuzzleMatches());

        }

        
    }

    /// <summary>
    /// used for animation the image when selected..
    /// </summary>
    /// <param name="anim"></param>
    /// <param name="btn"></param>
    /// <param name="puzzleImage"></param>
    /// <returns></returns>
    IEnumerator TurnPuzzleButtonUp(Animator anim, Button btn, Sprite puzzleImage, int buttonIndex)
    {
        Sprite applyableImage = puzzleImage;
        foreach (int number in mysteryIndexOfButtons)
        {
            if(buttonIndex == number)
            {
                applyableImage = mysteryCoverImage;
                break;
            }
        }
        anim.Play(AllStringConstant.GAMEBUTTON_REVEAL_ANIM);

        yield return new WaitForSeconds(.5f);

        btn.image.sprite = applyableImage;

        //GameTutorial part implementation.. 
        if (GameTutorialManager.instance != null && GameTutorialManager.instance.isTutorialModeOn)
        {
            GameTutorialManager gameTutorialManager = GameTutorialManager.instance;

            yield return new WaitForSeconds(1);

            if(gameTutorialManager.currentDialogueIndex == 5 || gameTutorialManager.currentDialogueIndex == 9)
                gameTutorialManager.TriggerUpdate();
        }
    }

    IEnumerator TurnPuzzleButtonBack(Animator anim, Button btn, Sprite puzzleImage)
    {
        anim.Play(AllStringConstant.GAMEBUTTON_HIDE_ANIM);
        yield return new WaitForSeconds(.5f);
        btn.image.sprite = puzzleImage;

        btns[firstGuessIndex].interactable = true;
        btns[secondGuessIndex].interactable = true;

        btns[firstGuessIndex].image.color = normalColor;
        btns[secondGuessIndex].image.color = normalColor;

        //GameTutorial part implementation.. 
        if (GameTutorialManager.instance != null && GameTutorialManager.instance.isTutorialModeOn)
        {
            GameTutorialManager gameTutorialManager = GameTutorialManager.instance;

            yield return new WaitForSeconds(0.2f);
            gameTutorialManager.UpdateDialogueIndexFromAnotherScript();
        }
    }

    IEnumerator CheckIfPuzzleMatches()
    {
        yield return new WaitForSeconds(1f);

        if(firstGuesspuzzle == secondGuessPuzzle)
        {
            btns[firstGuessIndex].image.color = correctColor;
            btns[secondGuessIndex].image.color = correctColor;

            AudioManager.instance.play("correct");

            yield return new WaitForSeconds(1f);

            btns[firstGuessIndex].interactable = false;
            btns[secondGuessIndex].interactable = false;

            btns[firstGuessIndex].GetComponent<Animator>().Play(AllStringConstant.GAMEBUTTON_FADEOUT_ANIM);
            btns[secondGuessIndex].GetComponent<Animator>().Play(AllStringConstant.GAMEBUTTON_FADEOUT_ANIM);

            moveText.text = AllStringConstant.MOVE_TEXT + countGuesses.ToString();

            CheckIfGameFinished();

            //GameTutorial part implementation.. 
            if (GameTutorialManager.instance != null && GameTutorialManager.instance.isTutorialModeOn)
            {
                GameTutorialManager gameTutorialManager = GameTutorialManager.instance;

                gameTutorialManager.currentDialogueIndex++;

                yield return new WaitForSeconds(0.7f);
                gameTutorialManager.UpdateDialogueIndexFromAnotherScript();
            }
        }
        else
        {
            btns[firstGuessIndex].gameObject.GetComponent<Image>().color = wrongColor;
            btns[secondGuessIndex].gameObject.GetComponent<Image>().color = wrongColor;

            AudioManager.instance.play("wrong");

            btns[firstGuessIndex].GetComponent<Animator>().Play(AllStringConstant.SHAKE);
            btns[secondGuessIndex].GetComponent<Animator>().Play(AllStringConstant.SHAKE);

            yield return new WaitForSeconds(1f);

            StartCoroutine(TurnPuzzleButtonBack(btns[firstGuessIndex].GetComponent<Animator>(),
                                                btns[firstGuessIndex], coverImage));

            StartCoroutine(TurnPuzzleButtonBack(btns[secondGuessIndex].GetComponent<Animator>(),
                                                btns[secondGuessIndex], coverImage));

            

        }

        

        yield return new WaitForSeconds(.7f);

        firstGuess = secondGuess = false;
    }

    private void CheckIfGameFinished()
    {
        countCorrectGuesses++;

        if(countCorrectGuesses == gameGuesses)
        {
            int starGained = CheckHowManyGuesses();

            StartCoroutine(OnGameOver(starGained));

            int previousPoints = PlayerPrefs.GetInt(AllStringConstant.TOTAL_POINTS, 0);
            PlayerPrefs.SetInt(AllStringConstant.TOTAL_POINTS, starGained + previousPoints);

            //save the data of already played level 1 once..
            PlayerPrefs.SetInt(AllStringConstant.HAS_PLAYED_LEVEL1_ONCE, 1);

        }

    }

    IEnumerator OnGameOver(int numberOfStars)
    {
        backButton = GameObject.FindGameObjectWithTag("backUI");

        AudioManager.instance.play(AllStringConstant.GAME_OVER_SFX);

        //hide back button
        backButton.SetActive(false);

        game_animator.Play(AllStringConstant.GAMEOVER_ANIM);

        yield return new WaitForSeconds(0.1f);

        game_animator.Play(AllStringConstant.STAR_1_ANIM, 1);
        numberOfStars--;

        if(numberOfStars > 0 )
        {
            yield return new WaitForSeconds(0.33f);

            game_animator.Play(AllStringConstant.STAR_2_ANIM, 2);
            numberOfStars--;

            if (numberOfStars > 0)
            {
                yield return new WaitForSeconds(0.33f);

                game_animator.Play(AllStringConstant.STAR_3_ANIM, 3);
                
            }
        }
        
        //AdManager.instance.ShowVideoAdAsLevelCompleted();
    }

    int CheckHowManyGuesses()
    {
        ManageGridAndLevelLockSystem(SceneManager.GetActiveScene().buildIndex);

        if (countGuesses == gameGuesses)
        {
            PlayerPrefs.SetInt(AllStringConstant.LEVEL + (SceneManager.GetActiveScene().buildIndex).ToString(), 3);
            //Debug.Log("got 3 star.. ");
            return 3;

        }
        else if (countGuesses >= gameGuesses / 2f)
        {
            PlayerPrefs.SetInt(AllStringConstant.LEVEL + (SceneManager.GetActiveScene().buildIndex).ToString(), 2);
            //Debug.Log("got 2 star.. ");
            return 2;

        }
        else
        {
            PlayerPrefs.SetInt(AllStringConstant.LEVEL + (SceneManager.GetActiveScene().buildIndex).ToString(), 1);
            //Debug.Log("got 1 star.. ");
            return 1;

        }

        
    }

    private void ManageGridAndLevelLockSystem(int sceneIndex)
    {
        if(sceneIndex <= 10)
        {
            PlayerPrefs.SetInt(AllStringConstant.UNLOCKED_GRID1_Level_BUTTON, sceneIndex - 0 - 1);
        }
        else if (sceneIndex > 10 && sceneIndex <= 20)
        {
            //unlock 2nd grid..
            PlayerPrefs.SetInt(AllStringConstant.UNLOCKED_GRID_BUTTON, 1);

            PlayerPrefs.SetInt(AllStringConstant.UNLOCKED_GRID2_Level_BUTTON, sceneIndex - 10 - 1);
        }
        else if (sceneIndex > 20 && sceneIndex <= 30)
        {
            //unlock 3nd grid..
            PlayerPrefs.SetInt(AllStringConstant.UNLOCKED_GRID_BUTTON, 2);

            PlayerPrefs.SetInt(AllStringConstant.UNLOCKED_GRID3_Level_BUTTON, sceneIndex - 20 - 1);
        }
        else if (sceneIndex > 30 && sceneIndex <= 40)
        {
            //unlock 4th grid..
            PlayerPrefs.SetInt(AllStringConstant.UNLOCKED_GRID_BUTTON, 3);

            PlayerPrefs.SetInt(AllStringConstant.UNLOCKED_GRID4_Level_BUTTON, sceneIndex - 30 - 1);
        }
    }

    /// <summary>
    /// shuffle the images..
    /// </summary>
    /// <param name="list"></param>
    void Shuffle(List<Sprite> list)
    {
        //shufle for first half..
        for (int i = 0; i < list.Count/2; i++)
        {
            Sprite temp = list[i];
            int randomIndex = UnityEngine.Random.Range(i, list.Count/2);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }

        //shufle for second half..
        for (int i = list.Count / 2 ; i < list.Count; i++)
        {
            Sprite temp = list[i];
            int randomIndex = UnityEngine.Random.Range(list.Count / 2, list.Count/2);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
