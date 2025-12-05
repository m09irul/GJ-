using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCEventManager : MonoBehaviour
{
    [SerializeField] private NPCNavAgentHandler navmeshHandler;
    // Start is called before the first frame update
    void Start()
    {
        navmeshHandler = GetComponent<NPCNavAgentHandler>();
    }

    public void FoodCollectEvent(Vector3 pos)
    {
        Debug.Log("NPC Food Event Triggered");
        gameObject.layer = LayerMask.NameToLayer("Dog");
        navmeshHandler.isEventTriggered = true;
        navmeshHandler.MoveNext(pos);
    }

    public void inedibleEvent()
    {
        StartCoroutine(startEvent(false));
    }

    IEnumerator startEvent(bool isEdible)
    {
        yield return new WaitForSeconds(2f);
        if (isEdible)
        {
            navmeshHandler.GoToRestingPoint();
        }
        else
        {
            navmeshHandler.isEventTriggered = false;
            navmeshHandler.GoBackToPatrol();
        }

        StopAllCoroutines();
    }

    public void EdibleEvent()
    {
        StartCoroutine(startEvent(true));
    }
}
