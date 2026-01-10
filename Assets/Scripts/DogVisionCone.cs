using UnityEngine;
using System;
public class DogVisionCone : MonoBehaviour
{
    [Header("Cone Settings")]
    [SerializeField] private float coneAngle = 45f;
    [SerializeField] private float coneDistance = 5f;
    [SerializeField] private int coneSegments = 40;

    [Header("Detection")]
    [SerializeField] private string targetTag = "cat";
    [SerializeField] private LayerMask visionMask = ~0;
    [SerializeField] private LayerMask targetMask;   // Cat
    [SerializeField] private LayerMask obstacleMask; // Walls, crates, etc

    [Header("Colors")]
    public Color idleColor = new(0f, 1f, 0f, 0.15f);
    public Color detectedColor = new(1f, 0f, 0f, 0.2f);
    public Color cooldownColor = new(1f, 0.92f, 0.016f, 0.2f);

    public event Action<Transform> OnTargetDetected;
    public event Action OnTargetLost;

    private GameObject coneObject;
    private Mesh coneMesh;
    private MeshCollider coneCollider;
    private Material coneMaterial;

    private bool targetVisible;
    private Transform detectedTarget;

    private static readonly Vector3 EyeOffset = new(0f, 0.18f, 0.3f);

    /* ======================
     * UNITY
     * ====================== */

    private void Awake()
    {
        CreateCone();
    }

    private void Update()
    {
        bool visibleThisFrame = PerformVisionCheck();
        UpdateDetectionState(visibleThisFrame);
        GenerateConeMesh();
    }

    /* ======================
     * DETECTION
     * ====================== */
    public void SetIdleColor() => SetColor(idleColor);
    public void SetCooldownColor() => SetColor(cooldownColor);

    private bool PerformVisionCheck()
    {
        Vector3 origin = coneObject.transform.position;
        Vector3 right = coneObject.transform.right;

        float step = (coneAngle * 2f) / coneSegments;
        float sphereRadius = 0.12f;
        float sideOffset = 0.18f;

        for (int i = 0; i <= coneSegments; i++)
        {
            float angle = -coneAngle + step * i;

            Vector3 dirLocal = new(
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0f,
                Mathf.Cos(angle * Mathf.Deg2Rad)
            );

            Vector3 dirWorld = coneObject.transform.TransformDirection(dirLocal);

            Vector3[] origins =
            {
            origin,
            origin + right * sideOffset,
            origin - right * sideOffset
        };

            foreach (var o in origins)
            {
                // STEP 1: Detect CAT only
                if (!Physics.SphereCast(
                        o,
                        sphereRadius,
                        dirWorld,
                        out RaycastHit catHit,
                        coneDistance,
                        targetMask,
                        QueryTriggerInteraction.Ignore))
                    continue;

                // STEP 2: Check if something blocks vision
                float distToCat = catHit.distance;

                if (Physics.Raycast(
                        o,
                        dirWorld,
                        out RaycastHit blockHit,
                        distToCat,
                        obstacleMask,
                        QueryTriggerInteraction.Collide))
                {
                    // Obstacle blocks view → ignore
                    continue;
                }

                detectedTarget = catHit.transform;
                return true;
            }
        }

        return false;
    }

    private void UpdateDetectionState(bool visibleThisFrame)
    {
        if (visibleThisFrame && !targetVisible)
        {
            targetVisible = true;
            SetColor(detectedColor);
            OnTargetDetected?.Invoke(detectedTarget);
        }
        else if (!visibleThisFrame && targetVisible)
        {
            targetVisible = false;
            detectedTarget = null;
            SetColor(cooldownColor);
            OnTargetLost?.Invoke();
        }
    }

    /* ======================
     * MESH
     * ====================== */

