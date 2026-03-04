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
    [SerializeField] private DogVisionCone dogVission;

    bool targetDetected = false;
    Coroutine chaseRoutine, coolDownRoutine, checkEndSearch;

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


    private void OnEnable()
    {
        dogVission.OnTargetDetected += TargetDetected;
        dogVission.OnTargetLost += TargetLost;
    }

    private void OnDisable()
    {
        dogVission.OnTargetDetected -= TargetDetected;
        dogVission.OnTargetLost -= TargetLost;
    }

    void TargetDetected(Transform player)
    {
        if (targetDetected)
            return;

        // If going towards final destination then stop
        if(checkEndSearch != null)
        {
            StopCoroutine(checkEndSearch);
            checkEndSearch = null;
        }

        targetDetected = true;
        agent.enabled = true;

        if (patrolSequence.IsPlaying())
        {
            patrolSequence.Pause();
            lastPatrolPosition = transform.position;
        }

        agent.SetDestination(player.position);

        if (coolDownRoutine != null)
        {
            StopCoroutine(coolDownRoutine);
            coolDownRoutine = null;
        }

        chaseRoutine = StartCoroutine(ChaseRoutine());
    }

    void TargetLost()
    {
        if (!targetDetected)
            return;
        
        targetDetected = false;
        if (chaseRoutine != null)
        {
            StopCoroutine(chaseRoutine);
            chaseRoutine = null;
        }
        checkEndSearch = StartCoroutine(checkForPlayerCompleteSearch());
    }

    IEnumerator ChaseRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            agent.SetDestination(player.position);
        }
    }

    IEnumerator CoolDownRouting()
    {
        StopCoroutine(checkEndSearch);
        checkEndSearch = null;
        yield return new WaitForSeconds(3f);
        agent.SetDestination(lastPatrolPosition);
        StartCoroutine(StartPatrollingAgain());
    }

    IEnumerator StartPatrollingAgain() {
        while (true)
        {
            yield return new WaitForSeconds(.5f);
            if (agent.remainingDistance < agent.stoppingDistance)
            {
                agent.enabled = false;
                StopAllCoroutines();
                patrolSequence.Play();
                dogVission.SetColor(dogVission.idleColor);
            }
        }
    }

    IEnumerator checkForPlayerCompleteSearch()
    {
        while (true)
        {
            yield return new WaitForSeconds(.5f);
            if (agent.remainingDistance < agent.stoppingDistance)
            {
                coolDownRoutine = StartCoroutine(CoolDownRouting());
            }
        }
    }
}