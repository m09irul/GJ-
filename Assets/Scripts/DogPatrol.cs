using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NPCNavAgentHandler))]
public class DogPatrol : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform restPoint;
    [SerializeField] private float idleTime = 2f;

    private NPCNavAgentHandler agent;
    private Coroutine patrolRoutine;

    private int patrolIndex = 0;
    private int direction = 1; // +1 forward, -1 backward


    private void Awake()
    {
        agent = GetComponent<NPCNavAgentHandler>();

    }

    private void Start()
    {
        StartPatrol();
    }

    /* =======================
     * PUBLIC CONTROL
     * ======================= */

    public void StartPatrol()
    {
        //StopPatrol();
        patrolRoutine = StartCoroutine(PatrolRoutine());
    }

    public void StopPatrol()
    {
        if (patrolRoutine != null)
            StopCoroutine(patrolRoutine);

        agent.Stop();
    }

    public void GoToRest()
    {
        StopPatrol();
        agent.MoveTo(restPoint.position);
    }

    /* =======================
     * CORE LOGIC
     * ======================= */

    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            agent.MoveTo(patrolPoints[patrolIndex].position);

            // wait until destination reached
            yield return new WaitUntil(agent.HasReachedDestination);

            yield return new WaitForSeconds(idleTime);

            AdvancePatrolIndex();
        }
    }

    private void AdvancePatrolIndex()
    {
        patrolIndex += direction;

        if (patrolIndex >= patrolPoints.Length - 1 || patrolIndex <= 0)
            direction *= -1;
    }


}
