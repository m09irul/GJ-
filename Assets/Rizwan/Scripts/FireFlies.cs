using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class FireFlies : MonoBehaviour
{
    public GameObject fireFlyBase;
    [HideInInspector]private Transform CameraTransform;
    [HideInInspector]private Transform player;
    private NavMeshAgent agent;
    public Vector3 destination;
    public bool isFireflyActive = false;
    private bool towardsDestination = false;
    [SerializeField] private bool isActivated = false;

    [SerializeField] GameObject panel;
    // Start is called before the first frame update
    void Start()
    {
        agent = fireFlyBase.GetComponent<NavMeshAgent>();
        CameraTransform = GameObject.FindWithTag("MainCamera").transform;
        player = GameObject.FindWithTag("cat").transform;
        fireFlyBase.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isActivated)
        {
            StartCoroutine(FireFliesRoutine());
        }
        if(!isFireflyActive) return;

        if(Vector3.Distance(fireFlyBase.transform.position,player.position) < 1f && !towardsDestination)
        {
            Debug.Log("Reached Player");
            agent.SetDestination(destination);
            towardsDestination = true;
        }
        else if(Vector3.Distance(fireFlyBase.transform.position,destination) < 1f && towardsDestination)
        {
            Debug.Log("Reached Destination");
            fireFlyBase.SetActive(false);
            isFireflyActive = false;
            towardsDestination = false;            
        }
    }

    void ActivateFireFly()
    {
        destination = GameManager.Instance.fireflyDestination;
        fireFlyBase.SetActive(true);
        fireFlyBase.transform.position = CameraTransform.position + CameraTransform.forward * 2f;
        agent.SetDestination(player.position);
        isFireflyActive = true;
    }


    public void startFireFly()
    {
        panel.SetActive(false);
        isActivated = true;
    }

    IEnumerator FireFliesRoutine()
    {
        isActivated = false;
        Debug.Log("Firefly Activated");
        ActivateFireFly();
        yield return new WaitForSeconds(20f);
        isActivated = true;
    }
}
