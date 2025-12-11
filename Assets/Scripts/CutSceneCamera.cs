using System;
using System.Collections;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CutsceneCamera : MonoBehaviour
{
    public string cameraName;

    [Header("Dolly")]
    public bool hasDolly;
    public CinemachineVirtualCamera virtualCam;
    public CinemachineDollyCart dolly;
    public float dollyDuration = 3f;
    public float delayBeforeFinish = 0.5f;

    private Action onComplete;

    public void ActivateCamera() => virtualCam.Priority = 20;
    public void DeactivateCamera() => virtualCam.Priority = 0;

    public void StartCutscene(Action done)
    {
        onComplete = done;
        StartCoroutine(RunCamera());
    }

    private IEnumerator RunCamera()
    {
        if (hasDolly)
        {
            float end = dolly.m_Path.PathLength;
            dolly.m_Position = 0;

            // Tween WITHOUT polling
            Tween t = DOTween.To(
                () => dolly.m_Position,
                v => dolly.m_Position = v,
                end,
                dollyDuration
            )
            .SetEase(Ease.InOutCirc);

            yield return t.WaitForCompletion();
        }

        if (delayBeforeFinish > 0)
            yield return new WaitForSeconds(delayBeforeFinish);

        onComplete?.Invoke();
    }
}
