using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class GuidingFlutterBlySpawner : MonoBehaviour
{
    public GuidingFutterBly butterflyPrefab;
    public Transform player;
    public Transform target;
    public float spawnInterval = 5f; // delay after previous is destroyed
    public float behindDistance = 1.5f;

    private bool spawning = false;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Spawn a butterfly
            GuidingFutterBly guide = Instantiate(butterflyPrefab, player.position - player.forward * behindDistance, quaternion.identity);

            guide.player = player;
            guide.target = target;

            // Wait until the butterfly is destroyed
            while (guide != null)
            {
                yield return null;
            }

            // Wait for spawnInterval seconds before spawning next
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