    private void CreateCone()
    {
        coneObject = new GameObject("VisionCone");
        coneObject.transform.SetParent(transform);
        coneObject.transform.localPosition = EyeOffset;
        coneObject.transform.localRotation = Quaternion.identity;

        coneMesh = new Mesh();
        coneMesh.name = "VisionConeMesh";

        coneObject.AddComponent<MeshFilter>().mesh = coneMesh;

        var renderer = coneObject.AddComponent<MeshRenderer>();
        coneMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        coneMaterial.SetFloat("_Surface", 1);
        coneMaterial.SetFloat("_ZWrite", 0);
        coneMaterial.renderQueue = 3000;
        coneMaterial.color = idleColor;
        renderer.material = coneMaterial;

        coneCollider = coneObject.AddComponent<MeshCollider>();
        coneCollider.convex = true;
        coneCollider.isTrigger = true;

        var rb = coneObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void GenerateConeMesh()
    {
        int ringCount = coneSegments + 2; // center + arc
        Vector3[] vertices = new Vector3[ringCount * 2];
        int[] triangles = new int[(coneSegments * 12) + 12];

        float height = 1f;
        float step = (coneAngle * 2f) / coneSegments;

        // Bottom & top center
        vertices[0] = Vector3.zero;
        vertices[ringCount] = Vector3.up * height;

        for (int i = 0; i <= coneSegments; i++)
        {
            float ang = -coneAngle + i * step;
            float rad = ang * Mathf.Deg2Rad;

            float visibleDist = GetVisibleDistance(ang);

            Vector3 basePoint = new(
                Mathf.Sin(rad) * visibleDist,
                0f,
                Mathf.Cos(rad) * visibleDist
            );

            vertices[i + 1] = basePoint;
            vertices[i + 1 + ringCount] = basePoint + Vector3.up * height;
        }

        int t = 0;

        // Top face
        for (int i = 0; i < coneSegments; i++)
        {
            triangles[t++] = ringCount;
            triangles[t++] = ringCount + i + 1;
            triangles[t++] = ringCount + i + 2;
        }

        // Side faces
        for (int i = 0; i < coneSegments; i++)
        {
            int bA = i + 1;
            int bB = i + 2;
            int tA = bA + ringCount;
            int tB = bB + ringCount;

            triangles[t++] = bA;
            triangles[t++] = tB;
            triangles[t++] = tA;

            triangles[t++] = bA;
            triangles[t++] = bB;
            triangles[t++] = tB;
        }

        // Left cap
        triangles[t++] = 0;
        triangles[t++] = 1;
        triangles[t++] = ringCount + 1;

        triangles[t++] = 0;
        triangles[t++] = ringCount + 1;
        triangles[t++] = ringCount;

        // Right cap
        int rightCap = coneSegments + 1;
        triangles[t++] = 0;
        triangles[t++] = ringCount;
        triangles[t++] = ringCount + rightCap;

        triangles[t++] = 0;
        triangles[t++] = ringCount + rightCap;
        triangles[t++] = rightCap;

        coneMesh.Clear();
        coneMesh.vertices = vertices;
        coneMesh.triangles = triangles;
        coneMesh.RecalculateNormals();
        coneMesh.RecalculateBounds();

        coneCollider.sharedMesh = null;
        coneCollider.sharedMesh = coneMesh;
    }
    private float GetVisibleDistance(float angle)
    {
        Vector3 dirLocal = new(
            Mathf.Sin(angle * Mathf.Deg2Rad),
            0f,
            Mathf.Cos(angle * Mathf.Deg2Rad)
        );

        Vector3 dirWorld = coneObject.transform.TransformDirection(dirLocal);
        Vector3 origin = coneObject.transform.position;

        if (Physics.Raycast(origin, dirWorld, out RaycastHit hit,
                coneDistance, visionMask, QueryTriggerInteraction.Collide))
        {
            return hit.distance;
        }

        return coneDistance;
    }
    public void OnMovementStarted()
    {
        if (!targetVisible && detectedTarget == null)
        {
            SetColor(idleColor);
        }
    }

    /* ======================
     * VISUAL
     * ====================== */

    public void SetColor(Color color)
    {
        if (coneMaterial)
            coneMaterial.color = color;
    }
}
