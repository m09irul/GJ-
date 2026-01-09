// using System.Collections;
// using UnityEngine;

// public class FoodItem : MonoBehaviour, Items
// {
//     public enum FoodType
//     {
//         edible,
//         inedible
//     }

//     [SerializeField] private float range = 20f;
//     [SerializeField] private LayerMask NPCLayer;
//     [SerializeField] private FoodType foodType;

//     private GameObject closestDog;
//     private bool onceTriggered;

// #if UNITY_EDITOR
//     private void OnDrawGizmosSelected()
//     {
//         Gizmos.color = Color.green;
//         Gizmos.DrawWireSphere(transform.position, range);
//     }
// #endif

//     /* =========================
//      * FIND DOG
//      * ========================= */

//     public void TriggerFoodFound()
//     {
//         Collider[] dogs = Physics.OverlapSphere(
//             transform.position,
//             range,
//             NPCLayer,
//             QueryTriggerInteraction.Collide
//         );

//         closestDog = null;
//         float closestDist = Mathf.Infinity;

//         foreach (Collider npc in dogs)
//         {
//             Vector3 origin = transform.position;
//             Vector3 target = npc.transform.position;

//             Vector3 dir = target - origin;
//             dir.y = 0f;
//             float dist = dir.magnitude;
//             dir.Normalize();

//             // Obstruction check
//             if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
//             {
//                 if (hit.collider != npc)
//                     continue;
//             }

//             if (dist < closestDist)
//             {
//                 closestDist = dist;
//                 closestDog = npc.gameObject;
//             }
//         }

//         if (closestDog != null)
//         {
//             NPCNavAgentHandler agent = closestDog.GetComponent<NPCNavAgentHandler>();
//             DogPatrol patrol = closestDog.GetComponent<DogPatrol>();

//             if (agent && patrol)
//             {
//                 patrol.StopPatrol();
//                 agent.GoToTemporaryTarget(transform.position, OnDogReachedFood);
//             }
//         }
//     }

//     /* =========================
//      * DOG REACHED FOOD
//      * ========================= */

//     private void OnDogReachedFood()
//     {
//         if (!closestDog) return;

//         DogPatrol patrol = closestDog.GetComponent<DogPatrol>();

//         if (patrol)
//         {
//             patrol.setAnimation("rig_idle");
//         }

//         if (foodType == FoodType.edible)
//         {
//             StartCoroutine(EdibleRoutine());
//         }
//         else
//         {
//             StartCoroutine(InedibleRoutine());
//         }
//     }

//     private IEnumerator EdibleRoutine()
//     {
//         yield return new WaitForSeconds(1f);

//         ResumeDogBehavior();
//         Destroy(gameObject);
//     }

//     private IEnumerator InedibleRoutine()
//     {
//         yield return new WaitForSeconds(0.5f);

//         // Optional reaction logic here (bark, shake head, etc.)

//         ResumeDogBehavior();
//         Destroy(gameObject);
//     }

//     private void ResumeDogBehavior()
//     {
//         if (!closestDog) return;

//         DogPatrol patrol = closestDog.GetComponent<DogPatrol>();
//         DogVisionCone vision = closestDog.GetComponent<DogVisionCone>();

//         patrol?.StartPatrol();
//         vision?.OnMovementStarted();
//     }

//     /* =========================
//      * TRIGGERS
//      * ========================= */

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Ground") && !onceTriggered)
//         {
//             onceTriggered = true;
//             TriggerFoodFound();
//         }
//     }
// }
