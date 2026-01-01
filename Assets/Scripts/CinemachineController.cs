using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cinemachine;

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

    [Header("Priority Settings")]
    [SerializeField] private int activePriority = 15;
    [SerializeField] private int inactivePriority = 10;
    private CutsceneCamera currentCinematicCam;
    private bool inCinematic = false;
    private CinemachineVirtualCamera currentCamera;

    public CinemachineBrain brain;
    void Awake()
    {
        brain = Camera.main.GetComponent<CinemachineBrain>();
        Instance = this;
    }

    public void SetBlendTime(float time)
    {
        brain.m_DefaultBlend.m_Time = time;
    }
    public float GetBlendTime()
    {
        return brain.m_DefaultBlend.BlendTime;
    }
    public void ResetBlendTime()
    {
        brain.m_DefaultBlend.m_Time = 1.5f;
    }

    public void SetCamera(CinemachineVirtualCamera newCamera)
    {
        if (newCamera == null) return;
        if (currentCamera == newCamera) return;

        // Lower previous camera
        if (currentCamera != null)
            currentCamera.Priority = inactivePriority;

        // Raise new camera
        currentCamera = newCamera;
        currentCamera.Priority = activePriority;
    }

    public CinemachineVirtualCamera GetCurrentCamera()
    {
        return currentCamera;
    }
    // =====================================================
    // MAIN API — CUTSCENES CALL THESE
    // =====================================================

    public void PlayCamera(string camName, Ease ease, Action callback)
    {
        var tmpCam = Array.Find(cameras, c => c.cameraName == camName);

        if (!tmpCam)
        {
            Debug.LogError("Camera not found: " + camName);
            return;
        }

        StartCoroutine(PlayCameraFlow(tmpCam, ease, callback));
    }

    // =====================================================
    // CAMERA PLAY FLOW
    // =====================================================

    private IEnumerator PlayCameraFlow(CutsceneCamera tmpCam, Ease ease, Action callback)
    {
        // FIRST CAMERA IN CUTSCENE
        if (!inCinematic)
        {
            GameManager.Instance.OnSceneStart();
            SetBlendTime(4f);

            inCinematic = true;
            yield return ShowBars();

        }
        yield return FadeOut();

        // ACTIVATE CAMERA
        if (currentCinematicCam != null)
            currentCinematicCam.DeactivateCamera();

        currentCinematicCam = tmpCam;
        currentCinematicCam.ActivateCamera();
        yield return FadeIn();

        // wait if there is a blend
        yield return WaitForBlend();

        // RUN CAMERA LOGIC
        bool finished = false;
        currentCinematicCam.StartCutscene(ease, () => finished = true);

        // Wait until camera finished
        yield return new WaitUntil(() => finished);
        callback?.Invoke();
    }

    // =====================================================
    // END CUTSCENE FLOW
    // =====================================================
    public void StopCamera(Action callback = null)
    {
        StartCoroutine(EndCinematicFlow(callback));
    }
    IEnumerator EndCinematicFlow(Action callback)
    {
        yield return FadeOut();
        yield return HideBars();

        if (currentCinematicCam != null)
            currentCinematicCam.DeactivateCamera();

        currentCinematicCam = null;
        inCinematic = false;

        yield return FadeIn();

        // wait if there is a blend
        yield return WaitForBlend();



        GameManager.Instance.OnSceneComplete();

        ResetBlendTime();

        callback?.Invoke();
    }
    public IEnumerator WaitForBlend()
    {
        // Wait until blending starts (if any)
        while (brain.IsBlending)
            yield return null;

        // Blend is finished
        yield break;
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
        yield return null;
    }
}