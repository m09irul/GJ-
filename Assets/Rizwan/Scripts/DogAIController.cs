using UnityEngine;

[RequireComponent(typeof(DogPatrol))]
public class DogAIController : MonoBehaviour
{
    public DogPatrol patrol;
    public DogVisionCone visionCone;
    public string barkTrigger = "Bark";
    public float cooldownTime = 4f;

    private bool targetInside = false;   // NEW
    private Transform currentTarget = null;

    private void Start()
    {
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
        if (targetInside) return;

        targetInside = true;
        currentTarget = target;

        patrol.StopPatrol();

        Debug.Log("Dog barks!");

        // Dog looks at target continuously only once
        transform.LookAt(target.position + Vector3.up * 0.5f);

        // 🔥 KEEP RED as long as target is inside vision cone
        visionCone.SetColor(visionCone.detectedColor);
    }

    private void StartCooldown()
    {
        if (!targetInside) return;

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
