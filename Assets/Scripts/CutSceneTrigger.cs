using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CutSceneTrigger : MonoBehaviour
{
    public enum CamTriggerType
    {
        dog,
        bat,
        police

    }
    public CamTriggerType camTriggerType;
    Collider triggerCollider;

    void Start()
    {
        triggerCollider = GetComponent<Collider>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("cat"))
        {
            triggerCollider.enabled = false;
            
            if (camTriggerType == CamTriggerType.dog)
            {
                CinemachineController.Instance.PlayCamera(AllStringConstant.DOG_CAMERA, Ease.Linear, () =>
                {
                    DialogueManager.instance.StartDialogue(AllStringConstant.DOG_CONFIDENCE_DIALOUGE_NODE_ID, () =>
                    {
                         CinemachineController.Instance.StopCamera();
                    });
                });

            }

            else if (camTriggerType == CamTriggerType.bat)
            {
                CinemachineController.Instance.PlayCamera(AllStringConstant.BAT_CAMERA, Ease.Linear, () =>
                {
                    DialogueManager.instance.StartDialogue(AllStringConstant.BAT_DIALOUGE_NODE_ID, () =>
                    {
                         CinemachineController.Instance.StopCamera();
                    });
                });

            }

            else if (camTriggerType == CamTriggerType.police)
            {
                CinemachineController.Instance.PlayCamera(AllStringConstant.POLICE_CAMERA, Ease.Linear, () =>
                {
                    DialogueManager.instance.StartDialogue(AllStringConstant.POLICE_DIALOUGE_NODE_ID, () =>
                    {
                         CinemachineController.Instance.StopCamera();
                    });
                });

            }

        }
    }
}
