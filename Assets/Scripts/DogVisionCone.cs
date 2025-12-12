using System.Diagnostics;
using UnityEngine;

public class DogVisionCone : MonoBehaviour
{
    [Header("Cone Settings")]
    public float coneAngle = 45f;
    public float coneDistance = 5f;
    public int coneSegments = 50; // Increased for smoother cutoff

    [Header("Detection")]
    public string targetTag = "cat";

    [Header("Colors (lower opacity for transparency)")]
    public Color idleColor = new Color(0f, 1f, 0f, 0.15f);
    public Color detectedColor = new Color(1f, 0f, 0f, 0.20f);
    public Color cooldownColor = new Color(1f, 0.92f, 0.016f, 0.20f);

    private GameObject coneObject;
    private Mesh coneMesh;
    private Material coneMaterial;

    public delegate void TargetDetectedHandler(Transform target);
    public event TargetDetectedHandler OnTargetDetected;

    // Better detection height
    private Vector3 EyeOffset => new Vector3(0, 0.18f, 0.3f);

    private void Awake()
    {
        CreateCone();
    }

    private void Update()
    {
        DetectTargets();
        GenerateDynamicConeMesh();
    }

    private void CreateCone()
    {
        coneObject = new GameObject("VisionCone");
        coneObject.transform.SetParent(transform);

        // IMPORTANT: Inherit parent's rotation and position
        coneObject.transform.localRotation = Quaternion.identity;
        coneObject.transform.localPosition = EyeOffset;
        coneObject.transform.localScale = Vector3.one;

        coneMesh = new Mesh();
        coneObject.AddComponent<MeshFilter>().mesh = coneMesh;

        var mr = coneObject.AddComponent<MeshRenderer>();
        coneMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        // THIS makes transparency work properly
        coneMaterial.SetFloat("_Surface", 1);     // Transparent
        coneMaterial.SetFloat("_Blend", 1);       // Alpha blend
        coneMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        coneMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        coneMaterial.SetFloat("_ZWrite", 0);
        coneMaterial.renderQueue = 3000;

        coneMaterial.color = idleColor;
        mr.material = coneMaterial;

        GenerateDynamicConeMesh();
    }

    private void GenerateDynamicConeMesh()
    {
        Vector3[] vertices = new Vector3[coneSegments + 2];
        int[] triangles = new int[coneSegments * 3];

        vertices[0] = Vector3.zero;

        float step = (coneAngle * 2f) / coneSegments;

        // Raycast origin in WORLD space
        Vector3 origin = coneObject.transform.position;

        for (int i = 0; i <= coneSegments; i++)
        {
            float ang = -coneAngle + i * step;
            float rad = Mathf.Deg2Rad * ang;

            // Local direction for the mesh
            Vector3 dirLocal = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));

            // World direction for raycasting (uses cone's world rotation)
            Vector3 dirWorld = coneObject.transform.TransformDirection(dirLocal);

            float distance = coneDistance;

            // Use SphereCast for more accurate collision detection
            if (Physics.SphereCast(origin, 0.05f, dirWorld, out RaycastHit hit, coneDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                // Cut the cone ONLY if it's NOT the target (cat)
                if (!hit.collider.CompareTag(targetTag))
                {
                    distance = Mathf.Max(0.05f, hit.distance - 0.02f); // Precise offset
                }
            }

            // Set vertex in LOCAL space
            vertices[i + 1] = dirLocal * distance;

            if (i < coneSegments)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        coneMesh.Clear();
        coneMesh.vertices = vertices;
        coneMesh.triangles = triangles;
        coneMesh.RecalculateNormals();
        coneMesh.RecalculateBounds();
    }

    private void DetectTargets()
    {
        Vector3 origin = coneObject.transform.position;

        Collider[] hits = Physics.OverlapSphere(origin, coneDistance);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag(targetTag)) continue;

            Vector3 dir = (hit.transform.position - origin).normalized;

            // Check if target is within cone angle (uses cone's forward direction)
            if (Vector3.Angle(coneObject.transform.forward, dir) <= coneAngle)
            {
                // Check line of sight
                if (Physics.Raycast(origin, dir, out RaycastHit info, coneDistance, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (info.collider.CompareTag(targetTag))
                    {
                        OnTargetDetected?.Invoke(info.transform);
                        return;
                    }
                }
            }
        }
    }

    public void SetColor(Color color)
    {
        if (coneMaterial != null)
            coneMaterial.color = color;
    }
}