using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisionCone : MonoBehaviour
{
    [Header("Cone Settings")]
    public float coneAngle = 45f;      // Half-angle in degrees
    public float coneDistance = 5f;    // Max distance
    public int coneSegments = 20;      // Number of slices
    public float height = 1f;          // Vertical height of cone
    public LayerMask obstacleMask;     // Layers that cut the cone

    private Mesh coneMesh;

    void Awake()
    {
        coneMesh = new Mesh();
        coneMesh.name = "VisionConeMesh";

        GetComponent<MeshFilter>().mesh = coneMesh;

        // Optional: simple transparent material
        var renderer = GetComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        renderer.material.color = new Color(0f, 1f, 0f, 0.2f);
    }

    void Update()
    {
        GenerateConeMesh();
    }

    void GenerateConeMesh()
    {
        int ringCount = coneSegments + 2;
        Vector3[] vertices = new Vector3[ringCount * 2];
        int[] triangles = new int[(coneSegments * 12) + 12];

        float step = (coneAngle * 2f) / coneSegments;

        // Bottom & top center
        vertices[0] = Vector3.zero;         // bottom center
        vertices[ringCount] = Vector3.up * height; // top center

        // Generate vertices per segment with obstacle cut
        for (int i = 0; i <= coneSegments; i++)
        {
            float ang = -coneAngle + i * step;
            float rad = ang * Mathf.Deg2Rad;

            // Direction in local XZ plane
            Vector3 dirLocal = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
            Vector3 dirWorld = transform.TransformDirection(dirLocal);
            Vector3 origin = transform.position;

            float visibleDist = GetVisibleDistance(dirWorld, origin);

            Vector3 basePoint = dirLocal * visibleDist;
            vertices[i + 1] = basePoint;                // bottom ring
            vertices[i + 1 + ringCount] = basePoint + Vector3.up * height; // top ring
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

        // Caps
        triangles[t++] = 0;
        triangles[t++] = 1;
        triangles[t++] = ringCount + 1;

        triangles[t++] = 0;
        triangles[t++] = ringCount + 1;
        triangles[t++] = ringCount;

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
    }

    float GetVisibleDistance(Vector3 dirWorld, Vector3 origin)
    {
        if (Physics.Raycast(origin, dirWorld, out RaycastHit hit, coneDistance, obstacleMask))
        {
            return hit.distance;
        }
        return coneDistance;
    }
}