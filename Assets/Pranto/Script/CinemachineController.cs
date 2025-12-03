using System.Collections;
using UnityEngine;
using Cinemachine;

public class CinemachineController : MonoBehaviour
{
    public CinemachineVirtualCamera hubCam;
    public CinemachineVirtualCamera destinationCam;

    public float hubDuration = 3f;
    public float destinationDuration = 3f;

    public bool autoStart = true;

    void Start()
    {
        // Ensure baseline
        hubCam.Priority = 1;
        destinationCam.Priority = 1;

        if (autoStart)
            StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // FORCE HUB TO BE ACTIVE
        hubCam.Priority = 50;
        destinationCam.Priority = 1;

        yield return new WaitForSeconds(hubDuration);

        // FORCE DESTINATION TO BE ACTIVE
        hubCam.Priority = 1;
        destinationCam.Priority = 50;

        yield return new WaitForSeconds(destinationDuration);

        // RETURN TO MAIN CAMERA (since both VCams drop priority)
        hubCam.Priority = 0;
        destinationCam.Priority = 0;
    }

    // Optional dynamic target
    public void SetDestinationCamera(Transform target)
    {
        destinationCam.Follow = target;
        destinationCam.LookAt = target;
    }
}
