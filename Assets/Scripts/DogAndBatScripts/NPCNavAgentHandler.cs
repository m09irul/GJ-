using System;
using UnityEngine;
using UnityEngine.AI;

public class NPCNavAgentHandler : MonoBehaviour
{

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private bool eventTriggered = false;
    DogPatrol dogPatrol;

    // Start is called before the first frame update
    void Start()
    {
        dogPatrol = GetComponent<DogPatrol>();
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
        isEventTriggered = false;
        dogPatrol.StartPatrol();
    }

    public void GoToRestingPoint()
    {
        dogPatrol.isGoingResting = true;
        GoBackToPatrol();
    }
}
