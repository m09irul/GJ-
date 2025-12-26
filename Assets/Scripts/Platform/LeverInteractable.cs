using UnityEngine;
using UnityEngine.UIElements;

public class LeverInteractable : MonoBehaviour
{
    [SerializeField] private MovingPlatform platform;
    [SerializeField] private GameObject outline;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            outline.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            outline.SetActive(false);
    }

    public void OnMouseDown()
    {
        platform.Activate();
    }
}