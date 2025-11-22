using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public PanelAnimator panelAnim;
    public TypewriterAnime typewriter;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(BeginStartingTutorials());
    }

    private IEnumerator BeginStartingTutorials()
    {
        yield return new WaitForSeconds(1f);
        ShowTutorial("Use WASD to move.");

        yield return new WaitForSeconds(4f);
        ShowTutorial("Use Mouse to look around.");

        yield return new WaitForSeconds(4f);
        ShowTutorial("Press Space to jump.");

        yield return new WaitForSeconds(4f);
        ShowTutorial("Explore the area to discover objectives.");
    }

    public void ShowTutorial(string message)
    {
        Debug.Log("ShowTutorial called with: " + message + " | typewriter = " + typewriter);
        panelAnim.FadeIn();
        typewriter.ShowText(message);
    }
}
