using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject healthBarGameObj;
    [SerializeField] private int confidence = 100;
    [SerializeField] private Joystick movementJostick;

    [Header("Movement")]
    public float velocity = 5f;
    public float gravity = 12f;

    private bool isOnSkateboard;

    private float inputHorizontal;
    private float inputVertical;

    [SerializeField] private Animator animator;
    public CharacterController cc;
    private float verticalVelocity;
    private GameManager gameManager;
    private UIManager uIManager;

    private SegmentedBarUI confidenceBar;

    [Header("Throw")]
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float arcForce = 4f;

    [Header("Throw Preview")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPoints = 20;
    [SerializeField] private float trajectoryTimeStep = 0.1f;

    private bool isPreviewingThrow;

    [Header("Rail")]
    public RailPath railPath;


    // --------------------------------------------------

    void Start()
    {
        gameManager = GameManager.Instance;
        uIManager = UIManager.Instance;

        confidenceBar = uIManager.confidenceBar.GetComponent<SegmentedBarUI>();

        AudioManager.instance.play("main");

        if (animator == null)
            Debug.LogWarning("Animator missing on PlayerController");
    }

    void Update()
    {
        inputHorizontal = movementJostick.Horizontal;
        inputVertical = movementJostick.Vertical;

        if (cc.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        if (animator != null)
            animator.SetBool("run", cc.velocity.magnitude > 0.01f);
    }

    void FixedUpdate()
    {
        float speed = velocity;

        Vector3 inputMove =
            CameraForwardFlat() * inputVertical +
            CameraRightFlat() * inputHorizontal;

        inputMove *= speed;

        // gravity
        verticalVelocity -= gravity * Time.fixedDeltaTime;

        Vector3 finalMove =
            inputMove * Time.fixedDeltaTime +
            Vector3.up * verticalVelocity * Time.fixedDeltaTime;

        cc.Move(finalMove);

        ClampToRail();

        RotateTowardsMovement(inputMove);
    }

    // --------------------------------------------------
    // RAIL CLAMP
    // --------------------------------------------------

    void ClampToRail()
    {
        if (!railPath || railPath.nodes.Count < 2)
            return;

        Vector3 worldPos = transform.position;
        float bestDist = float.MaxValue;

        Vector3 bestCenter = worldPos;
        Vector3 bestDir = Vector3.forward;
        float bestHalfWidth = 1f;

        for (int i = 0; i < railPath.nodes.Count - 1; i++)
        {
            Vector3 a = railPath.transform.TransformPoint(railPath.nodes[i].position);
            Vector3 b = railPath.transform.TransformPoint(railPath.nodes[i + 1].position);

            Vector3 ab = b - a;
            float t = Vector3.Dot(worldPos - a, ab) / ab.sqrMagnitude;
            t = Mathf.Clamp01(t);

            Vector3 closest = a + ab * t;
            float dist = Vector3.SqrMagnitude(worldPos - closest);

            if (dist < bestDist)
            {
                bestDist = dist;
                bestCenter = closest;
                bestDir = ab.normalized;
                bestHalfWidth = Mathf.Lerp(
                    railPath.nodes[i].halfWidth,
                    railPath.nodes[i + 1].halfWidth,
                    t
                );
            }
        }

        Vector3 side = Vector3.Cross(Vector3.up, bestDir).normalized;
        Vector3 offset = worldPos - bestCenter;

        float sideAmount = Vector3.Dot(offset, side);
        sideAmount = Mathf.Clamp(sideAmount, -bestHalfWidth, bestHalfWidth);

        Vector3 clampedPos =
            bestCenter +
            side * sideAmount +
            Vector3.up * offset.y;

        transform.position = clampedPos;
    }

    // --------------------------------------------------
    // ROTATION
    // --------------------------------------------------

    void RotateTowardsMovement(Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude < 0.001f)
            return;

        float angle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
        Quaternion target = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, 0.15f);
    }

    // --------------------------------------------------
    // CAMERA HELPERS
    // --------------------------------------------------

    Vector3 CameraForwardFlat()
    {
        Vector3 f = Camera.main.transform.forward;
        f.y = 0;
        return f.normalized;
    }

    Vector3 CameraRightFlat()
    {
        Vector3 r = Camera.main.transform.right;
        r.y = 0;
        return r.normalized;
    }
// --------------------------------------------------
// THROW SYSTEM (SINGLE SOURCE OF TRUTH)
// --------------------------------------------------

private Vector3 GetThrowDirection()
{
    Vector3 dir = transform.forward;
    dir.y = 0;
    return dir.normalized;
}

private Vector3 GetThrowVelocity()
{
    return GetThrowDirection() * throwForce + Vector3.up * arcForce;
}

private void DrawTrajectory()
{
    if (!throwPoint || !trajectoryLine) return;

    Vector3 startPos = throwPoint.position;
    Vector3 velocity = GetThrowVelocity();

    trajectoryLine.positionCount = trajectoryPoints;

    for (int i = 0; i < trajectoryPoints; i++)
    {
        float t = i * trajectoryTimeStep;
        Vector3 point =
            startPos +
            velocity * t +
            0.5f * Physics.gravity * t * t;

        trajectoryLine.SetPosition(i, point);
    }
}

public void StartThrowPreview()
{
    isPreviewingThrow = true;
    trajectoryLine.enabled = true;
}

public void StopThrowPreview()
{
    isPreviewingThrow = false;
    trajectoryLine.enabled = false;
}


public void ThrowItem(GameObject projectilePrefab)
{
    if (!projectilePrefab || !throwPoint) return;

    GameObject proj = Instantiate(
        projectilePrefab,
        throwPoint.position,
        Quaternion.identity
    );

    if (proj.TryGetComponent(out Rigidbody rb))
    {
        rb.velocity = GetThrowVelocity();
    }
}

// --------------------------------------------------

void OnTriggerEnter(Collider other)
{

    if (other.CompareTag("car"))
    {
        ReduceConfidence(4);
    }
}

public void ReduceConfidence(int value)
{
    gameManager.TakeHit(value);
}
}
