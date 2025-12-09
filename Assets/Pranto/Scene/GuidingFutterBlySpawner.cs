using UnityEngine;

public class GuidingFlutterBlySpawner : MonoBehaviour
{
    public GuidingFlutterBly butterflyPrefab;
    public Transform player;
    public Transform target;
    public float spawnInterval = 5f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnGuide();
            timer = 0f;
        }
    }

    void SpawnGuide()
    {
        var guide = Instantiate(butterflyPrefab);
        guide.player = player;
        guide.target = target;
    }
}
