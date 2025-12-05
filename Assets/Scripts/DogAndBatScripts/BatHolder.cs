using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BatHolder : MonoBehaviour
{
    public Transform holder;
    public NavMeshAgent agent;          // Leader (parent object)
    public Transform target;            // Where the swarm moves
    public GameObject[] bats;           // All 15 bats

    [Header("Swarm Settings")]
    public float radius = .3f;           // Spread distance of bats
    public float flutterSpeed = 3f;     // How fast bats shake
    public float followSmoothness = 4f; // How smooth bats follow center
    public float amplitude = 0.1f;      // Flutter amount

    private Vector3[] offsets;

    void Start()
    {
        if (bats.Length == 0) return;

        offsets = new Vector3[bats.Length];

        // Assign random starting offsets so bats don’t overlap
        for (int i = 0; i < bats.Length; i++)
        {
            offsets[i] = new Vector3(
                Random.Range(-radius, radius),
                Random.Range(0.5f, radius), // above
                Random.Range(-radius, radius)
            );
        }
    }

    void Update()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
        }

        Vector3 swarmCenter = holder.position;

        // Move each bat relative to the leader
        for (int i = 0; i < bats.Length; i++)
        {
            if (bats[i] == null) continue;

            // Make the offset flutter like chaotic flying
            Vector3 flutter =
                new Vector3(
                    Mathf.Sin(Time.time * flutterSpeed + i) * amplitude,
                    Mathf.Cos(Time.time * flutterSpeed * 1.4f + i) * amplitude,
                    Mathf.Sin(Time.time * flutterSpeed * 0.7f + i) * amplitude
                );

            // Target position for this bat
            Vector3 desiredPos = swarmCenter + offsets[i] + flutter;
            desiredPos.y = Mathf.Max(swarmCenter.y + 1f, desiredPos.y);

            // Smooth movement
            bats[i].transform.position = Vector3.Lerp(
                bats[i].transform.position,
                desiredPos,
                Time.deltaTime * followSmoothness
            );

            // Make each bat face the target direction
            Vector3 dir = (desiredPos - bats[i].transform.position).normalized;
            if (dir != Vector3.zero)
                bats[i].transform.forward = Vector3.Lerp(
                    bats[i].transform.forward,
                    dir,
                    Time.deltaTime * 5f
                );
        }
    }
}
