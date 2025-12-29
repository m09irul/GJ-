using UnityEngine;
using System.Collections;
// using UnityEngine.TestTools.Constraints;

public class Penguin : MonoBehaviour
{
    [SerializeField] public BustedPostProcess bustedPostProcess;
    private Transform cat;
    [SerializeField] private Animator animator;
    private VisionCone visionCone;
    private NPCNavAgentHandler agentHandler;

    private bool isSearching = false;
    private float catchDistance = 1f;
    public bool busted;
    private int stars;

    private bool goingBack = false;

    [SerializeField] private Vector3 startPosition;
    [SerializeField] NPCNavAgentHandler agent;

    [SerializeField] public GameObject BustedGui;
    [SerializeField] private bool isHiding = false;
    [SerializeField] private bool isFoundBeforeHide = false;
    private void Start()
    {
        // isHiding = GameManager.isPlayerHiding;
        agent = GetComponent<NPCNavAgentHandler>();
        startPosition = transform.position;
        busted = false;
        cat = GameObject.FindGameObjectWithTag("cat").transform;
        //animator = GetComponent<Animator>();
        agentHandler = GetComponent<NPCNavAgentHandler>();
        visionCone = GetComponent<VisionCone>();

        visionCone.OnPlayerDetected += StartChasingPlayer;
        // visionCone.OnPlayerLost += StartSearching;
    }

    private void Update()
    {
        if (goingBack && agent.getRemainingDistance() <= agent.getStopDistance())
        {
            goingBack = false;
            animator.Play("idle");
        }
        if(!busted && !isHidable())
            if(Vector3.Distance(transform.position, cat.position) < catchDistance)
            {
                // GameManager.Instance.Busted();
                Busted();
                busted = true;
            }

        if (!afterStart) return;
        if(agent.getRemainingDistance() <= agent.getStopDistance())
        {
            afterStart = false;
            StartSearching();
        }
    }

    void Busted()
    {
        Time.timeScale = .02f;
        bustedPostProcess.PlayBustedEffect();

        StartCoroutine(panelGUI());
    }

    IEnumerator panelGUI()
    {
        yield return new WaitForSeconds(.02f);
        BustedGui.SetActive(true);
        Time.timeScale = 0f;
    }

    private void OnDestroy()
    {
        
        visionCone.OnPlayerDetected -= StartChasingPlayer;
        // visionCone.OnPlayerLost -= StartSearching;
    }
    private bool afterStart = false;
    private void StartChasingPlayer(Vector3 targetPosition)
    {
        // isHiding = GameManager.isPlayerHiding;
        if(isHidable())
            return;

        isFoundBeforeHide = true;
        //stars = GameManager.Instance.Stars;
        stars = 1;
        animator.Play("run");
        Debug.Log("Running");
        agentHandler.MoveNext(targetPosition);
        afterStart = true;
    }

    private void StartSearching()
    {
        isFoundBeforeHide = false;
        if (!isSearching)
        {
            isSearching = true;
            animator.Play("walk");
            Debug.Log("Searching");

            CoolDown(stars);
        }
    }

    public void CoolDown(int num)
    {
        StartCoroutine(GoBack(num));
    }

    IEnumerator GoBack(int num) {
        float multiplier = 2f;
        yield return new WaitForSeconds(num * multiplier);
        animator.Play("idle");
        Debug.Log("idle");
        isSearching = false;
        yield return new WaitForSeconds(1f);
        animator.Play("run");
        goingBack = true;
        agentHandler.MoveNext(startPosition);
    }

    public bool isHidable()
    {
        return isHiding && !isFoundBeforeHide;
    }
}
