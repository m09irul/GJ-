using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class GuidingFlutterBlySpawner : MonoBehaviour
{
    public GuidingFutterBly butterflyPrefab;
    public float spawnInterval = 15f; // delay after previous is destroyed
    public float behindDistance = 1.5f;

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Spawn a butterfly
            GuidingFutterBly guide = Instantiate(butterflyPrefab, GameManager.Instance.player.position - GameManager.Instance.player.forward * behindDistance, quaternion.identity);

            guide.player = GameManager.Instance.player;
            guide.target = GameManager.Instance.target;

            // Wait until the butterfly is destroyed
            while (guide != null)
            {
                yield return null;
            }

            // Wait for spawnInterval seconds before spawning next
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    public void Spawn()
    {
        StartCoroutine(SpawnLoop());
    }

}
