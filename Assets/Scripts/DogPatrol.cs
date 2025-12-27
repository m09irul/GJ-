using UnityEngine;
using System.Collections;
using System.Buffers;
using DG.Tweening;

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
            //DoPatrol();
        }

        if(doPatrol == Patrol.doesnt)
        {
            StartCoroutine (MoveTowardsTarget());
        }
    }

    IEnumerator PatrolRoutine()
    {
        Debug.Log("PetrolRoutine");
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
    Sequence patrolSequence;
    void DoPatrol()
    {
        patrolSequence = DOTween.Sequence();

        patrolSequence.Append(transform.DOLookAt(patrolPoints[0].position, 0.3f));
        patrolSequence.Append(transform.DOMove(patrolPoints[0].position, 3f));

        patrolSequence.Append(transform.DOLookAt(patrolPoints[1].position, 0.3f));
        patrolSequence.Append(transform.DOMove(patrolPoints[1].position, 3f));

        patrolSequence.Append(transform.DOLookAt(patrolPoints[2].position, 0.3f));
        patrolSequence.Append(transform.DOMove(patrolPoints[2].position, 3f));

        patrolSequence.Append(transform.DOLookAt(patrolPoints[1].position, 0.3f));
        patrolSequence.Append(transform.DOMove(patrolPoints[1].position, 3f));

        patrolSequence.SetLoops(-1);
    }


    private void pausePatrol()
    {
        patrolSequence?.Pause();
    }

    private void resumePatrol() {
        patrolSequence?.Restart();
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
        Debug.Log("ChangePatrolPoint");
        gameObject.layer = LayerMask.NameToLayer("Dog");
        if (isPositiveDirection)
        {
            idx++;

            if (idx >= patrolPoints.Length - 1)
            {
                idx = patrolPoints.Length - 1;
                isPositiveDirection = false;
            }
        }
        else
        {
            idx--;

            if (idx <= 0)
            {
                idx = 0;
                isPositiveDirection = true;
            }
        }

    }

    public void StartPatrol()
    {
        //resumePatrol();
        // idx = 0;
        // isPositiveDirection = true;
        Debug.Log("StartPatroll");
        StopAllCoroutines();
        if(doPatrol == Patrol.does)
            StartCoroutine(PatrolRoutine());
        else
            return;
    }

    public void StopPatrol()
    {
        //pausePatrol();
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
