using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FoodItem;

public class FireCracker : MonoBehaviour, Items
{
    public float range;
    public LayerMask NPCLayer;
    public GameObject closestBat;

    private bool isTriggered = false;
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Ground") && !isTriggered)
        {
            Debug.Log("FireCracker Triggered");
            isTriggered = true;
            StartCoroutine(waitToTrigger());
        }
    }

    IEnumerator waitToTrigger()
    {
        yield return new WaitForSeconds(0.5f);
        TriggerFoodFound();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        DrawRangeGizmo(transform.position, range, Color.green);
    }

    private void DrawRangeGizmo(Vector3 position, float range, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(position, range);
    }
#endif

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
    }

    private void OnEnable()
    {
        range = 20f;
        closestBat = null;
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
            Vector3 origin = transform.position;
            origin.y += 0.2f; // Adjust height if necessary
            Vector3 target = npc.transform.position;
            Vector3 dir = (target - origin);
            dir.Normalize();
            float dist = Vector3.Distance(origin, target);

            // Raycast to check obstruction
            if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
            {
                Debug.DrawLine(origin, hit.point, Color.red, 100f); // lasts 100 seconds
                Debug.Log("FireCracker Raycast Hit: " + hit.collider.name);
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
            closestBat.GetComponent<Bats>().GoTowardsFireCracker(transform.position, gameObject);
        }
    }
}
