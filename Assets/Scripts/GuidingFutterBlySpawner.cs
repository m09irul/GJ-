using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;

public class GuidingFlutterBlySpawner : MonoBehaviour
{
    public GuidingFutterBly butterflyPrefab;
    public float spawnInterval = 15f; // delay after previous is destroyed
    public float behindDistance = 1.5f;
    Transform player;

    void Start()
    {
        player = GameManager.Instance.player.transform;
    }
    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Spawn a butterfly
            GuidingFutterBly guide = Instantiate(butterflyPrefab, player.position -player.forward * behindDistance, quaternion.identity);

            guide.player = player;
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
