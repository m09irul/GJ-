using System;
using System.Collections;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CutsceneCamera : MonoBehaviour
{
    public string cameraName;

    [Header("Camera Type")]
    public bool hasDolly;
    public CinemachineVirtualCamera virtualCam;
    public CinemachineDollyCart dolly;

    [Header("Waypoint Trigger")]
    public bool triggerDialogue;
    public int dollyTriggerPoint = 2;

    private Action onComplete;
    private bool triggered = false;

    public void ActivateCamera() => virtualCam.Priority = 20;
    public void DeactivateCamera() => virtualCam.Priority = 0;

    public void StartCutscene(Action done)
    {
        onComplete = done;
        StartCoroutine(RunCamera());
    }

    private IEnumerator RunCamera()
    {
        triggered = false;

        if (hasDolly)
        {
            dolly.m_Position = 0;
            float end = dolly.m_Path.PathLength;

            // Smooth start and end
            float duration = 4f;
            float easePos = 0;
            DOTween.To(() => easePos, v => easePos = v, end, duration)
                   .SetEase(Ease.InOutCubic);

            while (easePos < end)
            {
                dolly.m_Position = easePos;


                yield return null;
            }
        }

        // NORMAL CAMERA (no dolly)
        else
        {
            
                yield return new WaitForSeconds(4f);
        }

        onComplete?.Invoke();
    }

    private IEnumerator TriggerDialoguePause()
    {
        // your dialogue logic here
        yield return new WaitForSeconds(1f);
    }
}
