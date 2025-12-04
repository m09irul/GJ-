using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finder : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private LayerMask targetLayer;
    private Coroutine runningCoroutine;

    public System.Action<GameObject> OnTargetFound;
    private bool isSearching = true;

    void Start()
    {
        runningCoroutine = StartCoroutine(DetectTargets());
    }

    IEnumerator DetectTargets()
    {
        while (isSearching)
        {
            Collider[] detectedColliders = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);
            if (detectedColliders.Length > 0)
            {
                float min = Mathf.Infinity;
                GameObject closestFood = null;

                foreach (Collider food in detectedColliders)
                {
                    float distance = Vector3.Distance(transform.position, food.transform.position);
                    if (distance < min)
                    {
                        FoodItem foodItem = food.GetComponent<FoodItem>();
                        if (foodItem != null && !foodItem.IsLocked)
                        {
                            min = distance;
                            closestFood = food.gameObject;
                        }
                    }
                }

                if (closestFood != null)
                {
                    closestFood.GetComponent<FoodItem>().LockFood();
                    OnTargetFound?.Invoke(closestFood);
                    StopSearching();
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public void StopSearching()
    {
        isSearching = false;
        if (runningCoroutine != null)
            StopCoroutine(runningCoroutine);
    }

    public void StartSearching()
    {
        if (!isSearching)
        {
            isSearching = true;
            runningCoroutine = StartCoroutine(DetectTargets());
        }
    }
}