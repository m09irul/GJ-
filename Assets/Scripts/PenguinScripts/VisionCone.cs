using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider), typeof(MeshRenderer))]
public class VisionCone : MonoBehaviour
{
    [Header("Circle Settings")]
    public float radius = 5f;
    public int segments = 48;
    public float yOffset = 1.2f; // How high above object the circle floats

    [Header("Detection")]
    public LayerMask catLayer;
    public string catTag = "cat";

    [Header("Appearance")]
    [Range(0f, 1f)] public float alpha = 0.15f; // Transparency

    private Mesh mesh;
    private Material material;

    void Awake()
    {
        CreateCircleMesh();
        SetupMaterial();
    }

    private void CreateCircleMesh()
    {
        mesh = new Mesh();
        mesh.name = "PoliceVisionMesh";

        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        // Center vertex slightly above object
        vertices[0] = new Vector3(0, yOffset, 0);

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, yOffset, Mathf.Sin(angle) * radius);

            int k = i * 3;
            // Flip triangles for top-facing mesh
            triangles[k] = 0;
            triangles[k + 1] = i + 2 > segments ? 1 : i + 2;
            triangles[k + 2] = i + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;

        // Setup collider
        var col = GetComponent<MeshCollider>();
        col.sharedMesh = mesh;
        col.convex = true;
        col.isTrigger = true;
    }

    private void SetupMaterial()
    {
        // Create material
        material = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        // Enable transparency
        material.SetFloat("_Surface", 1); // Transparent
        material.SetFloat("_Blend", 0);   // Alpha blend
        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0);  // Do not write to depth buffer
        material.renderQueue = 3000;

        // Set color with alpha
        material.color = new Color(1f, 1f, 1f, alpha);

        GetComponent<MeshRenderer>().material = material;
    }

    private void OnTriggerEnter(Collider c)
    {
        if (c.CompareTag(catTag) || ((1 << c.gameObject.layer) & catLayer) != 0)
            OnCatDetected(c.transform.position);
    }

    private void OnCatDetected(Vector3 pos)
    {
        Debug.Log("Cat detected at " + pos);
    }

    // Optional: change transparency/color at runtime
    public void SetColor(Color color)
    {
        if (material != null)
            material.color = color;
    }
}
