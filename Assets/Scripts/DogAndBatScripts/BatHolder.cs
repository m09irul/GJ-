using UnityEngine;
using UnityEngine.AI;

public class BatHolder : MonoBehaviour
{
    public Transform holder;
    public NavMeshAgent agent;
    public Transform target;
    public GameObject[] bats;

    [Header("Swarm Settings")]
    public float radius = 0.1f;
    public float flutterSpeed = 3f;
    public float followSmoothness = 4f;
    public float amplitude = 0.04f;

    private Transform[] batTransforms;
    private Vector3[] swarmOffsets;
    private float[] phaseOffsets;

    // Transition blending
    private float swarmBlend = 0f;
    private float blendVelocity = 0f;

    private float stoppingDist;

    void Start()
    {
        int count = bats.Length;
        if (count == 0) return;

        stoppingDist = agent.stoppingDistance + 0.1f;

        batTransforms = new Transform[count];
        swarmOffsets = new Vector3[count];
        phaseOffsets = new float[count];

        for (int i = 0; i < count; i++)
        {
            batTransforms[i] = bats[i].transform;

            swarmOffsets[i] = new Vector3(
                Random.Range(-radius, radius),
                Random.Range(0.3f, radius),
                Random.Range(-radius, radius)
            );

            phaseOffsets[i] = Random.Range(0f, Mathf.PI * 2);
        }
    }

    void Update()
    {
        if (target != null)
            agent.SetDestination(target.position);

        bool moving = agent.remainingDistance > stoppingDist;

        // Smoothly blend between swarm (1) and formation (0)
        float targetBlend = moving ? 0f : 1f;
        swarmBlend = Mathf.SmoothDamp(swarmBlend, targetBlend, ref blendVelocity, 0.4f);

        UpdateBats(moving);
    }

    // ---------------------------------------------------------
    // Blended system — Formation <-> Swarm
    // ---------------------------------------------------------
    private void UpdateBats(bool moving)
    {
        int count = batTransforms.Length;
        Vector3 center = holder.position;

        Vector3 forward = agent.velocity.sqrMagnitude > 0.001f
            ? agent.velocity.normalized
            : holder.forward;

        Vector3 right = holder.right;

        float spacing = 0.25f;
        float time = Time.time * flutterSpeed;

        for (int i = 0; i < count; i++)
        {
            Transform bat = batTransforms[i];
            float phase = phaseOffsets[i];

            // -----------------------------
            // 1) Formation Target Position
            // -----------------------------
            int half = count / 2;
            int idx = i - half;

            Vector3 formation =
                center
                + forward * (Mathf.Abs(idx) * -spacing)
                + right * (idx * spacing)
                + Vector3.up * 0.5f;

            formation += new Vector3(
                Mathf.Sin(time + phase) * amplitude * 0.2f,
                Mathf.Abs(Mathf.Cos(time + phase)) * amplitude * 0.2f,
                Mathf.Sin(time * 0.5f + phase) * amplitude * 0.1f
            );

            // -----------------------------
            // 2) Swarm Target Position
            // -----------------------------
            Vector3 swarmPos =
                center +
                swarmOffsets[i] +
                new Vector3(
                    Mathf.Sin(time + phase) * amplitude,
                    Mathf.Abs(Mathf.Cos(time * 1.4f + phase)) * amplitude,
                    Mathf.Sin(time * 0.7f + phase) * amplitude
                );

            swarmPos.y = Mathf.Max(center.y + 0.5f, swarmPos.y);

            // -----------------------------
            // 3) Final Blended Position
            // -----------------------------
            Vector3 desired = Vector3.Lerp(formation, swarmPos, swarmBlend);

            bat.position = Vector3.Lerp(
                bat.position,
                desired,
                Time.deltaTime * followSmoothness
            );

            // -----------------------------
            // Smooth Rotation Logic
            // -----------------------------
            Vector3 dir = desired - bat.position;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Vector3 targetForward = Vector3.Lerp(forward, dir.normalized, swarmBlend);
                bat.forward = Vector3.Lerp(
                    bat.forward,
                    targetForward,
                    Time.deltaTime * 4f
                );
            }
        }
    }
}
