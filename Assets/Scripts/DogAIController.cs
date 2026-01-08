using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DogPatrol))]
public class DogAIController : MonoBehaviour
{
    public NPCNavAgentHandler agentHandler;
    public GameObject player;
    public PlayerController playerController;
    public DogPatrol patrol;
    public DogVisionCone visionCone;
    public string barkTrigger = "Bark";
    public float cooldownTime = 4f;
    public bool isDamageOverTime = false;

    private bool targetInside = false;
    private Transform currentTarget = null;

    [SerializeField] private bool isHiding = false;
    [SerializeField] private bool isFoundBeforeHide = false;
    private void Start()
    {
        // PlayerPrefs.SetInt(AllStringConstant.FARMING_ITEM, 2);
        // isHiding = GameManager.isPlayerHiding;
        isFoundBeforeHide = false;
        agentHandler = GetComponent<NPCNavAgentHandler>();
        player = GameObject.FindWithTag("cat");
        playerController = player.GetComponent<PlayerController>();
        visionCone = GetComponent<DogVisionCone>();
        patrol = GetComponent<DogPatrol>();

        // Subscribe to OnTargetDetected if you still want raycast detection as backup
        if (visionCone != null)
            visionCone.OnTargetDetected += HandleDetection;
    }

    public bool isHidable()
    {
        return isHiding && !isFoundBeforeHide;
    }

    private void Update()
    {
        // Optional: move toward target while inside cone
        if (targetInside && currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);

            // Start cooldown if target leaves cone distance
            if (distance > visionCone.coneDistance || !IsTargetInVisionCone(currentTarget))
            {
                StartCooldown();
            }
        }
    }

    private bool IsTargetInVisionCone(Transform target)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        return Vector3.Angle(transform.forward, dir) <= visionCone.coneAngle;
    }

    // // -----------------------------
    // // TRIGGER EVENT HANDLERS
    // // -----------------------------
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("cat") && !targetInside)
    //     {
    //         HandleDetection(other.transform);
    //     }
    // }

    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("cat"))
    //     {
    //         StartCooldown();
    //     }
    // }

    // -----------------------------
    // DETECTION LOGIC
    // -----------------------------
    private void HandleDetection(Transform target)
    {
        // isHiding = GameManager.isPlayerHiding;
        if(isHiding && !isFoundBeforeHide)
            return;
        
        isFoundBeforeHide = true;
        resumeCalled = false;
        if (targetInside) return; // already tracking

        targetInside = true;
        currentTarget = target;

        // Move dog toward player / trigger event
        agentHandler.isEventTriggered = true;
        Debug.Log("Calling From here");
        patrol.StopPatrol();
        agentHandler.MoveNext(transform.position);
        patrol.setAnimation("Dog_001_idle");

        // Look at player
        transform.LookAt(target.position + Vector3.up * 0.5f);

        // Set vision cone color to RED
        visionCone.SetColor(visionCone.detectedColor);

        // Start damage coroutine if needed
        if (!isDamageOverTime)
            StartCoroutine(GiveDamage());
    }

    IEnumerator GiveDamage()
    {
        isDamageOverTime = true;
        // AudioManager
        if (playerController != null)
            playerController.ReduceConfidence(1);

        Debug.Log("Dog barks!");

        if (!AudioManager.instance.sounds[4].source.isPlaying)
            AudioManager.instance.play("DogBarkingSFX");

        // Damage every 5 seconds while inside cone
        yield return new WaitForSeconds(1f);

        isDamageOverTime = false;

        // Repeat damage if target still inside
        if (targetInside)
            StartCoroutine(GiveDamage());
    }

    private void StartCooldown()
    {
        if (!targetInside) return;
        isFoundBeforeHide = false;
        StopAllCoroutines();
        isDamageOverTime = false;
        targetInside = false;
        currentTarget = null;

        // Change cone color to cooldown
        visionCone.SetColor(visionCone.cooldownColor);

        // Resume patrol after cooldown
        Invoke(nameof(ResumePatrol), cooldownTime);
    }

    bool resumeCalled = false;
    private void ResumePatrol()
    {
        if(resumeCalled)
            return;
        resumeCalled = true;
        agentHandler.isEventTriggered = false;
        visionCone.SetColor(visionCone.idleColor);
        agentHandler.GoBackToPatrol();
        patrol.StartPatrol();
    }
}
