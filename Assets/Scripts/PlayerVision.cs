using UnityEngine;

public class PlayerVision : MonoBehaviour
{
    [Header("Vision")]
    public float viewRadius = 6f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask laserMask;
    public LayerMask obstacleMask;

    public Laser CurrentLaser { get; private set; }
    public Vector3 LastSeenPosition { get; private set; }

    void Update()
    {
        if (CurrentLaser == null)
            ScanForLaser();
        else
            ValidateCurrentLaser();
    }

    void ScanForLaser()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewRadius, laserMask);

        foreach (Collider hit in hits)
        {
            Laser laser = hit.GetComponent<Laser>();
            if (!laser) continue;

            Vector3 dir = (laser.transform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dir) > viewAngle * 0.5f)
                continue;

            float dist = Vector3.Distance(transform.position, laser.transform.position);

            if (Physics.Raycast(transform.position, dir, dist, obstacleMask))
                continue;

            LockLaser(laser);
            break; // FIRST laser only
        }
    }

    void ValidateCurrentLaser()
    {
        Vector3 dir = (CurrentLaser.transform.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, CurrentLaser.transform.position);

        bool lost =
            dist > viewRadius ||
            Vector3.Angle(transform.forward, dir) > viewAngle * 0.5f ||
            Physics.Raycast(transform.position, dir, dist, obstacleMask);

        if (lost)
            ClearLaser();
    }

    void LockLaser(Laser laser)
    {
        CurrentLaser = laser;
        LastSeenPosition = laser.transform.position;
        laser.Freeze();
    }

    public void ClearLaser()
    {
        if (CurrentLaser)
            CurrentLaser.Unfreeze();

        CurrentLaser = null;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 left = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + left * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + right * viewRadius);

        if (CurrentLaser)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, CurrentLaser.transform.position);
        }
    }
#endif
}
