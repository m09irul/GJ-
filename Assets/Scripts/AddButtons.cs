using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class AddButtons : MonoBehaviour
{
    [Header("Manipulate It")]
    [Tooltip("numbers of buttons that will be displayed on screen..")]
    [SerializeField] int buttonsNumber;
    [Space]
    [Space]

    [SerializeField] Transform puzzleField;
    [SerializeField] GameObject puzzleButton;
    public GameObject restartBtn;
    // Start is called before the first frame update
    void Awake()
    {
        CreateButtons();
        restartBtn.SetActive(false);
    }

    void CreateButtons()
    {
        for (int i = 0; i < buttonsNumber; i++)
        {
            GameObject btn = Instantiate(puzzleButton);
            btn.name = "" + i;
            btn.transform.SetParent(puzzleField, false);
        }
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
