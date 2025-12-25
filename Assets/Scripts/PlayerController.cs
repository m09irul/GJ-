using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // --------------------------------------------------
    // REFERENCES
    // --------------------------------------------------
    [Header("Refs")]
    [SerializeField] private GameObject healthBarGameObj;
    [SerializeField] private Animator animator;
    [SerializeField] private WalkableArea walkableArea;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private LineRenderer trajectoryLine;

    // --------------------------------------------------
    // STATS
    // --------------------------------------------------
    [Header("Stats")]
    [SerializeField] private int confidence = 100;

    // --------------------------------------------------
    // MOVEMENT
    // --------------------------------------------------
    [Header("Movement")]
    [SerializeField] private float velocity = 5f;
    [SerializeField] private float gravity = 12f;
    [SerializeField] private float jumpForce = 6.5f;
    [SerializeField] private float rotationSpeed = 6.5f;

    // --------------------------------------------------
    // THROW
    // --------------------------------------------------
    [Header("Throw")]
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float arcForce = 4f;

    // --------------------------------------------------
    // THROW PREVIEW
    // --------------------------------------------------
    [Header("Throw Preview")]
    [SerializeField] private int trajectoryPoints = 20;
    [SerializeField] private float trajectoryTimeStep = 0.1f;

    // --------------------------------------------------
    // INTERNAL
    // --------------------------------------------------
    private CharacterController cc;
    private Camera mainCam;
    private GameManager gameManager;

    private float inputH;
    private float inputV;
    private float verticalVelocity;

    private Vector3 lastMove; // actual applied movement
    private bool isPreviewingThrow;

    private static readonly int RunHash = Animator.StringToHash("run");

    // --------------------------------------------------
    // UNITY
    // --------------------------------------------------
    void Awake()
    {
        cc = GetComponent<CharacterController>();
        gameManager = GameManager.Instance;

        if (trajectoryLine)
            trajectoryLine.enabled = false;
    }

    void Start()
    {
        mainCam = Camera.main;

        AudioManager.instance.play("main");

        if (!animator)
            Debug.LogWarning("Animator missing on PlayerController");
    }

    void Update()
    {
        ReadInput();
        HandleMovement();
        HandleAnimation();

        if (isPreviewingThrow)
            DrawTrajectory();
    }

    // --------------------------------------------------
    // INPUT
    // --------------------------------------------------
    void ReadInput()
    {
        inputH = Input.GetAxis("Horizontal");
        inputV = Input.GetAxis("Vertical");
    }

    // --------------------------------------------------
    // MOVEMENT
    // --------------------------------------------------
    void HandleMovement()
    {
        Vector3 moveDir = GetCameraRelativeInput();

        // --- Ground & gravity ---
        if (cc.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (Input.GetButtonDown("Jump"))
                verticalVelocity = jumpForce;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        Vector3 velocityVec =
            moveDir * velocity +
            Vector3.up * verticalVelocity;

        Vector3 deltaMove = velocityVec * Time.deltaTime;

        // --- Clamp movement (NOT position) ---
        if (walkableArea)
        {
            Vector3 nextPos = transform.position + deltaMove;
            Vector3 clampedPos = walkableArea.ClampPoint(nextPos);
            deltaMove = clampedPos - transform.position;
        }

        cc.Move(deltaMove);
        lastMove = deltaMove;

        // Rotate only if real movement happened
        Vector3 flatMove = lastMove;
        flatMove.y = 0f;

        if (flatMove.sqrMagnitude > 0.0001f)
            RotateTowards(flatMove.normalized);
    }

    Vector3 GetCameraRelativeInput()
    {
        if (!mainCam)
            return Vector3.zero;

        Vector3 forward = mainCam.transform.forward;
        Vector3 right = mainCam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        Vector3 dir = forward.normalized * inputV + right.normalized * inputH;
        return Vector3.ClampMagnitude(dir, 1f);
    }

    // --------------------------------------------------
    // ANIMATION
    // --------------------------------------------------
    void HandleAnimation()
    {
        if (!animator) return;

        Vector3 horizontalMove = lastMove;
        horizontalMove.y = 0f;

        bool isRunning = horizontalMove.sqrMagnitude > 0.0001f;
        animator.SetBool(RunHash, isRunning);
    }

    // --------------------------------------------------
    // ROTATION
    // --------------------------------------------------
    void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }

    // --------------------------------------------------
    // THROW SYSTEM
    // --------------------------------------------------
    Vector3 GetThrowVelocity()
    {
        Vector3 dir = throwPoint.forward;
        dir.y = 0f;
        dir.Normalize();

        return dir * throwForce + Vector3.up * arcForce;
    }

    void DrawTrajectory()
    {
        if (!throwPoint || !trajectoryLine) return;

        Vector3 start = throwPoint.position;
        Vector3 vel = GetThrowVelocity();
        Vector3 gravityVec = Physics.gravity;

        trajectoryLine.positionCount = trajectoryPoints;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * trajectoryTimeStep;
            trajectoryLine.SetPosition(
                i,
                start + vel * t + 0.5f * gravityVec * t * t
            );
        }
    }

    public void StartThrowPreview()
    {
        if (!trajectoryLine) return;

        isPreviewingThrow = true;
        trajectoryLine.enabled = true;
    }

    public void StopThrowPreview()
    {
        if (!trajectoryLine) return;

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
            rb.velocity = GetThrowVelocity();
    }

    // --------------------------------------------------
    // DAMAGE
    // --------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("car"))
            ReduceConfidence(4);
    }

    public void ReduceConfidence(int value)
    {
        gameManager.TakeHit(value);
    }
}
