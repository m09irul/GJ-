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

    private Transform[] batTr;
    private Vector3[] baseOffset;
    private float[] phase;
    private Vector3[] formationOffset;

    private float swarmBlend = 0f;
    private float blendVelocity = 0f;

    private float stoppingDist;

    // Cache static constants
    private const float FORM_Y = 0.5f;
    private const float FORM_SPACING = 0.25f;
    private const float MIN_Y_ADD = 0.5f;

    // Cached reusable vectors
    private Vector3 cachedForward;
    private Vector3 cachedRight;
    private Vector3 cachedCenter;

    // Precomputed per-frame values
    private float sinT, cosT, sinHalfT, timeValue;

    private int batCount;
    private int halfCount;

    void Start()
    {
        batCount = bats.Length;
        if (batCount == 0)
            return;

        stoppingDist = agent.stoppingDistance + 0.1f;

        halfCount = batCount >> 1;

        batTr = new Transform[batCount];
        baseOffset = new Vector3[batCount];
        phase = new float[batCount];
        formationOffset = new Vector3[batCount];

        // Bake everything ONCE
        for (int i = 0; i < batCount; i++)
        {
            batTr[i] = bats[i].transform;

            baseOffset[i] = new Vector3(
                Random.Range(-radius, radius),
                Random.Range(0.3f, radius),
                Random.Range(-radius, radius)
            );

            phase[i] = Random.Range(0f, Mathf.PI * 2f);
        }

        // Precompute formation offsets
        for (int i = 0; i < batCount; i++)
        {
            int idx = i - halfCount;

            formationOffset[i] = new Vector3(
                idx * FORM_SPACING,
                FORM_Y,
                -Mathf.Abs(idx) * FORM_SPACING
            );
        }
    }

    void Update()
    {
        if (target != null)
            agent.SetDestination(target.position);

        bool moving = agent.remainingDistance > stoppingDist;

        float targetBlend = moving ? 0f : 1f;
        swarmBlend = Mathf.SmoothDamp(swarmBlend, targetBlend, ref blendVelocity, 0.35f);

        PrecomputeFrameValues();
        UpdateSwarmFast();
    }

    private void PrecomputeFrameValues()
    {
        timeValue = Time.time * flutterSpeed;

        sinT = Mathf.Sin(timeValue);
        cosT = Mathf.Cos(timeValue);
        sinHalfT = Mathf.Sin(timeValue * 0.7f);

        cachedCenter = holder.position;

        Vector3 vel = agent.velocity;
        cachedForward = vel.sqrMagnitude > 0.001f ? vel.normalized : holder.forward;
        cachedRight = holder.right;
    }

    private void UpdateSwarmFast()
    {
        for (int i = 0; i < batCount; i++)
        {
            Transform t = batTr[i];

            float ph = phase[i];

            // --- FORMATION POSITION (NO MATH inside loop) ---
            Vector3 form = cachedCenter
                + cachedRight * formationOffset[i].x
                + cachedForward * formationOffset[i].z;

            // tiny flutter for formation
            form.x += sinT * amplitude * 0.15f;
            form.y += FORM_Y + Mathf.Abs(cosT) * amplitude * 0.1f;
            form.z += sinHalfT * amplitude * 0.1f;

            // --- SWARM POSITION ---
            Vector3 sw = cachedCenter + baseOffset[i];

            sw.x += Mathf.Sin(timeValue + ph) * amplitude;
            sw.y += Mathf.Abs(Mathf.Cos(timeValue * 1.4f + ph)) * amplitude;
            sw.z += Mathf.Sin(timeValue * 0.7f + ph) * amplitude;

            // always above center
            if (sw.y < cachedCenter.y + MIN_Y_ADD)
                sw.y = cachedCenter.y + MIN_Y_ADD;

            // Blend formation <-> swarm
            Vector3 targetPos =
                form + (sw - form) * swarmBlend;

            // Smooth move
            t.position = Vector3.Lerp(t.position, targetPos, Time.deltaTime * followSmoothness);

            // Forward direction
            Vector3 dir = targetPos - t.position;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Vector3 newForward =
                    cachedForward + (dir.normalized - cachedForward) * swarmBlend;

                t.forward = Vector3.Lerp(t.forward, newForward, Time.deltaTime * 4f);
            }
        }
    }
}
