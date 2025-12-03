using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finder : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private LayerMask targetLayer;

    private Coroutine runningCoroutine;
    // Start is called before the first frame update
    void Start()
    {
        runningCoroutine = StartCoroutine(DetectTargets());
    }

    IEnumerator DetectTargets()
    {
        bool isActiveAndEnabled = true;
        while (isActiveAndEnabled)
        {
            Collider[] detectedColliders = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);

            if (detectedColliders.Length > 0)
            {
                float min = Mathf.Infinity;
                GameObject closestFood = null;
                foreach (Collider food in detectedColliders)
                {
                    if(Vector3.Distance(transform.position, food.transform.position) < min)
                    {
                        min = Vector3.Distance(transform.position, food.transform.position);
                        closestFood = food.gameObject;
                    }
                }
                isActiveAndEnabled = false;
                closestFood.GetComponent<FoodItem>().lockFood();
                stopFinding();
                StartCoroutine(restartFinding());
            }
            yield return new WaitForSeconds(1f); // Check every second
        }
    }

    private void stopFinding()
    {
        if (runningCoroutine != null)
            StopCoroutine(runningCoroutine);
    }

    IEnumerator restartFinding()
    {
        yield return new WaitForSeconds(20f);
        StartCoroutine(DetectTargets());
    }

}
