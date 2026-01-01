using UnityEngine;

public class FoodItem : MonoBehaviour, Items
{
    public enum FoodType
    {
        edible,
        inedible
    }
    public float range;
    public LayerMask NPCLayer;
    public FoodType foodType;
    public GameObject clossestDog;

    private void Start()
    {
        range = 20f;
        clossestDog = null;
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


    private void OnEnable()
    {
        range = 20f;
        clossestDog = null;
        TriggerFoodFound();
    }

    //public void TriggerFoodFound()
    //{
    //    Collider[] dog = Physics.OverlapSphere(transform.position, range, NPCLayer, QueryTriggerInteraction.Collide);
    //    foreach (Collider npc in dog)
    //    {
    //        if (transform.position.y > npc.gameObject.transform.position.y + .5f)
    //            continue;
    //        if (clossestDog == null)
    //        {
    //            clossestDog = npc.gameObject;
    //        }
    //        else
    //        {
    //            float dist1 = Vector3.Distance(transform.position, npc.transform.position);
    //            float dist2 = Vector3.Distance(transform.position, clossestDog.transform.position);
    //            if (dist1 < dist2)
    //            {
    //                clossestDog = npc.gameObject;
    //            }
    //        }
    //    }
    //    clossestDog.GetComponent<NPCEventManager>().GotoTarget(transform.position);

    //}

    public void TriggerFoodFound()
    {
        Collider[] dogs = Physics.OverlapSphere(
            transform.position,
            range,
            NPCLayer,
            QueryTriggerInteraction.Collide
        );

        clossestDog = null;
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
                // If ray hits something OTHER than the npc → blocked
                if (hit.collider != npc)
                    continue;
            }

            // Find closest visible dog
            if (dist < closestDist)
            {
                closestDist = dist;
                clossestDog = npc.gameObject;
            }
        }

        if (clossestDog != null)
        {
            clossestDog
                .GetComponent<NPCEventManager>()
                .GotoTarget(transform.position);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("collided");
        if (other.gameObject == clossestDog)
        {
            clossestDog.GetComponent<DogPatrol>().setAnimation("rig_idle");
            if (foodType == FoodType.edible)
            {
                clossestDog.GetComponent<NPCEventManager>().EdibleEvent();
            }
            else
            {
                clossestDog.GetComponent<NPCEventManager>().inedibleEvent();
            }
            Destroy(gameObject);
        }
    }
}