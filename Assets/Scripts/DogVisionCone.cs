using JetBrains.Annotations;
using System.Diagnostics;
using UnityEngine;

public class DogVisionCone : MonoBehaviour
{
    [Header("Cone Settings")]
    public float coneAngle = 45f;
    public float coneDistance = 5f;
    public float detectDistance;
    public int coneSegments = 50;

    [Header("Detection")]
    public string targetTag = "cat";

    [Header("Colors (lower opacity for transparency)")]
    public Color idleColor = new Color(0f, 1f, 0f, 0.15f);
    public Color detectedColor = new Color(1f, 0f, 0f, 0.20f);
    public Color cooldownColor = new Color(1f, 0.92f, 0.016f, 0.20f);

    private GameObject coneObject;
    private Mesh coneMesh;
    private Material coneMaterial;
    private MeshCollider coneCollider;

    public delegate void TargetDetectedHandler(Transform target);
    public event TargetDetectedHandler OnTargetDetected;

    private Transform currentTarget;

    [SerializeField] private Vector3 EyeOffset => new Vector3(0, 0.18f, 0.3f);
    private DogAIController dogAIController;
    private void Awake()
    {
        CreateCone();
    }
    private void Start()
    {
        detectDistance = coneDistance;
        dogAIController = GetComponent<DogAIController>();
    }
    private void Update()
    {
        GenerateDynamicConeMesh();
    }

    public void DestroyCone()
    {
        if (coneObject != null)
        {
            Destroy(coneObject);
            coneObject = null;
            coneMesh = null;
            coneCollider = null;
            coneMaterial = null;
        }
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

        // ---------- COLLIDER + TRIGGER ----------
        coneCollider = coneObject.AddComponent<MeshCollider>();
        coneCollider.convex = true;
        coneCollider.isTrigger = true;

        Rigidbody rb = coneObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        GenerateDynamicConeMesh();
    }
    [SerializeField] private float rad = 0.05f;
    private void GenerateDynamicConeMesh()
    {
        int ringCount = coneSegments + 2;
        Vector3[] vertices = new Vector3[ringCount * 2];
        int[] triangles = new int[(coneSegments * 12) + 12];

        float height = 1f;
        float step = (coneAngle * 2f) / coneSegments;

        // World-space eye origin
        Vector3 origin = coneObject.transform.position;

        // Center points
        vertices[0] = Vector3.zero;
        vertices[ringCount] = Vector3.up * height;

        // ---- OCCLUSION SETTINGS ----
        float sphereRadius = 0.12f;        // thickness of vision ray
        float sideOffset = 0.18f;           // width sampling
        float surfaceOffset = 0.05f;        // prevent clipping
        LayerMask visionMask = ~0;          // preferably set explicitly

        Vector3 right = coneObject.transform.right;

        for (int i = 0; i <= coneSegments; i++)
        {
            float ang = -coneAngle + i * step;
            float rad = Mathf.Deg2Rad * ang;

            Vector3 dirLocal = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
            Vector3 dirWorld = coneObject.transform.TransformDirection(dirLocal).normalized;

            float closestHit = coneDistance;

            // Multi-sample to cover full collider width
            Vector3[] origins =
            {
            origin,
            origin + right * sideOffset,
            origin - right * sideOffset
        };

            foreach (Vector3 castOrigin in origins)
            {
                if (Physics.SphereCast(
                    castOrigin,
                    sphereRadius,
                    dirWorld,
                    out RaycastHit hit,
                    coneDistance,
                    visionMask,
                    QueryTriggerInteraction.Ignore))
                {
                    if (!hit.collider.CompareTag(targetTag))
                    {
                        closestHit = Mathf.Min(closestHit, hit.distance);
                    }
                    else
                    {
                        DetectionTasks(); // target visible
                    }
                }
            }

            float finalDistance = Mathf.Clamp(
                closestHit - surfaceOffset,
                0.05f,
                coneDistance
            );

            Vector3 point = dirLocal * finalDistance;

            vertices[i + 1] = point;
            vertices[i + 1 + ringCount] = point + Vector3.up * height;
        }

        // ---------- TRIANGLES ----------
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

        // ---------- APPLY ----------
        coneMesh.Clear();
        coneMesh.vertices = vertices;
        coneMesh.triangles = triangles;
        coneMesh.RecalculateNormals();
        coneMesh.RecalculateBounds();

        coneCollider.sharedMesh = null;
        coneCollider.sharedMesh = coneMesh;
    }




    public void SetColor(Color color)
    {
        if (coneMaterial != null && !dogAIController.isHidable())
            coneMaterial.color = color;
    }




    [SerializeField] private Transform playerBody;
    private void DetectionTasks()
    {
        currentTarget = playerBody;
        SetColor(detectedColor);
        OnTargetDetected?.Invoke(currentTarget);
        return;
    }
}
