using UnityEngine;
using System.Collections;

public class DogPatrol : MonoBehaviour
{
    public enum Patrol
    {
        does,
        doesnt
    }

    [SerializeField] public Patrol doPatrol;

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
        
        if (doPatrol == Patrol.does)
        {
            StartCoroutine(PatrolRoutine());
        }

        if(doPatrol == Patrol.doesnt)
        {
            StartCoroutine (MoveTowardsTarget());
        }
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
            setAnimation("rig_walk");
            yield return StartCoroutine(WaitUntilArrived());
            yield return new WaitForSeconds(0.2f);
            setAnimation("rig_idle");
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
            setAnimation("rig_idle");
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
        if(doPatrol == Patrol.does)
            StartCoroutine(PatrolRoutine());
        else
            return;
    }

    public void StopPatrol()
    {
        StopAllCoroutines();
    }


    IEnumerator MoveTowardsTarget()
    {
        while (true)
        {
            if (navAgentHandler.isEventTriggered)
            {
                //setAnimation("rig_walk");
                Debug.Log("playWalk");
                yield return StartCoroutine(WaitUntilArrived());
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
                //setAnimation("rig_searching");
                Debug.Log("ReachedTarget");
                navAgentHandler.isEventTriggered = false;
                yield return new WaitForSeconds(2f);
                //setAnimation("rig_idle");
                Debug.Log("playIdle");
            }
            
        }
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    public void setAnimation(string anim)
    {
        animator.Play(anim);
    }
}
