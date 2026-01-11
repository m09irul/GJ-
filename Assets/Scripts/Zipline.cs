using UnityEngine;
using DG.Tweening;

public class Zipline : MonoBehaviour
{
    [Header("Zipline Points")]
    [SerializeField] Transform startPoint;
    [SerializeField] Transform endPoint;

    [Header("Player Positions")]
    [SerializeField] Transform playerInPoint;   // where player snaps when zip starts
    [SerializeField] Transform playerOutPoint;  // where player lands at the end

    [Header("Movement")]
    [SerializeField] float rideDuration = 2.5f;

    Tween zipTween;
    PlayerController player;
    public GameObject bucket;
    bool riding;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("cat") || GameManager.Instance.isPlayerDetected || riding) return;

        player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.StartHide(playerInPoint, playerOutPoint, StartZip);

        }
    }
    void StartZip()
    {
        riding = true;
        AudioManager.instance.play("zipline");
        UIManager.Instance.hudPanel.SetActive(false);
        // Parent player to bucket
        player.transform.SetParent(bucket.transform);

        zipTween?.Kill();

        zipTween = bucket.transform
            .DOMove(endPoint.position, rideDuration)
            .OnComplete(FinishZip);
    }

    void FinishZip()
    {
        zipTween = null;
        player.transform.SetParent(null);
        player.ExitHide();
        UIManager.Instance.hudPanel.SetActive(true);
    }

    void OnDisable()
    {
        zipTween?.Kill();
    }
}
