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

        float input = Input.GetAxis("Vertical");
        input = Mathf.Max(0f, input); // forward only

        transform.Translate(0f, 0f, input * speed * Time.deltaTime);
    }

    public void StartVan()
    {
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
        canVanMove = false;

        var player = GameManager.Instance.player;

        player.transform.SetParent(null);
        player.transform.position = playerOutPos.position;
        player.enabled = true;

        if (blocker)
            blocker.SetActive(true);
    }
}
