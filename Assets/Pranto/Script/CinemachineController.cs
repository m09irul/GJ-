using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class CinemachineController : MonoBehaviour
{
    [Header("Cinemachine Virtual Cameras")]
    public CinemachineVirtualCamera hubCam;
    public CinemachineVirtualCamera destinationCam;

    [Header("Durations")]
    public float hubDuration = 3f;
    public float destinationDuration = 3f;

    [Header("Letterbox Settings")]
    public RectTransform topBar;
    public RectTransform bottomBar;
    public float barSize = 200f;  // Height in pixels
    public float barSpeed = 2f;

    [Header("Fade Settings")]
    public Image fadeImage;
    public float fadeSpeed = 1.5f;

    [Header("Gameplay UI")]
    public GameObject gameplayUI;

    public bool autoStart = true;

    private float screenAspect;
    private float refAspect = 16f / 9f;

    void Start()
    {
        screenAspect = (float)Screen.width / Screen.height;

        hubCam.Priority = 1;
        destinationCam.Priority = 1;

        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 1);  // Start fully black

        // Hide bars initially
        if (topBar) topBar.gameObject.SetActive(false);
        if (bottomBar) bottomBar.gameObject.SetActive(false);

        if (autoStart)
            StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        if (gameplayUI != null) gameplayUI.SetActive(false);

        yield return StartCoroutine(FadeIn());

        // Show and open bars
        if (topBar) topBar.gameObject.SetActive(true);
        if (bottomBar) bottomBar.gameObject.SetActive(true);
        yield return StartCoroutine(AnimateBars(true));

        // Shots...
        hubCam.Priority = 50;
        destinationCam.Priority = 1;
        yield return new WaitForSeconds(hubDuration);

        hubCam.Priority = 1;
        destinationCam.Priority = 50;
        yield return new WaitForSeconds(destinationDuration);

        yield return StartCoroutine(FadeOut());

        yield return StartCoroutine(AnimateBars(false));

        // Hide bars after closing
        if (topBar) topBar.gameObject.SetActive(false);
        if (bottomBar) bottomBar.gameObject.SetActive(false);

        hubCam.Priority = 0;
        destinationCam.Priority = 0;

        if (gameplayUI != null) gameplayUI.SetActive(true);

        yield return StartCoroutine(FadeIn());
    }

    IEnumerator AnimateBars(bool opening)
    {
        float targetHeight = opening ? barSize : 0f;
        float aspectScale = Mathf.Clamp(screenAspect / refAspect, 0.8f, 1.5f);  // FIXED: screen / ref for proper scaling
        targetHeight *= aspectScale;

        // Start from current height
        float startHeight = topBar.sizeDelta.y;

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * barSpeed;
            float t = Mathf.SmoothStep(0f, 1f, elapsed);
            float height = Mathf.Lerp(startHeight, targetHeight, t);

            topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, height);  // Keep width, change height
            bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, height);

            yield return null;
        }

        // Snap to exact
        topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, targetHeight);
        bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, targetHeight);

        Debug.Log($"Bars animated! Final height: {targetHeight}");
    }

    IEnumerator FadeIn()
    {
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * fadeSpeed;
            if (fadeImage) fadeImage.color = new Color(0, 0, 0, t);
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            if (fadeImage) fadeImage.color = new Color(0, 0, 0, t);
            yield return null;
        }
    }

    public void SetDestinationCamera(Transform target)
    {
        destinationCam.Follow = target;
        destinationCam.LookAt = target;
    }
}