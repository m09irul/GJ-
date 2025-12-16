using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VisionCone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject PenguinePrefab;
    [SerializeField] private Transform MainCamera;

    [Header("Vision Settings")]
    public float radius = 5f;
    [Range(8, 128)] public int segments = 48;
    public float yOffset = 1.2f;

    [Header("Layers")]
    public LayerMask obstacleMask;
    public string catTag = "cat";

    [Header("Visuals")]
    [Range(0f, 1f)] public float alpha = 0.15f;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Collider detectedPlayerCollider = null;
    private bool isSpawned = false;

    public event System.Action<Vector3> OnPlayerDetected;
    public event System.Action OnPlayerLost;


    void Awake()
    {
        if (!MainCamera)
            MainCamera = GameObject.FindGameObjectWithTag("MainCamera")?.transform;

        mesh = new Mesh { name = "VisionMesh" };
        GetComponent<MeshFilter>().sharedMesh = mesh;

        SetupMaterial();
        AllocateMesh();
        UpdateMesh();

        // Add and configure MeshCollider
        MeshCollider mc = GetComponent<MeshCollider>();
        mc.sharedMesh = mesh;
        mc.convex = true;       // Must be convex to act as trigger
        mc.isTrigger = true;
    }


    void Update()
    {
        UpdateMesh();
    }

    // ---------------- MESH ---------------- 
    void AllocateMesh()
    {
        vertices = new Vector3[segments + 1];
        triangles = new int[segments * 3];

        for (int i = 0; i < segments; i++)
        {
            int t = i * 3;
            triangles[t] = 0;
            triangles[t + 1] = (i + 1) % segments + 1;
            triangles[t + 2] = i + 1;
        }
    }

    void SetupMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetFloat("_ZWrite", 0);
        mat.renderQueue = 3000;
        mat.color = new Color(1f, 1f, 1f, alpha);
        GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    void UpdateMesh()
    {
        Vector3 localOrigin = Vector3.up * yOffset;
        vertices[0] = localOrigin;
        float angleStep = Mathf.PI * 2f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = angleStep * i;
            Vector3 localDir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 worldDir = transform.TransformDirection(localDir);

            float finalDist = radius;

            if (Physics.SphereCast(
                transform.TransformPoint(localOrigin),
                0.05f,
                worldDir,
                out RaycastHit hit,
                radius,
                obstacleMask,
                QueryTriggerInteraction.Ignore))
            {
                finalDist = Mathf.Max(0.05f, hit.distance);
            }

            vertices[i + 1] = localOrigin + localDir * finalDist;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Update MeshCollider to match mesh
        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc)
        {
            mc.sharedMesh = null;  // Required to force update
            mc.sharedMesh = mesh;
        }
    }

    // ---------------- TRIGGER DETECTION ----------------

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(catTag))
        {
            detectedPlayerCollider = other;
            OnPlayerDetected?.Invoke(other.transform.position);
            if (!isSpawned)
                SpawnPolice(1);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (detectedPlayerCollider != null && other == detectedPlayerCollider)
        {
            OnPlayerDetected?.Invoke(other.transform.position);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(catTag) && detectedPlayerCollider != null)
        {
            OnPlayerLost?.Invoke();
            detectedPlayerCollider = null;
        }
    }

    // ---------------- SPAWN ----------------
    private void SpawnPolice(int num)
    {
        if(PenguinePrefab != null){
            isSpawned = true;

            Transform catTransform = GameObject.FindGameObjectWithTag(catTag)?.transform;
            if (!catTransform || !MainCamera) return;

            for (int i = 0; i < num; i++)
            {
                Vector3 spawnPos = new Vector3(MainCamera.position.x, transform.position.y, MainCamera.position.z);

                Instantiate(PenguinePrefab, spawnPos, Quaternion.identity)
                    .GetComponent<NPCNavAgentHandler>()
                    .MoveNext(catTransform.position);
            }
        }
    }
}