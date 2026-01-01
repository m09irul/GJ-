using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public class LeverInteractable : MonoBehaviour
{
    [SerializeField] private MovingPlatform platform;
    [SerializeField] private GameObject handle, bottom;
    bool canMove = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("cat"))
        {
            canMove = true;
            ToggleOutline(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("cat"))
        {
            canMove = false;
            ToggleOutline(false);
        }
    }
    void ToggleOutline(bool stat)
    {
        handle.GetComponent<Outline>().enabled = stat;
        bottom.GetComponent<Outline>().enabled = stat;
    }
    public void OnMouseDown()
    {
        if (canMove)
            platform.Activate(handle);

    }
}