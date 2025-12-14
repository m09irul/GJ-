using UnityEngine;

[RequireComponent(typeof(SphereCollider), typeof(MeshFilter), typeof(MeshRenderer))]
public class VisionCone : MonoBehaviour
{

    [SerializeField] private GameObject PenguinePrefab;
    [SerializeField] private Transform MainCamera;
    public float radius = 5f;
    [Range(8, 128)] public int segments = 48;
    public float yOffset = 1.2f;
    public LayerMask obstacleMask;
    public LayerMask catLayer;
    public string catTag = "cat";

    [Range(0f, 1f)] public float alpha = 0.15f;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private NPCNavAgentHandler agentHandler;

    private Collider detectedPlayerCollider = null;

    public event System.Action<Vector3> OnPlayerDetected;
    public event System.Action OnPlayerLost;

    private bool isSpawend;
    void Awake()
    {
        isSpawend = false;
        agentHandler = GetComponent<NPCNavAgentHandler>();
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;

        mesh = new Mesh { name = "VisionMesh" };
        GetComponent<MeshFilter>().sharedMesh = mesh;

        SetupMaterial();
        AllocateMesh();
        UpdateMesh();

        SphereCollider collider = GetComponent<SphereCollider>();
        collider.radius = radius;
        collider.isTrigger = true;
    }

    void Update()
    {
        UpdateMesh();
    }

    // Allocate vertices and triangles for the vision cone mesh
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

    // Setup material for vision cone appearance
    void SetupMaterial()
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetFloat("_Surface", 1);   // Transparent
        mat.SetFloat("_ZWrite", 0);
        mat.renderQueue = 3000;
        mat.color = new Color(1f, 1f, 1f, alpha);

        GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    // Update the mesh for the vision cone
    void UpdateMesh()
    {
        Vector3 localOrigin = Vector3.up * yOffset;
        Vector3 worldOrigin = transform.TransformPoint(localOrigin);

        vertices[0] = localOrigin;

        float angleStep = Mathf.PI * 2f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = angleStep * i;

            Vector3 localDir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 worldDir = transform.TransformDirection(localDir);

            float finalDist = radius;

            if (Physics.SphereCast(
                worldOrigin,
                0.05f,  // Small sphere cast radius for checking obstacles
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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(catTag))
        {
            detectedPlayerCollider = other;
            OnPlayerDetected?.Invoke(other.transform.position);
            int stars = 1;
            //stars = GameManager.Instance.Stars();
            if(!isSpawend) 
                spawnPolice(stars);
        }
    }

    private void spawnPolice(int num)
    {
        isSpawend = true;
        while (num > 0) {
            Instantiate(PenguinePrefab, new Vector3(MainCamera.position.x,transform.position.y,MainCamera.position.z), Quaternion.identity).GetComponent<NPCNavAgentHandler>().MoveNext(GameObject.FindGameObjectWithTag(catTag).transform.position);
            num--;
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

}
