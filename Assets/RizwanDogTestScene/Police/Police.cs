using DG.Tweening;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Sequence = DG.Tweening.Sequence;

public class Police : MonoBehaviour
{
    public enum patrolInfo
    {
        doPatrol, dontPatrol
    }
    [SerializeField] private float speed = 2f;

    [Header("Patrolling")]
    public patrolInfo patrolling;
    [SerializeField] private List<Transform> patrolPoints;
    private Sequence patrollSequence;
    private Vector3 patrollStartPosition;

    [Header("AI Chasing")]
    [SerializeField] private NavMeshAgent agent;

    [Header("Player Detection")]
    [SerializeField] private Transform player;
    public DogVisionCone visionCone;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;

        //player = GameManager.Instance.player.gameObject.transform;

        if (patrolling == patrolInfo.doPatrol)
        {
            patrollStartPosition = transform.position;
            patrollSequence = MoveThroughPoints(patrolPoints, speed, true);
            patrollSequence.Play();
        }
    }

    public Sequence MoveThroughPoints(List<Transform> points, float speed, bool loop = true)
    {
        if (points == null || points.Count == 0)
            return null;

        Sequence sequence = DOTween.Sequence();
        Vector3 startPosition = transform.position;

        for (int i = 0; i < points.Count; i++)
        {
            float durationPerPoint;
            if (i == 0)
            {
                durationPerPoint = Vector3.Distance(transform.position, points[i].position) / speed;
            }
            else
            {
                durationPerPoint = Vector3.Distance(points[i].position, points[i - 1].position) / speed;
            }
            sequence.Append(transform.DOLookAt(points[i].position, 0f)).Append(
                transform.DOMove(points[i].position, durationPerPoint)
            );
        }
        for (int i = points.Count-2 ; i >= -1 ; i--)
        {
            float durationPerPoint;
            if (i == -1)
            {
                durationPerPoint = Vector3.Distance(startPosition, points[i + 1].position) / speed;
                sequence.Append(transform.DOLookAt(startPosition, 0f)).Append(
                    transform.DOMove(startPosition, durationPerPoint)
                );
                break;
            }
            else
            {
                durationPerPoint = Vector3.Distance(points[i].position, points[i + 1].position) / speed;
                sequence.Append(transform.DOLookAt(points[i].position, 0f)).Append(
                    transform.DOMove(points[i].position, durationPerPoint)
                );
            }
        }

        if (loop)
            sequence.SetLoops(-1, LoopType.Restart);

        return sequence;
    }


    private void OnEnable()
    {
        visionCone.OnTargetDetected += HandleTargetDetected;
        visionCone.OnTargetLost += HandleTargetLost;

    }


    private void OnDisable()
    {
        visionCone.OnTargetDetected -= HandleTargetDetected;
        visionCone.OnTargetLost -= HandleTargetLost;
    }
    private bool isAlreadyChasing;
    void HandleTargetDetected(Transform target)
    {
        if (isAlreadyChasing) return;
        isAlreadyChasing = true;

        agent.SetDestination(target.position);
    }

    void HandleTargetLost()
    {

    }
}
