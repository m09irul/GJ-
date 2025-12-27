using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FoodItem;

public class FireCracker : MonoBehaviour, Items
{
    public float range;
    public LayerMask NPCLayer;
    public GameObject closestBat;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Bat"))
        {
            StartCoroutine(destroyFireCracker());
        }
    }

    IEnumerator destroyFireCracker()
    {
        yield return new WaitForSeconds(2f);
        closestBat.GetComponent<NPCNavAgentHandler>().isEventTriggered = false;
        StopAllCoroutines();
        Destroy(gameObject);
    }
    private void Start()
    {
        range = 20f;
        closestBat = null;
        TriggerFoodFound();
    }

    private void OnEnable()
    {
        range = 20f;
        closestBat = null;
        TriggerFoodFound();
    }

    public void TriggerFoodFound()
    {
        Collider[] dogs = Physics.OverlapSphere(
            transform.position,
            range,
            NPCLayer,
            QueryTriggerInteraction.Collide
        );

        closestBat = null;
        float closestDist = Mathf.Infinity;

        foreach (Collider npc in dogs)
        {
            // Height check (your original logic)
            if (transform.position.y > npc.transform.position.y + 0.5f)
                continue;

            Vector3 origin = transform.position;
            Vector3 target = npc.transform.position;
            Vector3 dir = (target - origin).normalized;
            float dist = Vector3.Distance(origin, target);

            // Raycast to check obstruction
            if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
            {
                // If ray hits something OTHER than the npc ? blocked
                if (hit.collider != npc)
                    continue;
            }

            // Find closest visible dog
            if (dist < closestDist)
            {
                closestDist = dist;
                closestBat = npc.gameObject;
            }
        }

        if (closestBat != null)
        {
            closestBat
                .GetComponent<NPCEventManager>()
                .GotoTarget(transform.position);
        }
    }
}
