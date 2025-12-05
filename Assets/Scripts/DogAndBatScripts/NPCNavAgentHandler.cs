using System;
using UnityEngine;
using UnityEngine.AI;

public class NPCNavAgentHandler : MonoBehaviour
{

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private bool eventTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void MoveNext(Vector3 position)
    {
        targetPosition = position;
        agent.SetDestination(position);
    }

    public bool isEventTriggered
    {
        get { return eventTriggered; }
        set { eventTriggered = value; }
    }

    public float getStopDistance()
    {
        return agent.stoppingDistance;
    }

    public float getRemainingDistance()
    {
        return agent.remainingDistance;
    }

    public bool GetpathPending()
    {
        return agent.pathPending;
    }

    public void GoBackToPatrol()
    {
        gameObject.GetComponent<DogPatrol>().StartPatrol();
    }

    public void GoToRestingPoint()
    {
        agent.SetDestination(gameObject.GetComponent<DogPatrol>().restPosition.position);
    }
}
