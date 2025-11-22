using UnityEngine;
using System.Collections;
public class DogPatrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float moveSpeed = 2f;
    public float stopDistance = 0.2f;

    private int patrolIndex = 0;
    private bool isPatrolling = true;

    public bool IsPatrolling => isPatrolling;
    private bool isPositiveDirection = true;
    private bool isDelayed = false;
    private void Update()
    {
        if (isPatrolling)
            PatrolMovement();
    }

    private void PatrolMovement()
    {
        if (patrolPoints.Length == 0) return;

        Vector3 target = patrolPoints[patrolIndex].position;
        target.y = transform.position.y; // lock height

        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        transform.LookAt(target);

        Debug.Log("Dog walking");

        if (Vector3.Distance(transform.position, target) <= stopDistance)
        {
            if (!isDelayed)
            {
                StartCoroutine(ChangeTarget());
            }
        }
    }

    IEnumerator ChangeTarget()
    {
        isDelayed = true;
        yield return new WaitForSeconds(2f);
        if(isPositiveDirection)
        {
            patrolIndex++;
            if (patrolIndex >= patrolPoints.Length)
            {
                patrolIndex = patrolPoints.Length - 2;
                isPositiveDirection = false;
            }
        }
        else
        {
            patrolIndex--;
            if (patrolIndex < 0)
            {
                patrolIndex = 1;
                isPositiveDirection = true;
            }
        }
        isDelayed = false;
    }

    public void StopPatrol()
    {
        isPatrolling = false;
        Debug.Log("Dog stopped patrol");
    }

    public void StartPatrol()
    {
        isPatrolling = true;
        Debug.Log("Dog started patrol");
    }
}
