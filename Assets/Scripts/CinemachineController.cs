using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class CinemachineController : MonoBehaviour
{
    [Header("Cinemachine Virtual Cameras")]
    public CinemachineVirtualCamera hubCam;
    public CinemachineVirtualCamera destinationCam;

    [Header("Dolly (optional)")]
    public CinemachineDollyCart dollyCart;         // optional, assign in Inspector
    public Transform hubDollyManualTarget;         // optional fallback target to pan to if no dolly
    public float dollyDuration = 4f;
    public AnimationCurve dollyCurve = AnimationCurve.EaseInOut(0,0,1,1);

    [Header("Durations")]
    public float hubHoldAfterDolly = 1.2f;
    public float destinationDuration = 3.0f;

    [Header("Letterbox Settings")]
    public RectTransform topBar;
    public RectTransform bottomBar;
    public float barSize = 180f;
    public float barSpeed = 2f;

    [Header("Fade Settings")]
    public Image fadeImage;
    public float fadeSpeed = 1.2f;

    [Header("Gameplay UI")]
    public GameObject gameplayUI;

    public bool autoStart = true;

    private float screenAspect;
    private float refAspect = 16f / 9f;

    void Start()
    {
        screenAspect = (float)Screen.width / Screen.height;

        // baseline
        if (hubCam != null) hubCam.Priority = 1;
        if (destinationCam != null) destinationCam.Priority = 1;

        if (fadeImage) { fadeImage.color = Color.black; fadeImage.gameObject.SetActive(true); }

        if (topBar) { topBar.gameObject.SetActive(false); topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, 0); }
        if (bottomBar) { bottomBar.gameObject.SetActive(false); bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, 0); }

        if (autoStart) StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // Disable gameplay UI during cinematic
        if (gameplayUI != null) gameplayUI.SetActive(false);

        // Fade from black to reveal hub
        yield return StartCoroutine(FadeIn());

        // Open letterbox bars
        if (topBar) topBar.gameObject.SetActive(true);
        if (bottomBar) bottomBar.gameObject.SetActive(true);
        yield return StartCoroutine(AnimateBars(true));

        // Activate hub camera (priority)
        if (hubCam != null) hubCam.Priority = 50;
        if (destinationCam != null) destinationCam.Priority = 1;

        // If dolly assigned and has path -> play dolly; else fallback to manual pan
        bool playedDolly = false;
        if (dollyCart != null && dollyCart.m_Path != null)
        {
            // reset dolly
            dollyCart.m_Position = 0f;
            float timer = 0f;
            float pathLen = 0f;
            // guard: some versions use m_Path (CinemachinePathBase)
            try { pathLen = dollyCart.m_Path.PathLength; } catch { pathLen = 1f; }

            while (timer < dollyDuration)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / dollyDuration);
                float eased = dollyCurve.Evaluate(t);
                // set position along path safely
                if (dollyCart.m_Path != null)
                    dollyCart.m_Position = eased * pathLen;
                else
                    dollyCart.m_Position = eased * dollyCart.m_Position; // harmless fallback

                yield return null;
            }

            // hold briefly at end of dolly
            yield return new WaitForSecondsRealtime(hubHoldAfterDolly);
            playedDolly = true;
        }
        else
        {
            // Fallback: simple smooth pan from current main camera to a hub focus point
            if (hubDollyManualTarget != null)
            {
                Camera mainCamTrans = Camera.main;
                if (mainCamTrans != null)
                {
                    Vector3 startPos = mainCamTrans.transform.position;
                    Quaternion startRot = mainCamTrans.transform.rotation;
                    Vector3 endPos = hubDollyManualTarget.position;
                    Quaternion endRot = Quaternion.LookRotation(hubDollyManualTarget.forward);

                    float t = 0f;
                    float panDur = dollyDuration;
                    while (t < panDur)
                    {
                        t += Time.unscaledDeltaTime;
                        float p = Mathf.SmoothStep(0, 1, t / panDur);
                        mainCamTrans.transform.position = Vector3.Lerp(startPos, endPos, p);
                        mainCamTrans.transform.rotation = Quaternion.Slerp(startRot, endRot, p);
                        yield return null;
                    }
                }
            }
            // hold a moment
            yield return new WaitForSecondsRealtime(hubHoldAfterDolly);
        }

        // Switch to destination cam
        if (hubCam != null) hubCam.Priority = 1;
        if (destinationCam != null) destinationCam.Priority = 50;

        // small black flash before revealing destination (nice impact)
        if (fadeImage) fadeImage.color = Color.black;
        yield return new WaitForSecondsRealtime(0.25f);
        yield return StartCoroutine(FadeIn());

        yield return new WaitForSecondsRealtime(destinationDuration);

        // Fade out then close bars
        yield return StartCoroutine(FadeOut());
        yield return StartCoroutine(AnimateBars(false));

        if (topBar) topBar.gameObject.SetActive(false);
        if (bottomBar) bottomBar.gameObject.SetActive(false);

        // Drop virtual camera priorities to let main gameplay camera take over
        if (hubCam != null) hubCam.Priority = 0;
        if (destinationCam != null) destinationCam.Priority = 0;

        // Re-enable UI
        if (gameplayUI != null) gameplayUI.SetActive(true);

        // Small fade in to gameplay state
        yield return StartCoroutine(FadeIn());

        // finally disable fade image so it doesn't block clicks
        if (fadeImage) fadeImage.gameObject.SetActive(false);
    }

    IEnumerator AnimateBars(bool opening)
    {
        if (topBar == null || bottomBar == null) yield break;

        float elapsed = 0f;
        float start = opening ? 0f : topBar.sizeDelta.y;
        float target = opening ? barSize * Mathf.Clamp((float)Screen.width / Screen.height / (refAspect), 0.8f, 1.25f) : 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.unscaledDeltaTime * barSpeed;
            float h = Mathf.Lerp(start, target, Mathf.SmoothStep(0, 1, elapsed));
            topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, h);
            bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, h);
            yield return null;
        }

        topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, target);
        bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, target);
    }

    IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;
        float a = 1f;
        while (a > 0f)
        {
            a -= Time.unscaledDeltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(a));
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0f);
    }

    IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;
        float a = 0f;
        while (a < 1f)
        {
            a += Time.unscaledDeltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(a));
            yield return null;
        }
    }

    // Optional target setter
    public void SetDestinationCamera(Transform target)
    {
        if (destinationCam != null)
        {
            destinationCam.Follow = target;
            destinationCam.LookAt = target;
        }
    }
}
