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
        Collider[] dog = Physics.OverlapSphere(transform.position, range, NPCLayer, QueryTriggerInteraction.Collide);
        foreach (Collider npc in dog)
        {
            if (transform.position.y > npc.gameObject.transform.position.y + .5f)
                continue;
            if (closestBat == null)
            {
                closestBat = npc.gameObject;
            }
            else
            {
                float dist1 = Vector3.Distance(transform.position, npc.transform.position);
                float dist2 = Vector3.Distance(transform.position, closestBat.transform.position);
                if (dist1 < dist2)
                {
                    closestBat = npc.gameObject;
                }
            }
        }
        closestBat.GetComponent<NPCEventManager>().GotoTarget(transform.position);
    }
}
