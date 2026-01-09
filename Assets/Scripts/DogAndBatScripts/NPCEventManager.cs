// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.AI;

// public class NPCEventManager : MonoBehaviour
// {
//     [SerializeField] private NPCNavAgentHandler navmeshHandler;
//     private DogPatrol dogPatrol;
//     // Start is called before the first frame update
//     void Start()
//     {
//         dogPatrol = GetComponent<DogPatrol>();
//         navmeshHandler = GetComponent<NPCNavAgentHandler>();
//     }

//     public void GotoTarget(Vector3 pos)
//     {
//         Debug.Log("NPC Food Event Triggered");
//         navmeshHandler.isEventTriggered = true;
//         navmeshHandler.MoveNext(pos);
//         dogPatrol.setAnimation("rig_walk");
//     }

//     public void inedibleEvent()
//     {
//         StartCoroutine(startEvent(false));
//     }

//     IEnumerator startEvent(bool isEdible)
//     {
//         yield return new WaitForSeconds(2f);
//         if (isEdible)
//         {
//             navmeshHandler.GoToRestingPoint();
//         }
//         else
//         {
//             navmeshHandler.isEventTriggered = false;
//             navmeshHandler.GoBackToPatrol();
//         }
//         dogPatrol.setAnimation("rig_walk");
//         StopAllCoroutines();
//     }

//     public void EdibleEvent()
//     {
//         StartCoroutine(startEvent(true));
//     }
// }
