using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VisionCone : MonoBehaviour
{
    [Header("Shape")]
    public float radius = 5f;
    [Range(8, 128)] public int segments = 48;
    public float yOffset = 1.2f;

    [Header("Detection")]
    public LayerMask obstacleMask;
    public LayerMask catLayer;
    public string catTag = "cat";

    [Header("Appearance")]
    [Range(0f, 1f)] public float alpha = 0.15f;
    public float sphereCastRadius = 0.05f;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private RaycastHit[] hitBuffer = new RaycastHit[1];

    private NPCNavAgentHandler agentHandler;

    void Awake()
    {
        agentHandler = GetComponent<NPCNavAgentHandler>();

        mesh = new Mesh { name = "VisionMesh" };
        GetComponent<MeshFilter>().sharedMesh = mesh;

        SetupMaterial();
        SetupCollider();
        AllocateMesh();
        UpdateMesh();
    }

    void Update()
    {
        UpdateMesh();
    }

    void AllocateMesh()
    {
        vertices = new Vector3[segments + 1];
        triangles = new int[segments * 3];

        for (int i = 0; i < segments; i++)
        {
            int t = i * 3;

            triangles[t]     = 0;
            triangles[t + 1] = (i + 1) % segments + 1;
            triangles[t + 2] = i + 1;
        }
    }

    void SetupCollider()
    {
        var col = GetComponent<MeshCollider>();
        col.convex = true;
        col.isTrigger = true;
        col.sharedMesh = mesh;
    }

    void SetupMaterial()
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetFloat("_Surface", 1);
        mat.SetFloat("_ZWrite", 0);
        mat.renderQueue = 3000;
        mat.color = new Color(1, 1, 1, alpha);

        GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    void UpdateMesh()
    {
        Vector3 localOrigin = Vector3.up * yOffset;
        Vector3 worldOrigin = transform.TransformPoint(localOrigin);

        vertices[0] = localOrigin;

        float angleStep = Mathf.PI * 2f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = angleStep * i;

            // LOCAL direction (for mesh)
            Vector3 localDir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            // WORLD direction (for physics)
            Vector3 worldDir = transform.TransformDirection(localDir);

            float finalDist = radius;

            int hitCount = Physics.SphereCastNonAlloc(
                worldOrigin,
                sphereCastRadius,
                worldDir,
                hitBuffer,
                radius,
                obstacleMask,
                QueryTriggerInteraction.Ignore
            );

            if (hitCount > 0)
            {
                finalDist = Mathf.Max(0.05f, hitBuffer[0].distance);
            }

            vertices[i + 1] = localOrigin + localDir * finalDist;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    void OnTriggerStay(Collider c)
    {
        if (c.CompareTag(catTag) || ((1 << c.gameObject.layer) & catLayer) != 0)
        {
            agentHandler.MoveNext(c.transform.position);
        }
    }
}
