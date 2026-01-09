using UnityEngine;

[RequireComponent(typeof(NPCNavAgentHandler))]
public class BatHolder : MonoBehaviour
{
    [Header("References")]
    public Transform holder;
    public Transform target;
    public GameObject[] bats;

    private NPCNavAgentHandler navHandler;

    [Header("Swarm Settings")]
    public float radius = 0.5f;
    public float flutterSpeed = 3f;
    public float followSmoothness = 4f;
    public float amplitude = 0.04f;

    private Transform[] batTr;
    private Vector3[] baseOffset;
    private float[] phase;
    private Vector3[] formationOffset;

    private float swarmBlend;
    private float blendVelocity;

    private const float FORM_Y = 0.5f;
    private const float FORM_SPACING = 0.25f;
    private const float MIN_Y_ADD = 0.5f;

    private Vector3 cachedForward;
    private Vector3 cachedRight;
    private Vector3 cachedCenter;

    private float sinT, cosT, sinHalfT, timeValue;

    private int batCount;
    private int halfCount;

    private Vector3 lastPos;

    // Holder height
    private float initialHolderYOffset;
    private float holderSmoothVel;
    public float holderYSmoothTime = 0.25f;

    // State
    private Vector3 initialHolderPosition;
    private bool chasing;
    private bool returning;

    private void Awake()
    {
        navHandler = GetComponent<NPCNavAgentHandler>();
        if (!navHandler)
        {
            Debug.LogError("NPCNavAgentHandler missing!");
            enabled = false;
        }
    }

    private void Start()
    {
        initialHolderPosition = holder.position;
        initialHolderYOffset = holder.position.y - transform.position.y;

        batCount = bats.Length;
        if (batCount == 0)
            return;

        halfCount = batCount / 2;

        batTr = new Transform[batCount];
        baseOffset = new Vector3[batCount];
        phase = new float[batCount];
        formationOffset = new Vector3[batCount];

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

    private void Update()
    {
        HandleMovementState();
        PrecomputeFrameValues();
        UpdateSwarm();
        UpdateHolderHeight();
    }

    /* =======================
     * MOVEMENT STATE
     * ======================= */

    private void HandleMovementState()
    {
        // CHASE
        if (!chasing && target != null)
        {
            chasing = true;
            returning = false;
            navHandler.MoveTo(target.position);
        }

        // RETURN
        if (chasing && navHandler.HasReachedDestination())
        {
            chasing = false;
            returning = true;
            navHandler.MoveTo(initialHolderPosition);
        }

        // FINISH RETURN
        if (returning && navHandler.HasReachedDestination())
        {
            returning = false;
        }
    }

    /* =======================
     * HOLDER HEIGHT
     * ======================= */

    private void UpdateHolderHeight()
    {
        float targetY = chasing
            ? transform.position.y
            : transform.position.y + initialHolderYOffset;

        Vector3 pos = holder.position;
        pos.y = Mathf.SmoothDamp(pos.y, targetY, ref holderSmoothVel, holderYSmoothTime);
        holder.position = pos;
    }

    /* =======================
     * SWARM MOTION
     * ======================= */

    private void PrecomputeFrameValues()
    {
        timeValue = Time.time * flutterSpeed;

        sinT = Mathf.Sin(timeValue);
        cosT = Mathf.Cos(timeValue);
        sinHalfT = Mathf.Sin(timeValue * 0.7f);

        cachedCenter = holder.position;

        Vector3 vel = (transform.position - lastPos) / Mathf.Max(Time.deltaTime, 0.001f);
        cachedForward = vel.sqrMagnitude > 0.001f ? vel.normalized : holder.forward;
        cachedRight = holder.right;
    }

    private void LateUpdate()
    {
        lastPos = transform.position;
    }

    private void UpdateSwarm()
    {
        bool moving = chasing || returning;

        float targetBlend = moving ? 0f : 1f;
        swarmBlend = Mathf.SmoothDamp(swarmBlend, targetBlend, ref blendVelocity, 0.35f);

        for (int i = 0; i < batCount; i++)
        {
            Transform t = batTr[i];
            float ph = phase[i];

            Vector3 swarmPos = cachedCenter + baseOffset[i];
            swarmPos.x += Mathf.Sin(timeValue + ph) * amplitude;
            swarmPos.y += Mathf.Abs(Mathf.Cos(timeValue * 1.4f + ph)) * amplitude;
            swarmPos.z += Mathf.Sin(timeValue * 0.7f + ph) * amplitude;

            Vector3 formPos = cachedCenter
                + cachedRight * formationOffset[i].x
                + cachedForward * formationOffset[i].z;

            formPos.y += FORM_Y + Mathf.Abs(cosT) * amplitude * 0.1f;

            Vector3 targetPos = Vector3.Lerp(formPos, swarmPos, swarmBlend);
            targetPos.y = Mathf.Max(targetPos.y, cachedCenter.y + MIN_Y_ADD);

            t.position = Vector3.Lerp(t.position, targetPos, Time.deltaTime * followSmoothness);

            Vector3 dir = targetPos - t.position;
            if (dir.sqrMagnitude > 0.0001f)
                t.forward = Vector3.Lerp(t.forward, dir.normalized, Time.deltaTime * 4f);
        }
    }
}
