using UnityEngine;
using System.Collections.Generic;

public class DogVisionCone : MonoBehaviour
{
    [Header("Cone Settings")]
    public float coneAngle = 60f;
    public float coneDistance = 5f;
    public int coneSegments = 50;

    [Header("Detection")]
    public string targetTag = "cat";
    public string[] ignoreTags = { "Player", "cat", "Dog" }; // Tags that won't block vision

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
        coneObject.layer = LayerMask.NameToLayer("Ignore Raycast"); // Don't detect own cone

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
        coneMaterial.SetInt("_Cull", 0); // Disable backface culling so both sides render
        coneMaterial.SetInt("_ZTest", 4); // Always render on top
        coneMaterial.renderQueue = 3000;

        coneMaterial.color = idleColor;
        mr.material = coneMaterial;

        GenerateDynamicConeMesh();
    }

    private bool ShouldIgnoreTag(string tag)
    {
        foreach (string ignoreTag in ignoreTags)
        {
            if (tag == ignoreTag)
                return true;
        }
        return false;
    }

    private void GenerateDynamicConeMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        vertices.Add(Vector3.zero); // cone origin (local space) - index 0

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

            // Raycast to find obstacles that block vision
            RaycastHit hit;
            if (Physics.Raycast(origin, dirWorld, out hit, coneDistance))
            {
                // Check if we should ignore this object
                if (!ShouldIgnoreTag(hit.collider.tag))
                {
                    // This is an obstacle - cut the cone here
                    distance = Mathf.Max(0.1f, hit.distance - 0.05f);
                }
            }

            // Mesh vertex (LOCAL space)
            vertices.Add(dirLocal * distance);
        }

        // Create triangles
        for (int i = 0; i < coneSegments; i++)
        {
            triangles.Add(0);
            triangles.Add(i + 1);
            triangles.Add(i + 2);
        }

        coneMesh.Clear();
        coneMesh.vertices = vertices.ToArray();
        coneMesh.triangles = triangles.ToArray();
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

            Vector3 targetPos = hit.transform.position;
            Vector3 dir = (targetPos - origin).normalized;
            float distanceToTarget = Vector3.Distance(origin, targetPos);

            // Check angle
            if (Vector3.Angle(coneObject.transform.forward, dir) <= coneAngle)
            {
                // Check line of sight - make sure no obstacles block the view
                RaycastHit[] rayHits = Physics.RaycastAll(origin, dir, distanceToTarget);

                bool targetVisible = true;

                foreach (RaycastHit rayHit in rayHits)
                {
                    // Skip the target itself and ignored tags
                    if (rayHit.collider == hit || ShouldIgnoreTag(rayHit.collider.tag))
                        continue;

                    // Hit an obstacle before reaching the target
                    if (rayHit.distance < distanceToTarget)
                    {
                        targetVisible = false;
                        break;
                    }
                }

                if (targetVisible)
                {
                    OnTargetDetected?.Invoke(hit.transform);
                    return;
                }
            }
        }
    }

    public void SetColor(Color color)
    {
        if (coneMaterial != null)
            coneMaterial.color = color;
    }

    // Optional: Visualize raycasts in editor for debugging
    private void OnDrawGizmos()
    {
        if (coneObject == null) return;

        Vector3 origin = coneObject.transform.position;
        float step = (coneAngle * 2f) / coneSegments;

        for (int i = 0; i <= coneSegments; i += 5) // Draw every 5th ray for performance
        {
            float ang = -coneAngle + i * step;
            float rad = Mathf.Deg2Rad * ang;
            Vector3 dirLocal = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
            Vector3 dirWorld = coneObject.transform.TransformDirection(dirLocal);

            float distance = coneDistance;
            RaycastHit[] hits = Physics.RaycastAll(origin, dirWorld, coneDistance);

            bool hitObstacle = false;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject == coneObject)
                    continue;

                if (!ShouldIgnoreTag(hit.collider.tag))
                {
                    distance = hit.distance;
                    hitObstacle = true;
                    break;
                }
            }

            Gizmos.color = hitObstacle ? Color.red : Color.green;
            Gizmos.DrawLine(origin, origin + dirWorld * distance);
        }
    }
}