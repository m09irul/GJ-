using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DogPatrol))]
[RequireComponent(typeof(DogVisionCone))]
public class DogAIController : MonoBehaviour
{
    private DogPatrol patrol;
    private DogVisionCone visionCone;
    private NPCNavAgentHandler agentHandler;
    private PlayerController playerController;

    [SerializeField] private float cooldownTime = 4f;

    private Coroutine damageRoutine;
    private Coroutine cooldownRoutine;
    private bool targetDetected;
    [Space]
    private Vector3 stimulusTarget;
    private StimulusType currentStimulus;
    private bool respondingToStimulus;
    [Space]
    [SerializeField] private float hearingRange = 20f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask groundMask;

    private void Awake()
    {
        patrol = GetComponent<DogPatrol>();
        visionCone = GetComponent<DogVisionCone>();
        agentHandler = GetComponent<NPCNavAgentHandler>();

        GameObject player = GameObject.FindWithTag("cat");
        if (player)
            playerController = player.GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        visionCone.OnTargetDetected += HandleTargetDetected;
        visionCone.OnTargetLost += HandleTargetLost;
        AIStimulusDispatcher.OnStimulusEmitted += HandleStimulus;

    }


    private void OnDisable()
    {
        visionCone.OnTargetDetected -= HandleTargetDetected;
        visionCone.OnTargetLost -= HandleTargetLost;
        AIStimulusDispatcher.OnStimulusEmitted -= HandleStimulus;

    }
    private void HandleStimulus(AIStimulus stimulus)
    {
        // Ignore if chasing player
        if (targetDetected)
            return;

        // Dog reacts only to Food & Stone
        if (stimulus.Type != StimulusType.Food &&
            stimulus.Type != StimulusType.Stone)
            return;

        // 1️⃣ Range check
        float dist = Vector3.Distance(transform.position, stimulus.Position);
        if (dist > hearingRange)
            return;

        // 2️⃣ Line of sight check
        if (!HasLineOfSight(stimulus.Position))
            return;

        // 3️⃣ Accept stimulus
        RespondToStimulus(stimulus);
    }
    private bool HasLineOfSight(Vector3 stimulusPos)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 target = stimulusPos + Vector3.up * 0.1f;

        Vector3 dir = target - origin;
        float dist = dir.magnitude;
        dir.Normalize();

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, obstacleMask))
        {
            Debug.DrawLine(origin, hit.point, Color.red, 1f);
            return false; // blocked
        }

        Debug.DrawLine(origin, target, Color.green, 1f);
        return true;
    }

    private void RespondToStimulus(AIStimulus stimulus)
    {
        respondingToStimulus = true;
        currentStimulus = stimulus.Type;
        stimulusTarget = stimulus.Position;

        patrol.StopPatrol();
        StopAllAIActivity();

        agentHandler.GoToTemporaryTarget(stimulusTarget, StartStimulusRoutine);

    }
    void StartStimulusRoutine()
    {
        StartCoroutine(StimulusRoutine());
    }
    private IEnumerator StimulusRoutine()
    {
        Debug.Log("stim");
        while (!agentHandler.HasReachedDestination())
            yield return null;


        if (currentStimulus == StimulusType.Food)
            yield return new WaitForSeconds(7f); // eat
        else
            yield return new WaitForSeconds(2f); // investigate stone

        respondingToStimulus = false;

        patrol.StartPatrol();
        visionCone.OnMovementStarted();
    }

    private void StopAllAIActivity()
    {
        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
            damageRoutine = null;
        }

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
        }
    }
    private void HandleTargetDetected(Transform target)
    {
        if (targetDetected)
            return;

        targetDetected = true;
        GameManager.Instance.isPlayerDetected = true;
        agentHandler.MoveTo(target.position);
        patrol.StopPatrol();

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
        }

        damageRoutine = StartCoroutine(DamageRoutine());
    }

    private void HandleTargetLost()
    {
        if (!targetDetected)
            return;

        targetDetected = false;
        GameManager.Instance.isPlayerDetected = false;
        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
            damageRoutine = null;
        }

        if (cooldownRoutine == null)
            cooldownRoutine = StartCoroutine(CooldownRoutine());
    }

    private IEnumerator DamageRoutine()
    {
        while (targetDetected)
        {
            if (playerController)
                playerController.ReduceConfidence(2);

            if (!AudioManager.instance.GetAudio("DogBarkingSFX").source.isPlaying)
                AudioManager.instance.play("DogBarkingSFX");

            yield return new WaitForSeconds(3f);
        }
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldownTime);
        patrol.StartPatrol();
        visionCone.OnMovementStarted();

        cooldownRoutine = null;
    }
}
