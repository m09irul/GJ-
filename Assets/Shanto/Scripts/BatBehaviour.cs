using UnityEngine;

public class BatBehavior : MonoBehaviour
{
    public float flySpeed = 12f;
    public float flyAwayDistance = 20f;
    public int damage = 20;

    private Transform player;
    private bool hasHit = false;
    private Vector3 flyAwayDirection;

    public System.Action OnBatHitPlayer;   // callback to spawner

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("cat").transform;
    }

    private void Update()
    {
        if (!hasHit)
        {
            // Dive toward player
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                flySpeed * Time.deltaTime
            );
        }
        else
        {
            // Fly away
            transform.position += flyAwayDirection * flySpeed * Time.deltaTime;

            // Destroy after flying far enough
            if (Vector3.Distance(transform.position, player.position) > flyAwayDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("cat"))
        {
            Debug.Log("Bat hit the Cat!");

            // Damage here if needed
            other.GetComponent<PlayerController>().ReduceConfidence(damage);

            hasHit = true;

            // Calculate fly-away direction (forward + up)
            flyAwayDirection = (player.forward + Vector3.up).normalized;

            // Notify the spawner
            OnBatHitPlayer?.Invoke();
        }
    }
}
