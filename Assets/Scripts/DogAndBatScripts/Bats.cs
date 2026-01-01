using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bats : MonoBehaviour
{
    public Transform holder;
    public Transform target;
    public GameObject[] bats;


    [SerializeField] float attackRange = 10f;

    [Header("Swarm Settings")]
    public float radius = 0.5f;
    public float flutterSpeed = 3f;
    public float followSmoothness = 4f;
    public float amplitude = 0.5f;

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

    [SerializeField] private bool inAttackPosition;
    [SerializeField] private bool isMoving;

    private bool isAttacking = false;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] public Transform player;
    void Start()
    {
        // Cache initial local positions of bats
        batStartPositions = new Vector3[bats.Length];
        for (int i = 0; i < bats.Length; i++)
        {
            if (bats[i] != null)
                batStartPositions[i] = bats[i].transform.localPosition;
        }

        inAttackPosition = true;
        holder = gameObject.transform;

        // Cache initial holder position and height offset from main object
        initialHolderPosition = holder.position;
        initialHolderYOffset = holder.position.y - transform.position.y;

        batCount = bats.Length;
        if (batCount == 0)
            return;

        stoppingDist = 0.1f;
        halfCount = batCount >> 1;

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

    
    void Update()
    {
        if (!isAttacking)
        {
            Collider[] player = Physics.OverlapSphere(transform.position, attackRange, playerLayer);
            if(player.Length > 0)
            {
                isAttacking = true;
                AttackPlayer();
            }
        }
        PrecomputeFrameValues();
        UpdateSwarmFast(isMoving);
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
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


    private void UpdateSwarmFast(bool moving)
    {

        float targetBlend = moving ? 0f : 1f;
        swarmBlend = Mathf.SmoothDamp(swarmBlend, targetBlend, ref blendVelocity, 0.35f);

        for (int i = 0; i < batCount; i++)
        {
            Transform t = bats[i].transform;
            float ph = phase[i];

            Vector3 targetPos;

            if (moving)
            {
                amplitude = 0.3f;
                float verticalAmplitude = 0.6f; // bigger up-down motion

                targetPos = cachedCenter + baseOffset[i];

                targetPos.x += Mathf.Sin(timeValue + ph) * amplitude;
                targetPos.y += Mathf.Sin(timeValue * 1.4f + ph) * amplitude; // <-- change here
                targetPos.z += Mathf.Sin(timeValue * 0.7f + ph) * amplitude;
            }
            else
            {
                amplitude = 0.5f;
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


    public float moveSpeed = 2.5f; // units per second
    public float holdDuration = 1f; // wait time at target



    public void GoTowardsFireCracker(Vector3 pos, GameObject fireCracker)
    {
        Sequence firecrackerSequence = DOTween.Sequence();
        Vector3 startPos = transform.position;

        // Move to target
        firecrackerSequence.Append(
            transform.DOMove(pos, moveSpeed)
                     .SetSpeedBased(true)
                     .SetEase(Ease.Linear)
                     .OnStart(() => isMoving = true)
                     .OnStart(() => inAttackPosition = false)
                     .OnComplete(() => isMoving = false)
        );

        // Wait at target
        firecrackerSequence.AppendInterval(holdDuration);

        // Move back to start
        firecrackerSequence.Append(
            transform.DOMove(startPos, moveSpeed)
                     .SetSpeedBased(true)
                     .SetEase(Ease.Linear)
                     .OnStart(() => isMoving = true)
                     .OnStart(() => Destroy(fireCracker))
                     .OnComplete(() => isMoving = false)
        );

        // Optional: callback when whole sequence is done
        firecrackerSequence.OnComplete(() =>
        {
            inAttackPosition = true;
            Debug.Log("Finished going to firecracker and back!");
        });
    }



    public float bumpDistance = 1f; // how far the bats bump
    public float bumpDuration = 0.3f; // time for each bump
    public float returnDuration = 0.5f; // time to return to original position

    private Vector3[] batStartPositions;

    private void AttackPlayer()
    {
        if (!inAttackPosition || isMoving) return;

        isMoving = true;

        Vector3 startPos = transform.position;
        StartCoroutine(MoveToPlayerAndAttack(startPos));
    }

    private IEnumerator MoveToPlayerAndAttack(Vector3 startPos)
    {
        // Move NPC toward player until within 1 unit
        while (Vector3.Distance(transform.position, player.position) > 1f)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
            yield return null;
        }

        // NPC reached attack position

        for (int i = 0; i < bats.Length; i++)
        {
            if (bats[i] == null) continue;

            Transform bat = bats[i].transform;
            Vector3 originalPos = batStartPositions[i];

            // Dynamic bump toward player
            float elapsed = 0f;

            // Move forward toward the player
            while (elapsed < bumpDuration)
            {
                Vector3 dirToPlayer = (player.position - transform.position).normalized;
                bat.localPosition = Vector3.Lerp(bat.localPosition, originalPos + dirToPlayer * bumpDistance, Time.deltaTime / (bumpDuration - elapsed));
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Return to original position
            elapsed = 0f;
            while (elapsed < returnDuration)
            {
                bat.localPosition = Vector3.Lerp(bat.localPosition, originalPos, Time.deltaTime / (returnDuration - elapsed));
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Return NPC to starting position
        float distanceToStart = Vector3.Distance(transform.position, startPos);
        float returnTime = distanceToStart / moveSpeed;

        transform.DOMove(startPos, returnTime).SetEase(Ease.Linear)
            .OnComplete(() => isMoving = false);
    }

}