using UnityEngine;
using System.Collections; 
public class PanelAnimator : MonoBehaviour
{
    public CanvasGroup group;
    public float fadeTime = 0.5f;

    public void FadeIn() {
        StopAllCoroutines();
        StartCoroutine(Fade(0, 1));
    }

    IEnumerator Fade(float from, float to) {
        float t = 0;
        while (t < fadeTime) {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
    }
}
