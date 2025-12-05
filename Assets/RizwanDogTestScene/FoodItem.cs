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

    private void OnEnable()
    {
        range = 20f;
        clossestDog = null;
        TriggerFoodFound();
    }

    public void TriggerFoodFound()
    {
        Collider[] dog = Physics.OverlapSphere(transform.position, range, NPCLayer, QueryTriggerInteraction.Collide);
        foreach (Collider npc in dog)
        {
            if (transform.position.y > npc.gameObject.transform.position.y + .5f)
                continue;
            if (clossestDog == null)
            {
                clossestDog = npc.gameObject;
            }
            else
            {
                float dist1 = Vector3.Distance(transform.position, npc.transform.position);
                float dist2 = Vector3.Distance(transform.position, clossestDog.transform.position);
                if (dist1 < dist2)
                {
                    clossestDog = npc.gameObject;
                }
            }
        }
        clossestDog.GetComponent<NPCEventManager>().FoodCollectEvent(transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("collided");
        if (other.gameObject == clossestDog)
        {
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