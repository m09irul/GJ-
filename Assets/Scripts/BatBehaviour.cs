using UnityEngine;

public class BatBehavior : MonoBehaviour
{
    public float flySpeed = 12f;
    public float flyAwayDistance = 10f;
    public int damage = 20;

    private Transform player;
    private bool hasHit = false;
    private Vector3 flyAwayDirection;

    public System.Action OnBatHitPlayer;   // callback to spawner

    float amplitude = 0.03f;   // How high it moves
    float speed = 10f;         // How fast it moves
    float startY;
    float multi = 1f;

    private void Start()
    {
        if (Random.Range(0.0f, 1.0f) > 0.5)
        {
            multi = -1f;
        }
        startY = transform.position.y;
        speed = 10f;
        amplitude = 0.03f;
        player = GameObject.FindGameObjectWithTag("cat").transform;
    }

    private void Update()
    {
        FloatVertical();
        
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
            other.GetComponent<PlayerController>().ReduceConfidence(1);

            hasHit = true;

            // Calculate fly-away direction (forward + up)
            flyAwayDirection = (player.forward + Vector3.up).normalized;

            // Notify the spawner
            OnBatHitPlayer?.Invoke();
        }
    }



    void FloatVertical()
    {
        
        float newY = startY + Mathf.Sin(Time.time * speed) * amplitude * multi;
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            newY,
            transform.localPosition.z
        );
    }
}
