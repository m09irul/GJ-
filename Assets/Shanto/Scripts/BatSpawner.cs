using UnityEngine;

public class BatSpawner : MonoBehaviour
{
    public GameObject batPrefab;
    public Transform player;

    public float spawnHeight = 5f;
    public float nextSpawnDelay = 3f;      // delay after hit before new bat

    private int totalSpawns = 3;
    private int spawnCount = 0;


    private void SpawnBat()
    {
        if (spawnCount >= totalSpawns) return;

        Vector3 spawnPos = player.position + Vector3.up * spawnHeight;
        GameObject bat = Instantiate(batPrefab, spawnPos, Quaternion.identity);

        // Subscribe to event
        bat.GetComponent<BatBehavior>().OnBatHitPlayer += OnBatHit;

        spawnCount++;
    }

    private void OnBatHit()
    {
        // Spawn next bat AFTER delay
        Invoke(nameof(SpawnBat), nextSpawnDelay);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("cat"))
        {
            SpawnBat();
        }
    }
}
