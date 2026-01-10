using UnityEngine;

public class HidePlayer : MonoBehaviour
{
    [SerializeField] private Transform hideAnchor;
    [SerializeField] private Transform exitAnchor;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("cat") || GameManager.Instance.isPlayerDetected) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
            player.StartHide(hideAnchor, exitAnchor);
    }
}
