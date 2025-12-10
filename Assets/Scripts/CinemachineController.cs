using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CinemachineController : MonoBehaviour
{
    public static CinemachineController Instance;

    [Header("Fade")]
    public Image fadePanel;
    public float fadeDuration = 0.5f;

    [Header("Bars")]
    public RectTransform topBar;
    public RectTransform bottomBar;
    public float barHeight = 55f;
    public float barDuration = 0.45f;
    public Ease barEase = Ease.InOutQuad;

    [Header("Cameras")]
    public CutsceneCamera[] cameras;

    private CutsceneCamera currentCam;
    private bool inCinematic = false;

    void Awake() => Instance = this;

    // =====================================================
    // MAIN API — CUTSCENES CALL THESE
    // =====================================================

    public void PlayCamera(string camName, Action callback)
    {
        var tmpCam = Array.Find(cameras, c => c.cameraName == camName);

        if (!tmpCam)
        {
            Debug.LogError("Camera not found: " + camName);
            return;
        }

        StartCoroutine(PlayCameraFlow(tmpCam, callback));
    }

    // =====================================================
    // CAMERA PLAY FLOW
    // =====================================================

    private IEnumerator PlayCameraFlow(CutsceneCamera tmpCam, Action callback)
    {
        // FIRST CAMERA IN CUTSCENE
        if (!inCinematic)
        {
            inCinematic = true;

            yield return FadeOut();
            yield return ShowBars();
            yield return FadeIn();
        }
        else
        {
            // CHAINED CAMERA (no bars, optional soft fade)
            yield return FadeOut();
        }

        // ACTIVATE CAMERA
        if (currentCam != null)
            currentCam.DeactivateCamera();
        
        currentCam = tmpCam;
        currentCam.ActivateCamera();
        yield return FadeIn();

        // RUN CAMERA LOGIC
        bool finished = false;
        currentCam.StartCutscene(() => finished = true);

        // Wait until camera finished
        yield return new WaitUntil(() => finished);

        callback?.Invoke();
    }

    // =====================================================
    // END CUTSCENE FLOW
    // =====================================================

    public IEnumerator EndCinematicFlow()
    {
        yield return FadeOut();
        yield return HideBars();

        if (currentCam != null)
            currentCam.DeactivateCamera();

        currentCam = null;
        inCinematic = false;

        yield return FadeIn();

        GameManager.Instance.OnSceneComplete();
    }

    // =====================================================
    // UI TWEENS
    // =====================================================

    public IEnumerator FadeOut()
    {
        yield return fadePanel.DOFade(1f, fadeDuration)
                              .SetEase(Ease.OutQuad)
                              .WaitForCompletion();
    }

    public IEnumerator FadeIn()
    {
        yield return fadePanel.DOFade(0f, fadeDuration)
                              .SetEase(Ease.InQuad)
                              .WaitForCompletion();
    }

    public IEnumerator ShowBars()
    {
        Sequence s = DOTween.Sequence();
        s.Join(topBar.DOSizeDelta(new Vector2(topBar.sizeDelta.x, barHeight), barDuration).SetEase(barEase));
        s.Join(bottomBar.DOSizeDelta(new Vector2(bottomBar.sizeDelta.x, barHeight), barDuration).SetEase(barEase));
        yield return s.WaitForCompletion();
    }

    public IEnumerator HideBars()
    {
        Sequence s = DOTween.Sequence();
        s.Join(topBar.DOSizeDelta(new Vector2(topBar.sizeDelta.x, 0), barDuration).SetEase(barEase));
        s.Join(bottomBar.DOSizeDelta(new Vector2(bottomBar.sizeDelta.x, 0), barDuration).SetEase(barEase));
        yield return s.WaitForCompletion();
    }
}