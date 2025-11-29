using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class NavMeshGPS : MonoBehaviour
{
    public Transform hub;
    public Transform destination;
    public float updateInterval = 0.5f;

    private LineRenderer lr;
    private NavMeshPath path;
    private float timer;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.widthMultiplier = 0.5f;

        path = new NavMeshPath();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdatePath();
        }

        if (lr.material != null)
{
        Vector2 offset = lr.material.mainTextureOffset;
        offset.x += Time.deltaTime * 0.5f; // scroll speed
        lr.material.mainTextureOffset = offset;
}
    }

    void UpdatePath()
    {
        if (hub == null || destination == null) return;

        // NavMesh calculates path automatically
        if (NavMesh.CalculatePath(hub.position, destination.position, NavMesh.AllAreas, path))
        {
            lr.positionCount = path.corners.Length;
            lr.SetPositions(path.corners);
        }
    }
}
