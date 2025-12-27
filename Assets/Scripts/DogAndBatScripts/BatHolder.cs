using UnityEngine;

public class BatHolder : MonoBehaviour
{
    public Transform holder;
    public Transform target;
    public GameObject[] bats;

    private NPCNavAgentHandler navHandler;

    [Header("Swarm Settings")]
    public float radius = 0.5f;
    public float flutterSpeed = 3f;
    public float followSmoothness = 4f;
    public float amplitude = 0.04f;

    [SerializeField] private Transform[] batTr;
    private Vector3[] baseOffset;
    private float[] phase;
    private Vector3[] formationOffset;

    private float swarmBlend = 0f;
    private float blendVelocity = 0f;
    private float stoppingDist;

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

    // NEW: Holder Y follow settings
    private float initialHolderYOffset;
    private float holderSmoothVel = 0f;
    public float holderYSmoothTime = 0.25f;

    // State tracking
    private Vector3 initialHolderPosition;
    private bool wasEventTriggered = false;
    private bool returningToStart = false;

    void Start()
    {
        navHandler = GetComponent<NPCNavAgentHandler>();
        if (!navHandler)
        {
            Debug.LogError("NPCNavAgentHandler missing!");
            enabled = false;
        }

        // Cache initial holder position and height offset from main object
        initialHolderPosition = holder.position;
        initialHolderYOffset = holder.position.y - transform.position.y;

        batCount = bats.Length;
        if (batCount == 0)
            return;

        stoppingDist = navHandler.getStopDistance() + 0.1f;
        halfCount = batCount >> 1;

        batTr = new Transform[batCount];
        baseOffset = new Vector3[batCount];
        phase = new float[batCount];
        formationOffset = new Vector3[batCount];

        for (int i = 0; i < batCount; i++)
        {
            batTr[i] = bats[i].transform;
            Debug.Log("Bats Enterring");
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


    void Update()
    {
        HandleMovementTriggers();
        PrecomputeFrameValues();
        UpdateSwarmFast();
        UpdateHolderHeight();
    }


    private void HandleMovementTriggers()
    {
        // Trigger ON — chase
        if (navHandler.isEventTriggered && !wasEventTriggered)
        {
            wasEventTriggered = true;
            returningToStart = false;

            if (target != null)
                navHandler.MoveNext(target.position);
        }

        // Trigger OFF — return home
        if (!navHandler.isEventTriggered && wasEventTriggered)
        {
            wasEventTriggered = false;
            returningToStart = true;

            navHandler.MoveNext(initialHolderPosition);
        }

        // Stop returning once at home
        if (returningToStart &&
            navHandler.getRemainingDistance() <= stoppingDist &&
            !navHandler.GetpathPending())
        {
            returningToStart = false;
        }
    }


    private void UpdateHolderHeight()
    {
        float targetY;

        if (navHandler.isEventTriggered)
        {
            // While chasing: holder matches parent's Y height smoothly
            targetY = transform.position.y;
        }
        else
        {
            // Restore original height offset from parent
            targetY = transform.position.y + initialHolderYOffset;
        }

        Vector3 pos = holder.position;
        pos.y = Mathf.SmoothDamp(pos.y, targetY, ref holderSmoothVel, holderYSmoothTime);
        holder.position = pos;        
    }


    private void PrecomputeFrameValues()
    {
        timeValue = Time.time * flutterSpeed;

        sinT = Mathf.Sin(timeValue);
        cosT = Mathf.Cos(timeValue);
        sinHalfT = Mathf.Sin(timeValue * 0.7f);

        cachedCenter = holder.position;

        Vector3 vel = (transform.position - lastPos) / Time.deltaTime;
        cachedForward = vel.sqrMagnitude > 0.001f ? vel.normalized : holder.forward;
        cachedRight = holder.right;
    }

    void LateUpdate()
    {
        lastPos = transform.position;
    }


    private void UpdateSwarmFast()
    {
        bool moving = navHandler.getRemainingDistance() > stoppingDist;

        float targetBlend = moving ? 0f : 1f;
        swarmBlend = Mathf.SmoothDamp(swarmBlend, targetBlend, ref blendVelocity, 0.35f);

        for (int i = 0; i < batCount; i++)
        {
            Transform t = bats[i].transform;
            float ph = phase[i];

            Vector3 targetPos;

            if (moving)
            {
                targetPos = cachedCenter + baseOffset[i];

                targetPos.x += Mathf.Sin(timeValue + ph) * amplitude;
                targetPos.y += Mathf.Abs(Mathf.Cos(timeValue * 1.4f + ph)) * amplitude;
                targetPos.z += Mathf.Sin(timeValue * 0.7f + ph) * amplitude;
            }
            else
            {
                Vector3 form = cachedCenter
                    + cachedRight * formationOffset[i].x
                    + cachedForward * formationOffset[i].z;

                form.x += sinT * amplitude * 0.15f;
                form.y += FORM_Y + Mathf.Abs(cosT) * amplitude * 0.1f;
                form.z += sinHalfT * amplitude * 0.1f;

                Vector3 sw = cachedCenter + baseOffset[i];
                sw.x += Mathf.Sin(timeValue + ph) * amplitude;
                sw.y += Mathf.Abs(Mathf.Cos(timeValue * 1.4f + ph)) * amplitude;
                sw.z += Mathf.Sin(timeValue * 0.7f + ph) * amplitude;

                if (sw.y < cachedCenter.y + MIN_Y_ADD)
                    sw.y = cachedCenter.y + MIN_Y_ADD;

                targetPos = form + (sw - form) * swarmBlend;
            }

            targetPos.y = Mathf.Max(targetPos.y, cachedCenter.y + MIN_Y_ADD);

            t.position = Vector3.Lerp(t.position, targetPos, Time.deltaTime * followSmoothness);

            Vector3 dir = targetPos - t.position;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Vector3 newForward = cachedForward + (dir.normalized - cachedForward) * swarmBlend;
                t.forward = Vector3.Lerp(t.forward, newForward, Time.deltaTime * 4f);
            }
        }
    }
}
