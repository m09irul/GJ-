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

    private Vector3 EyeOffset => new Vector3(0, 0.18f, 0.3f);
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
        handleDetection();
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

        // Trigger proxy (same script)
        //ConeTriggerProxy proxy = coneObject.AddComponent<ConeTriggerProxy>();
        //proxy.owner = this;
        // ---------------------------------------

        GenerateDynamicConeMesh();
    }

    private void GenerateDynamicConeMesh()
    {
        int ringCount = coneSegments + 2;
        Vector3[] vertices = new Vector3[ringCount * 2];
        int[] triangles = new int[(coneSegments * 12) + 12];

        float height = 1.2f;
        float step = (coneAngle * 2f) / coneSegments;
        Vector3 origin = coneObject.transform.position;

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
                {
                    distance = Mathf.Max(0.05f, hit.distance - 0.02f);
                    detectDistance = distance;
                }
            }

            Vector3 point = dirLocal * distance;
            vertices[i + 1] = point;
            vertices[i + 1 + ringCount] = point + Vector3.up * height;
        }

        int t = 0;

        for (int i = 0; i < coneSegments; i++)
        {
            triangles[t++] = ringCount;
            triangles[t++] = ringCount + i + 1;
            triangles[t++] = ringCount + i + 2;
        }

        for (int i = 0; i < coneSegments; i++)
        {
            int bottomA = i + 1;
            int bottomB = i + 2;
            int topA = bottomA + ringCount;
            int topB = bottomB + ringCount;

            triangles[t++] = bottomA;
            triangles[t++] = topB;
            triangles[t++] = topA;

            triangles[t++] = bottomA;
            triangles[t++] = bottomB;
            triangles[t++] = topB;
        }

        triangles[t++] = 0;
        triangles[t++] = 1;
        triangles[t++] = ringCount + 1;

        triangles[t++] = 0;
        triangles[t++] = ringCount + 1;
        triangles[t++] = ringCount;

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

        coneCollider.sharedMesh = null;
        coneCollider.sharedMesh = coneMesh;
    }

    // ---------- INTERNAL TRIGGER HANDLERS ----------
    internal void HandleTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag) &&
            !other.transform.root.CompareTag(targetTag))
            return;

        Vector3 origin = coneObject.transform.position;
        Vector3 dir = (other.transform.position - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, coneDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.CompareTag(targetTag) &&
                !hit.collider.transform.root.CompareTag(targetTag))
                return;
        }

        currentTarget = other.transform;
        SetColor(detectedColor);
        OnTargetDetected?.Invoke(currentTarget);
    }

    internal void HandleTriggerExit(Collider other)
    {
        if (!other.CompareTag(targetTag) &&
            !other.transform.root.CompareTag(targetTag))
            return;

        if (other.transform == currentTarget)
        {
            currentTarget = null;
            SetColor(idleColor);
        }
    }

    public void SetColor(Color color)
    {
        if (coneMaterial != null && !dogAIController.isHidable())
            coneMaterial.color = color;
    }

    // ---------- PROXY (REQUIRED) ----------
    //private class ConeTriggerProxy : MonoBehaviour
    //{
    //    public DogVisionCone owner;

    //    private void OnTriggerEnter(Collider other)
    //    {
    //        owner.HandleTriggerEnter(other);
    //    }

    //    private void OnTriggerExit(Collider other)
    //    {
    //        owner.HandleTriggerExit(other);
    //    }
    //}




    [SerializeField] private Transform playerBody;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform dogBody;
    void handleDetection()
    {
        float dist = Vector3.Distance(transform.position ,playerBody.position);
        
        if(dist <= detectDistance)
        {
            if (IsTargetInVisionCone(transform.position, dogBody.forward, playerBody.position, coneAngle))
            {
                Physics.Raycast(transform.position, (playerBody.position - transform.position).normalized, out RaycastHit hit);
                //if(hit.collider.CompareTag(targetTag))
                //{
                    currentTarget = playerBody;
                    SetColor(detectedColor);
                    OnTargetDetected?.Invoke(currentTarget);
                    return;
                //}
                
            }

            currentTarget = null;
            SetColor(idleColor);
        }
        
    }

    bool IsTargetInVisionCone(Vector3 origin, Vector3 forward, Vector3 target, float conAngle)
    {
        Vector3 dirToTarget = target - origin;
        dirToTarget.y = 0; // horizontal plane only
        if (dirToTarget.sqrMagnitude == 0) return true; // target is on origin

        forward.y = 0;
        forward.Normalize();
        dirToTarget.Normalize();

        float angle = Vector3.Angle(forward, dirToTarget);
        return angle <= conAngle;
    }
}
