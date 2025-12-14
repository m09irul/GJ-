using UnityEngine;
using System.Collections;

public class Penguin : MonoBehaviour
{
    private Transform cat;
    private Animator animator;
    private VisionCone visionCone;
    private NPCNavAgentHandler agentHandler;

    private bool isSearching = false;
    private float catchDistance = 1f;
    public bool busted;
    private int stars;

    [SerializeField] private Vector3 startPosition;
    private void Start()
    {
        startPosition = transform.position;
        busted = false;
        cat = GameObject.FindGameObjectWithTag("cat").transform;
        //animator = GetComponent<Animator>();
        agentHandler = GetComponent<NPCNavAgentHandler>();
        visionCone = GetComponent<VisionCone>();

        visionCone.OnPlayerDetected += StartChasingPlayer;
        visionCone.OnPlayerLost += StartSearching;
    }

    private void Update()
    {
        if(!busted)
            if(Vector3.Distance(transform.position, cat.position) < catchDistance)
            {
                Debug.Log("Busted");
                Time.timeScale = .1f;
                busted = true;
            }
    }

    private void OnDestroy()
    {
        
        visionCone.OnPlayerDetected -= StartChasingPlayer;
        visionCone.OnPlayerLost -= StartSearching;
    }

    private void StartChasingPlayer(Vector3 targetPosition)
    {
        //stars = GameManager.Instance.Stars;
        stars = 1;
        //animator.Play("Walk");
        Debug.Log("Walk");
        agentHandler.MoveNext(targetPosition);
    }

    private void StartSearching()
    {
        if (!isSearching)
        {
            isSearching = true;
            //animator.Play("Searching");
            Debug.Log("Searching");

            // After the searching animation finishes, transition to idle
            StartCoroutine(WaitForSearchingAnimation());
        }
    }

    private IEnumerator WaitForSearchingAnimation()
    {
        //yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        yield return new WaitForSeconds(.5f);

        // Once the searching animation is finished, play idle animation
        //animator.Play("Idle");
        Debug.Log("idle");

        // Reset searching state
        isSearching = false;
        CoolDown(stars);
    }



    public void CoolDown(int num)
    {
        StopCoroutine(GoBack(num));
        StartCoroutine(GoBack(num));
    }

    IEnumerator GoBack(int num) {
        float multiplier = 2f;
        yield return new WaitForSeconds(num * multiplier);
        agentHandler.MoveNext(startPosition);
    }
}
