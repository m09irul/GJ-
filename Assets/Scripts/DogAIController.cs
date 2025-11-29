using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DogPatrol))]
public class DogAIController : MonoBehaviour
{
    public GameObject player;
    public PlayerController playerController;
    public DogPatrol patrol;
    public DogVisionCone visionCone;
    public string barkTrigger = "Bark";
    public float cooldownTime = 4f;
    public bool isDamageOverTime = false;
    private bool targetInside = false;   // NEW
    private Transform currentTarget = null;

    private void Start()
    {
        player = GameObject.FindWithTag("cat");
        playerController = player.GetComponent<PlayerController>();
        if (visionCone != null)
            visionCone.OnTargetDetected += HandleDetection;
    }

    private void Update()
    {
        if (targetInside && currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);

            // If the target moves out of detection range → cooldown begins
            if (distance > visionCone.coneDistance ||
                !IsTargetInVisionCone(currentTarget))
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

    private void HandleDetection(Transform target)
    {
        
        // If already tracking, ignore
        if (targetInside){
            if(!isDamageOverTime)
                StartCoroutine(GiveDamage());
            return;
        }

        targetInside = true;
        currentTarget = target;

        patrol.StopPatrol();

        Debug.Log("Dog barks!");

        // Dog looks at target continuously only once
        transform.LookAt(target.position + Vector3.up * 0.5f);

        // 🔥 KEEP RED as long as target is inside vision cone
        visionCone.SetColor(visionCone.detectedColor);
    }

    IEnumerator GiveDamage()
    {
        AudioManager.instance.play("DogBarkingSFX");
        isDamageOverTime = true;
        playerController.ReduceConfidence(10);
        yield return new WaitForSeconds(5f);
        isDamageOverTime = false;
    }

    private void StartCooldown()
    {
        if (!targetInside) return;
        StopCoroutine(GiveDamage());
        isDamageOverTime = false;
        targetInside = false;
        currentTarget = null;

        // 🟡 Start cooldown color
        visionCone.SetColor(visionCone.cooldownColor);

        Invoke(nameof(ResumePatrol), cooldownTime);
    }

    private void ResumePatrol()
    {
        visionCone.SetColor(visionCone.idleColor);

        patrol.StartPatrol();
    }
}
