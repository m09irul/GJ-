using UnityEngine;
using System.Collections;

public class DogPatrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    public Transform restingPoint;
    public float moveSpeed = 2f;
    public float stopDistance = 0.2f;
    public float eatDistance = 1f;

    private int patrolIndex = 0;
    private bool isPatrolling = true;
    private bool isPositiveDirection = true;
    private bool isDelayed = false;
    private bool isResting = false;

    private GameObject targetFood = null;
    private bool movingToFood = false;

    [SerializeField] private Animator animator;
    [SerializeField] private Finder finder;

    public bool IsPatrolling => isPatrolling;

    private void Start()
    {
        if (finder != null)
        {
            finder.OnTargetFound += OnFoodDetected;
        }
    }

    private void Update()
    {
        if (isResting) return;

        if (movingToFood && targetFood != null)
        {
            MoveToFood();
        }
        else if (isPatrolling)
        {
            PatrolMovement();
        }
    }

    private void OnFoodDetected(GameObject food)
    {
        targetFood = food;
        movingToFood = true;
        isPatrolling = false;

        if (animator != null)
            animator.Play("rig_walk");

        Debug.Log("Dog detected food and moving towards it");
    }

    private void MoveToFood()
    {
        if (targetFood == null)
        {
            movingToFood = false;
            StartPatrol();
            return;
        }

        Vector3 target = targetFood.transform.position;
        target.y = transform.position.y;

        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        transform.LookAt(target);

        if (Vector3.Distance(transform.position, target) <= eatDistance)
        {
            StartCoroutine(EatFood());
        }
    }

    IEnumerator EatFood()
    {
        movingToFood = false;

        if (animator != null)
            animator.Play("rig_idle");

        yield return new WaitForSeconds(1f);

        FoodItem foodItem = targetFood.GetComponent<FoodItem>();

        if (foodItem != null)
        {
            if (foodItem.foodType == FoodItem.FoodType.edible)
            {
                foodItem.ConsumeFood();
                targetFood = null;

                if (restingPoint != null)
                {
                    StartCoroutine(MoveToRestingPoint());
                }
                else
                {
                    Debug.LogWarning("Resting point not assigned!");
                    targetFood = null;
                    movingToFood = false;
                    StartPatrol();
                    if (finder != null)
                        finder.StartSearching();
                }
            }
            else
            {
                // Inedible food - go back to patrolling
                foodItem.ConsumeFood();
                targetFood = null;
                movingToFood = false;

                yield return new WaitForSeconds(0.5f);

                StartPatrol();
                if (finder != null)
                    finder.StartSearching();
            }
        }
        else
        {
            // Food was destroyed or null - go back to patrolling
            targetFood = null;
            movingToFood = false;
            StartPatrol();
            if (finder != null)
                finder.StartSearching();
        }
    }

    IEnumerator MoveToRestingPoint()
    {
        if (animator != null)
            animator.Play("rig_walk");

        while (Vector3.Distance(transform.position, restingPoint.position) > stopDistance)
        {
            Vector3 target = restingPoint.position;
            target.y = transform.position.y;

            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            transform.LookAt(target);

            yield return null;
        }

        if (animator != null)
            animator.Play("rig_idle");

        isResting = true;
        Debug.Log("Dog reached resting point and will stay here");
    }

    private void PatrolMovement()
    {
        if (patrolPoints.Length == 0) return;

        Vector3 target = patrolPoints[patrolIndex].position;
        target.y = transform.position.y;

        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        transform.LookAt(target);

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
        if (animator != null)
            animator.Play("rig_idle");

        isDelayed = true;
        yield return new WaitForSeconds(2f);

        if (isPositiveDirection)
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

        if (animator != null)
            animator.Play("rig_walk");
    }

    public void StopPatrol()
    {
        isPatrolling = false;
        Debug.Log("Dog stopped patrol");
    }

    public void StartPatrol()
    {
        if (!isResting)
        {
            isPatrolling = true;

            if (animator != null)
                animator.Play("rig_walk");

            Debug.Log("Dog started patrol");
        }
    }

    private void OnDestroy()
    {
        if (finder != null)
        {
            finder.OnTargetFound -= OnFoodDetected;
        }
    }
}