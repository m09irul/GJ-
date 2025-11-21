using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class FireFlies : MonoBehaviour
{
    public GameObject fireFlyPrefab;
    [HideInInspector]private Transform CameraTransform;
    [HideInInspector]private Transform player;
    private NavMeshAgent agent;
    public Transform destination;
    public bool isFireflyActive = false;
    private bool towardsDestination = false;
    // Start is called before the first frame update
    void Start()
    {
        agent = fireFlyPrefab.GetComponent<NavMeshAgent>();
        CameraTransform = GameObject.FindWithTag("MainCamera").transform;
        player = GameObject.FindWithTag("cat").transform;
        fireFlyPrefab.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Jump"))
        {
            fireFlyPrefab.SetActive(true);
            fireFlyPrefab.transform.position = CameraTransform.position;
            agent.SetDestination(player.position);
            isFireflyActive = !isFireflyActive;
        }
        if(!isFireflyActive) return;

        if(agent.remainingDistance < 0.5f && !towardsDestination)
        {
            Debug.Log("Reached Player");
            agent.SetDestination(destination.position);
            towardsDestination = true;
        }
        else if(agent.remainingDistance < 0.5f && towardsDestination)
        {
            Debug.Log("Reached Destination");
            fireFlyPrefab.SetActive(false);
            isFireflyActive = false;
            towardsDestination = false;            
        }
    }
}
