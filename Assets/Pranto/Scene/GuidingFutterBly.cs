using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class GuidingFutterBly : MonoBehaviour
{
    public Transform player;
    public Transform destination;

    public float spawnBehindDistance = 2f;
    public float frontDistance = 2f;
    public float moveSpeed = 6f;
    public float heightOffset = 1.5f;

    private NavMeshPath navPath;
    private int currentCornerIndex = 0;
    private enum State { SpawnBehind, MoveInFront, FollowPath }
    private State state;

    void Start()
    {
        navPath = new NavMeshPath();

        // Start behind the player
        transform.position = player.position - player.forward * spawnBehindDistance + Vector3.up * heightOffset;

        state = State.SpawnBehind;
    }

    void Update()
    {
        switch (state)
        {
            case State.SpawnBehind:
                MoveBehindToFront();
                break;

            case State.MoveInFront:
                MoveInFrontToPathStart();
                break;

            case State.FollowPath:
                FollowNavPath();
                break;
        }
    }

    void MoveBehindToFront()
    {
        Vector3 targetPos = player.position + player.forward * frontDistance + Vector3.up * heightOffset;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 3f);

        if (Vector3.Distance(transform.position, targetPos) < 0.4f)
        {
            BuildPath();
            state = State.MoveInFront;
        }
    }

    void MoveInFrontToPathStart()
    {
        if (navPath.corners.Length < 2) return;

        Vector3 firstCorner = navPath.corners[1] + Vector3.up * heightOffset;

        transform.position = Vector3.MoveTowards(
            transform.position,
            firstCorner,
            Time.deltaTime * moveSpeed
        );

        if (Vector3.Distance(transform.position, firstCorner) < 0.2f)
        {
            currentCornerIndex = 1;
            state = State.FollowPath;
        }
    }

    void FollowNavPath()
    {
        if (currentCornerIndex >= navPath.corners.Length) return;

        Vector3 targetCorner = navPath.corners[currentCornerIndex] + Vector3.up * heightOffset;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetCorner,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetCorner) < 0.2f)
        {
            currentCornerIndex++;

            // Destination reached → particle can fade/stop
            if (currentCornerIndex >= navPath.corners.Length)
            {
                Destroy(gameObject, 1f);
            }
        }
    }

    void BuildPath()
    {
        NavMesh.CalculatePath(player.position, destination.position, NavMesh.AllAreas, navPath);
    }
}
