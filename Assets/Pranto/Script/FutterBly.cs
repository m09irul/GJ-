using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FutterBly : MonoBehaviour
{
    [Header("References")]
    public GameObject particleButterflyPrefab;
    public Transform playerTransform;

    [Header("Current Target")]
    [SerializeField] private Transform currentDestination;  // ← This is the LIVE target

    [Header("Swarm Settings")]
    public int burstSize = 3;
    public float burstInterval = 1.5f;
    public float flutterStrength = 0.4f;
    public Vector3 spawnOffset = new Vector3(0, 0.8f, 0);
    public float spawnRadius = 1.2f;

    private bool isGuiding = false;

    void Start()
    {
        playerTransform = GameObject.FindWithTag("cat").transform;
        StartGuidingTo(currentDestination);
    }
    // ═══════════════════════════════════════════════════════════
    // PUBLIC METHODS — CALL THESE FROM YOUR QUEST SYSTEM
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Start guiding to a new destination (can be called multiple times)
    /// </summary>
    public void StartGuidingTo(Transform newDestination)
    {
        currentDestination = newDestination;
        if (!isGuiding) StartGuiding(); 
    }

    /// <summary>
    /// Instantly switch to a completely different target mid-flight
    /// </summary>
    public void ChangeDestination(Transform newDestination)
    {
        currentDestination = newDestination;
        // All currently flying butterflies will re-calculate direction on next frame
    }

    /// <summary>
    /// Stop everything
    /// </summary>
    public void StopGuiding()
    {
        isGuiding = false;
        StopAllCoroutines();
    }

    // ═══════════════════════════════════════════════════════════

    private void StartGuiding()
    {
        if (isGuiding || currentDestination == null) return;
        isGuiding = true;
        StartCoroutine(SpawnBursts());
    }

    IEnumerator SpawnBursts()
    {
        while (isGuiding && currentDestination != null)
        {
            for (int i = 0; i < burstSize; i++)
            {
                SpawnOneButterfly();
                yield return new WaitForSeconds(0.15f);
            }
            yield return new WaitForSeconds(burstInterval);
        }
    }

    void SpawnOneButterfly()
    {
        Vector3 spawnPos = playerTransform.position + spawnOffset 
                         + Random.insideUnitSphere * spawnRadius;

        GameObject go = Instantiate(particleButterflyPrefab, spawnPos, Quaternion.identity);

        // THIS IS THE MAGIC: every single butterfly checks the LIVE currentDestination
        var ps = go.GetComponent<ParticleSystem>();
        var main = ps.main;
        var vel = ps.velocityOverLifetime;

        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;

        Vector3 dir = (currentDestination.position - spawnPos).normalized;

        // Forward thrust + gentle flutter
        vel.x = Random.Range(-flutterStrength, flutterStrength);
        vel.y = Random.Range(0.1f, 0.6f);                    // slight upward drift
        vel.z = new ParticleSystem.MinMaxCurve(dir.z * 2f + 1f, dir.z * 4f + 2f);

        // Rotate the whole emitter to face target (optional but prettier)
        go.transform.rotation = Quaternion.LookRotation(dir);

        // Auto-cleanup
        Destroy(go, 8f);
    }

    // Optional: visual gizmo in Scene view
    private void OnDrawGizmosSelected()
    {
        if (currentDestination != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(playerTransform.position, currentDestination.position);
            Gizmos.DrawWireSphere(currentDestination.position, 1f);
        }
    }
}