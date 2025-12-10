using System;
using System.Collections;
using UnityEngine;
using Cinemachine;

public class CutsceneCamera : MonoBehaviour
{
    public string cameraName;

    [Header("Camera Type")]
    public bool hasDolly = false;
    public CinemachineVirtualCamera virtualCam;
    public CinemachineDollyCart dolly;

    [Header("Dialogue Trigger")]
    public bool triggerDialogue;
    public int dialogueTriggerPoint = 2; 
    public int dollyTriggerWaypoint = 2;

    private Action onComplete;

    public void ActivateCamera()
    {
        virtualCam.Priority = 20;
    }

    public void DeactivateCamera()
    {
        virtualCam.Priority = 0;
    }

    public void StartCutscene(Action onFinished)
    {
        onComplete = onFinished;
        StartCoroutine(CutsceneRoutine());
    }

    private IEnumerator CutsceneRoutine()
    {
        if (hasDolly)
        {
            dolly.m_Speed = 2f; 
            dolly.m_Position = 0;

            while (dolly.m_Position < 10)
            {
                if (triggerDialogue && Mathf.FloorToInt(dolly.m_Position) == dollyTriggerWaypoint)
                {
                    yield return StartCoroutine(TriggerDialogueAndWait());
                }

                yield return null;
            }
        }
        else
        {
            if (triggerDialogue)
                yield return StartCoroutine(TriggerDialogueAndWait());
        }

        onComplete?.Invoke();
    }

    private IEnumerator TriggerDialogueAndWait()
    {
        // DialogueManager.Instance.StartDialogue(" DogTutorial ");
        // while (DialogueManager.Instance.IsDialogueRunning)
            yield return null;
    }
}