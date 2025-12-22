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

    // Better detection height (UNCHANGED)
    private Vector3 EyeOffset => new Vector3(0, 0.18f, 0.3f);

    // INTERNAL ONLY (does NOT affect other scripts)
    private const float CONE_THICKNESS = 1.2f;

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
        coneObject.transform.localRotation = Quaternion.identity;
        coneObject.transform.localPosition = EyeOffset;
        coneObject.transform.localScale = Vector3.one;

        coneMesh = new Mesh();
        coneObject.AddComponent<MeshFilter>().mesh = coneMesh;

        var mr = coneObject.AddComponent<MeshRenderer>();
        coneMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        coneMaterial.SetFloat("_Surface", 1);
        coneMaterial.SetFloat("_Blend", 1);
        coneMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        coneMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        coneMaterial.SetFloat("_ZWrite", 0);
        coneMaterial.renderQueue = 3000;

        coneMaterial.color = idleColor;
        mr.material = coneMaterial;

        GenerateDynamicConeMesh();
    }

    /// <summary>
    /// SAME METHOD NAME — now generates a 2.5D extruded cone
    /// </summary>
    private void GenerateDynamicConeMesh()
{
    int ringCount = coneSegments + 2;

    // Top ring + base ring
    Vector3[] vertices = new Vector3[ringCount * 2];

    // Top + left + right faces only
    int[] triangles = new int[(coneSegments * 6) + 12];

    float height = 1.2f; // internal visual height (SAFE, not public)
    float step = (coneAngle * 2f) / coneSegments;

    Vector3 origin = coneObject.transform.position;

    // SAME origin for all faces
    vertices[0] = Vector3.zero;
    vertices[ringCount] = Vector3.up * height;

    for (int i = 0; i <= coneSegments; i++)
    {
        float ang = -coneAngle + i * step;
        float rad = Mathf.Deg2Rad * ang;

        Vector3 dirLocal = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
        Vector3 dirWorld = coneObject.transform.TransformDirection(dirLocal);

        float distance = coneDistance;

        if (Physics.SphereCast(origin, 0.05f, dirWorld, out RaycastHit hit, coneDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.CompareTag(targetTag))
                distance = Mathf.Max(0.05f, hit.distance - 0.02f);
        }

        Vector3 point = dirLocal * distance;

        // Base surface
        vertices[i + 1] = point;

        // Raised surface (same start)
        vertices[i + 1 + ringCount] = point + Vector3.up * height;
    }

    int t = 0;

    // --------------------
    // TOP FACE
    // --------------------
    for (int i = 0; i < coneSegments; i++)
    {
        triangles[t++] = ringCount;
        triangles[t++] = ringCount + i + 1;
        triangles[t++] = ringCount + i + 2;
    }

    // --------------------
    // LEFT SIDE FACE
    // --------------------
    triangles[t++] = 0;
    triangles[t++] = 1;
    triangles[t++] = ringCount + 1;

    triangles[t++] = 0;
    triangles[t++] = ringCount + 1;
    triangles[t++] = ringCount;

    // --------------------
    // RIGHT SIDE FACE
    // --------------------
    int right = coneSegments + 1;

    triangles[t++] = 0;
    triangles[t++] = ringCount;
    triangles[t++] = ringCount + right;

    triangles[t++] = 0;
    triangles[t++] = ringCount + right;
    triangles[t++] = right;

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

            if (Vector3.Angle(coneObject.transform.forward, dir) <= coneAngle)
            {
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
