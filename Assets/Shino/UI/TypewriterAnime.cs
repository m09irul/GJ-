using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterAnime : MonoBehaviour
{
    public float fadeDuration = 1f;
    private TMP_Text tmp;

    void Awake()
    {
        tmp = GetComponent<TMP_Text>();
    }

    public void ShowText(string message)
    {
        Debug.Log("ShowText called with message: " + message);

        tmp.ForceMeshUpdate();        // IMPORTANT
        tmp.alpha = 1f;               // force show
        tmp.text = message;

        Debug.Log("Text after setting: " + tmp.text + " | alpha = " + tmp.alpha);
    }


    public void HideText()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            tmp.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            tmp.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
    }
}
