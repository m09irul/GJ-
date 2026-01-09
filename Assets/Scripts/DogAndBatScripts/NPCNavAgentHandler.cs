using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCNavAgentHandler : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    private static readonly int WalkHash = Animator.StringToHash("walking");


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        if (!animator)
            animator = GetComponentInChildren<Animator>();
    }

    /* ======================
     * BASIC MOVEMENT
     * ====================== */

    public void MoveTo(Vector3 position)
    {
        if (!agent.enabled)
            return;

        agent.isStopped = false;
        agent.SetDestination(position);
        //transform.rotation = Quaternion.LookRotation(agent.velocity);
        transform.LookAt(position);

        SetWalking(true);
    }

    public void Stop()
    {
        if (!agent.enabled)
            return;

        agent.isStopped = true;
        SetWalking(false);

        agent.ResetPath();
    }

    public bool HasReachedDestination()
    {
        if (!agent.enabled || agent.pathPending)
            return false;

        bool reached = agent.remainingDistance <= agent.stoppingDistance;
        if (reached)
            SetWalking(false);

        return reached;
    }


    public void GoToTemporaryTarget(Vector3 position, Action onArrived)
    {
        //StopAllCoroutines();
        MoveTo(position);
        StartCoroutine(WaitUntilArrived(onArrived));
    }

    private IEnumerator WaitUntilArrived(Action onArrived)
    {
        while (!HasReachedDestination())
            yield return null;

        Stop();
        onArrived?.Invoke();
    }
    private void SetWalking(bool walking)
    {
        if (animator)
            animator.SetBool(WalkHash, walking);
    }
}
