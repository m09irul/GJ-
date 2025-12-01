using UnityEngine;

public class DogVisionCone : MonoBehaviour
{
    [Header("Cone Settings")]
    [HideInInspector] public float coneAngle = 60f;
    [HideInInspector] public float coneDistance = 5f;
    [HideInInspector] public int coneSegments = 1;

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

    // height offset from character's head/eyes
    private Vector3 EyeOffset => new Vector3(0, 0.18f, 0.3f);

    private void Awake()
    {
        float coneAngle = 60f;
        float coneDistance = 5f;
        int coneSegments = 1;
        CreateCone();


    }

    private void Update()
    {
        DetectTargets();
        GenerateDynamicConeMesh();
    }

    public void Start()
    {
        float coneAngle = 60f;
        float coneDistance = 5f;
        int coneSegments = 1;
}

    private void CreateCone()
    {
        coneObject = new GameObject("VisionCone");
        coneObject.transform.SetParent(transform);

        // IMPORTANT: cone inherits rotation and faces same direction as owner
        coneObject.transform.localRotation = Quaternion.identity;
        coneObject.transform.localPosition = EyeOffset;
        coneObject.transform.localScale = Vector3.one;

        coneMesh = new Mesh();
        coneObject.AddComponent<MeshFilter>().mesh = coneMesh;

        var mr = coneObject.AddComponent<MeshRenderer>();
        coneMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        // Transparency settings (works with URP)
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

    private void GenerateDynamicConeMesh()
    {
        Vector3[] vertices = new Vector3[coneSegments + 2];
        int[] triangles = new int[coneSegments * 3];

        vertices[0] = Vector3.zero; // cone origin (local space)

        float step = (coneAngle * 2f) / coneSegments;

        // Raycast origin in WORLD space
        Vector3 origin = coneObject.transform.position;

        for (int i = 0; i <= coneSegments; i++)
        {
            float ang = -coneAngle + i * step;
            float rad = Mathf.Deg2Rad * ang;

            // Local direction inside the mesh
            Vector3 dirLocal = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));

            // Convert to world direction for raycast
            Vector3 dirWorld = coneObject.transform.TransformDirection(dirLocal);

            float distance = coneDistance;

            // Raycast to cut cone by walls/objects
            if (Physics.Raycast(origin, dirWorld, out RaycastHit hit, coneDistance))
            {
                if (!hit.collider.CompareTag(targetTag))
                    distance = hit.distance;
            }

            // Mesh vertex (LOCAL space)
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
        Vector3 origin = coneObject.transform.position;

        Collider[] hits = Physics.OverlapSphere(origin, coneDistance);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag(targetTag)) continue;

            Vector3 dir = (hit.transform.position - origin).normalized;

            // Check angle
            if (Vector3.Angle(coneObject.transform.forward, dir) <= coneAngle)
            {
                // Check line of sight
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

