using System.Diagnostics;
using UnityEngine;

public class DogVisionCone : MonoBehaviour
{
    [Header("Cone Settings")]
    public float coneAngle = 45f;
    public float coneDistance = 5f;
    public int coneSegments = 20;

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
    private Vector3 EyeOffset => new Vector3(0, 0.25f, 0.3f);

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
        coneObject.transform.localPosition = EyeOffset;

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
        Vector3 origin = transform.position + transform.TransformDirection(EyeOffset);

        for (int i = 0; i <= coneSegments; i++)
        {
            float ang = -coneAngle + i * step;
            float rad = Mathf.Deg2Rad * ang;

            Vector3 dirLocal = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
            Vector3 dirWorld = transform.TransformDirection(dirLocal);

            float distance = coneDistance;

            // FIX: better hit detection for small objects
            if (Physics.Raycast(origin, dirWorld, out RaycastHit hit, coneDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                // only cut if NOT the target
                if (!hit.collider.CompareTag(targetTag))
                    distance = hit.distance;
            }

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
    }

    private void DetectTargets()
    {
        Vector3 origin = transform.position + transform.TransformDirection(EyeOffset);

        Collider[] hits = Physics.OverlapSphere(origin, coneDistance);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag(targetTag)) continue;

            Vector3 dir = (hit.transform.position - origin).normalized;

            if (Vector3.Angle(transform.forward, dir) <= coneAngle)
            {
                if (Physics.Raycast(origin, dir, out RaycastHit info, coneDistance))
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
