using UnityEngine;

public class VanController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject blocker;

    [Header("Player Positions")]
    [SerializeField] private Transform playerInPos;
    [SerializeField] private Transform playerOutPos;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;

    private bool canVanMove;

    private void Update()
    {
        HandleVanMovement();
    }

    private void HandleVanMovement()
    {
        if (!canVanMove)
            return;

        float input = GameManager.Instance.player.movementStick.Vertical;
        input = Mathf.Max(0f, input); // forward only

        transform.Translate(0f, 0f, input * speed * Time.deltaTime);

        if (input > 0)
        {
            if (!AudioManager.instance.GetAudio("car moving").source.isPlaying)
                AudioManager.instance.play("car moving");
        }
        else
            AudioManager.instance.stop("car moving");
    }

    public void StartVan()
    {
        AudioManager.instance.play("car start");
        canVanMove = true;

        var player = GameManager.Instance.player;

        player.transform.position = playerInPos.position;
        player.enabled = false;
        player.transform.SetParent(transform);

        if (blocker)
            blocker.SetActive(false);
    }

    public void StopVan()
    {
        AudioManager.instance.stop("car moving");
        AudioManager.instance.play("car engine off");
        canVanMove = false;

        var player = GameManager.Instance.player;

        player.transform.SetParent(null);
        player.transform.position = playerOutPos.position;
        player.enabled = true;

        if (blocker)
            blocker.SetActive(true);
    }
}
