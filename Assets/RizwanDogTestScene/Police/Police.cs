using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sequence = DG.Tweening.Sequence;

public class Police : MonoBehaviour
{
    public enum PatrolInfo
    {
        DoPatrol,
        DontPatrol
    }

    [SerializeField] private float speed = 2f;

    [Header("Patrolling")]
    public PatrolInfo patrolling;
    [SerializeField] private List<Transform> patrolPoints;
    private Sequence patrolSequence;
    private Vector3 lastPatrolPosition;

    [Header("AI Chasing")]
    [SerializeField] private NavMeshAgent agent;

    [Header("Player Detection")]
    [SerializeField] private Transform player;
    public float detectionRange = 3f;
    public LayerMask playerLayer;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;

        // player = GameManager.Instance.player.transform;

        if (patrolling == PatrolInfo.DoPatrol)
        {
            lastPatrolPosition = transform.position;
            patrolSequence = MoveThroughPoints(patrolPoints, speed, true);

            if (patrolSequence != null)
                patrolSequence.Play();
        }

    }

    public Sequence MoveThroughPoints(List<Transform> points, float speed, bool loop = true)
    {
        if (points == null || points.Count == 0)
            return null;

        Sequence sequence = DOTween.Sequence();
        Vector3 startPosition = transform.position;

        // Forward
        for (int i = 0; i < points.Count; i++)
        {
            float durationPerPoint;

            if (i == 0)
                durationPerPoint = Vector3.Distance(transform.position, points[i].position) / speed;
            else
                durationPerPoint = Vector3.Distance(points[i].position, points[i - 1].position) / speed;

            sequence
                .Append(transform.DOLookAt(points[i].position, 0f))
                .Append(transform.DOMove(points[i].position, durationPerPoint));
        }

        // Backward
        for (int i = points.Count - 2; i >= -1; i--)
        {
            float durationPerPoint;

            if (i == -1)
            {
                durationPerPoint = Vector3.Distance(startPosition, points[i + 1].position) / speed;

                sequence
                    .Append(transform.DOLookAt(startPosition, 0f))
                    .Append(transform.DOMove(startPosition, durationPerPoint));

                break;
            }
            else
            {
                durationPerPoint = Vector3.Distance(points[i].position, points[i + 1].position) / speed;

                sequence
                    .Append(transform.DOLookAt(points[i].position, 0f))
                    .Append(transform.DOMove(points[i].position, durationPerPoint));
            }
        }

        if (loop)
            sequence.SetLoops(-1, LoopType.Restart);

        return sequence;
    }

    IEnumerator checkForPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            SearchForPlayer();
        }
    }


    private void OnEnable()
    {
        Temp.WantedLevelUpdate += HandleWantedLevel;
        //GameManager.Instance.WantedLevelUpdate += HandleWantedLevel;
    }

    private void OnDisable()
    {
        Temp.WantedLevelUpdate -= HandleWantedLevel;
        //GameManager.Instance.WantedLevelUpdate -= HandleWantedLevel;
    }

    void HandleWantedLevel(int wantedLevel)
    {
        if (wantedLevel > 0)
        {
            StartCoroutine(checkForPlayer());
        }
        else
        {
            StopCoroutine(checkForPlayer());
            agent.SetDestination(lastPatrolPosition);
            StartCoroutine(isReachedPatrolPosition());
        }
    }

    IEnumerator isReachedPatrolPosition()
    {
        while (true) 
        {
            yield return new WaitForSeconds(1f);
            if (agent.remainingDistance < 0.2f)
            {
                patrolSequence.Play();
                break;
            }
        }
    }


    private void SearchForPlayer()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;

        if (direction.sqrMagnitude < detectionRange * detectionRange)
        {
            if (IsVisible(direction))
            {
                if (patrolSequence.IsPlaying())
                {
                    patrolSequence.Pause();
                    lastPatrolPosition = transform.position;
                }

                agent.enabled = true;
                agent.SetDestination(player.position);
            }
        }
    }

    private bool IsVisible(Vector3 direction)
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, direction.normalized, out hit, detectionRange))
        {
            if (hit.transform == player)
            {
                return true; // Direct line of sight
            }
        }

        return false; // Blocked or nothing hit
    }
}