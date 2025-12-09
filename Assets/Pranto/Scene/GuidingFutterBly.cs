using UnityEngine;
using UnityEngine.AI;

public class GuidingFlutterBly : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform destination;

    [Header("Spawn Settings")]
    public float spawnBehindDistance = 1.5f;
    public float floatHeight = 1.2f;
    public float frontDistance = 2f;

    [Header("Movement Settings")]
    public float floatSpeed = 3f;
    public float rotationSpeed = 5f;

    private NavMeshAgent agent;
    private Vector3 frontPoint;
    private bool movingToFront = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        SpawnBehindPlayer();
        CalculateFrontPoint();

        // Stop the agent until we reach the front
        agent.enabled = false;
    }

    void Update()
    {
        if (movingToFront)
        {
            MoveToFront();
        }
        else
        {
            RotateTowards(agent.steeringTarget);
        }
    }

    // ---------------------------------------
    // SPAWN BEHIND PLAYER
    // ---------------------------------------
    void SpawnBehindPlayer()
    {
        Vector3 behind = player.position - player.forward * spawnBehindDistance;
        behind.y += floatHeight;
        transform.position = behind;
    }

    // ---------------------------------------
    // FRONT FLOAT POINT
    // ---------------------------------------
    void CalculateFrontPoint()
    {
        frontPoint = player.position + player.forward * frontDistance;
        frontPoint.y += floatHeight;
    }

    // ---------------------------------------
    // FLOAT TO FRONT
    // ---------------------------------------
    void MoveToFront()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            frontPoint,
            Time.deltaTime * floatSpeed
        );

        RotateTowards(frontPoint);

        if (Vector3.Distance(transform.position, frontPoint) < 0.2f)
        {
            movingToFront = false;
            StartNavmeshMovement();
        }
    }

    // ---------------------------------------
    // USE NAVMESH AGENT
    // ---------------------------------------
    void StartNavmeshMovement()
    {
        agent.enabled = true;
        agent.SetDestination(destination.position);
    }

    // ---------------------------------------
    // SMOOTH ROTATION
    // ---------------------------------------
    void RotateTowards(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotationSpeed
        );
    }
}
