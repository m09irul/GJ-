using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterAnime : MonoBehaviour
{
    public float fadeDuration = 1f;   // How long fade takes
    private TMP_Text tmp;

    void Awake()
    {
        tmp = GetComponent<TMP_Text>();
    }

    public void ShowText(string message)
    {
        StopAllCoroutines();
        tmp.text = message;
        StartCoroutine(FadeIn());
    }

    public void HideText()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        tmp.alpha = 0f;
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
        tmp.alpha = 1f;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            tmp.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
    }
}
