using UnityEngine;
using System.Collections;

public class DogPatrol : MonoBehaviour
{
    [SerializeField] private NPCNavAgentHandler navAgentHandler;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] public Transform restPosition;
    private bool isPositiveDirection = true;
    private int idx = 0;
    [SerializeField] private Vector3 nextPoint;
    public bool isGoingResting = false;

    private void Start()
    {
        isGoingResting = false;
        navAgentHandler = GetComponent<NPCNavAgentHandler>();

        StartCoroutine(PatrolRoutine());
    }

    IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (navAgentHandler.isEventTriggered)
            {
                StopPatrol();
                break;
            }
            nextPoint = patrolPoints[idx].position;
            if (isGoingResting)
            {
                nextPoint = restPosition.position;
            }
            navAgentHandler.MoveNext(nextPoint);
            transform.LookAt(nextPoint);
            animator.Play("rig_walk");
            
            yield return StartCoroutine(WaitUntilArrived());
            yield return new WaitForSeconds(0.2f);
            animator.Play("rig_idle");
            yield return new WaitForSeconds(2f);

            ChangePatrolPoint();
        }
    }

    IEnumerator WaitUntilArrived()
    {
        // Wait until Agent has a path
        while (navAgentHandler.GetpathPending())
            yield return null;

        // Wait until remainingDistance <= stopDistance
        while (navAgentHandler.getRemainingDistance() >
               navAgentHandler.getStopDistance())
        {
            yield return null;
        }
    }

    public void ChangePatrolPoint()
    {
        if (isGoingResting)
        {
            animator.Play("rig_idle");
            navAgentHandler.isEventTriggered = false;
            gameObject.layer = LayerMask.NameToLayer("Default");
            return;
        }

        gameObject.layer = LayerMask.NameToLayer("Dog");
        if (isPositiveDirection)
        {
            idx++;
            if (idx >= patrolPoints.Length)
            {
                idx = patrolPoints.Length - 2;
                isPositiveDirection = false;
            }
        }
        else
        {
            idx--;
            if (idx < 0)
            {
                idx = 1;
                isPositiveDirection = true;
            }
        }
    }

    public void StartPatrol()
    {
        StartCoroutine(PatrolRoutine());
    }

    public void StopPatrol()
    {
        StopAllCoroutines();
    }


    public void setAnimation(string anim)
    {
        animator.Play(anim);
    }
}
