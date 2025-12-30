using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    // ==================================================
    // REFERENCES
    // ==================================================
    [Header("Refs")]
    [SerializeField] private GameObject healthBarGameObj;
    [SerializeField] private Animator animator;
    [SerializeField] private WalkableArea walkableArea;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private LineRenderer trajectoryLine;

    // ==================================================
    // STATS
    // ==================================================
    [Header("Stats")]
    [SerializeField] private int confidence = 100;

    // ==================================================
    // MOVEMENT
    // ==================================================
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 5.5f;
    [SerializeField] private float runThreshold = 0.75f;
    [SerializeField] private float rotationSpeed = 6.5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float gravity = 18f;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.12f;

    [Header("Air Control")]
    [SerializeField, Range(0f, 1f)]
    private float airControlStrength = 0.35f;

    // ==================================================
    // THROW
    // ==================================================
    [Header("Throw")]
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float arcForce = 4f;

    [Header("Throw Preview")]
    [SerializeField] private int trajectoryPoints = 20;
    [SerializeField] private float trajectoryTimeStep = 0.1f;

    // ==================================================
    // INTERNAL
    // ==================================================
    private CharacterController cc;
    private Camera mainCam;
    private GameManager gameManager;

    private float inputH;
    private float inputV;
    private float inputMagnitude;

    private float verticalVelocity;
    private float coyoteTimer;

    private Vector3 lastMove;
    private bool isPreviewingThrow;

    // 🔒 Jump lock
    private Vector3 lockedJumpDir;
    private float lockedJumpSpeed;
    private bool isJumping;

    // ==================================================
    // ANIMATOR HASHES
    // ==================================================
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
    private static readonly int JumpHash = Animator.StringToHash("Jump");

    // ==================================================
    // UNITY
    // ==================================================
    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        gameManager = GameManager.Instance;

        if (trajectoryLine)
            trajectoryLine.enabled = false;
    }

    private void Start()
    {
        mainCam = Camera.main;

        if (!animator)
            Debug.LogWarning("Animator missing on PlayerController");
    }

    private void Update()
    {
        ReadInput();
        HandleMovement();
        HandleAnimation();

        if (isPreviewingThrow)
            DrawTrajectory();
    }

    // ==================================================
    // INPUT
    // ==================================================
    private void ReadInput()
    {
        inputH = Input.GetAxis("Horizontal");
        inputV = Input.GetAxis("Vertical");
    }
    public void SnapPlayerPosition(Vector3 newPos)
    {
        transform.position = newPos;
    }
    // ==================================================
    // MOVEMENT
    // ==================================================
    private void HandleMovement()
    {
        Vector3 inputDir = GetCameraRelativeInput();
        inputMagnitude = inputDir.magnitude;

        bool grounded = cc.isGrounded;

        if (grounded)
        {
            isJumping = false;
            coyoteTimer = coyoteTime;

            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            float speed = (inputMagnitude >= runThreshold) ? runSpeed : walkSpeed;

            if (Input.GetButtonDown("Jump"))
                StartJump(inputDir, speed);

            Vector3 groundVelocity =
                inputDir.normalized * speed * inputMagnitude +
                Vector3.up * verticalVelocity;

            ApplyMove(groundVelocity);
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
            verticalVelocity -= gravity * Time.deltaTime;

            if (!isJumping && coyoteTimer > 0f && Input.GetButtonDown("Jump"))
                StartJump(inputDir, walkSpeed);

            float forwardInfluence = 0f;

            if (lockedJumpDir != Vector3.zero && inputMagnitude > 0.01f)
            {
                // Only allow influence along locked jump direction
                forwardInfluence = Vector3.Dot(inputDir.normalized, lockedJumpDir);
                forwardInfluence = Mathf.Clamp01(forwardInfluence);
            }

            float airSpeed = Mathf.Lerp(
                lockedJumpSpeed,
                lockedJumpSpeed * forwardInfluence,
                airControlStrength
            );

            Vector3 airVelocity =
                lockedJumpDir * airSpeed +
                Vector3.up * verticalVelocity;

            ApplyMove(airVelocity);
        }

        RotateFromMovement();
    }

    private void StartJump(Vector3 inputDir, float speed)
    {
        isJumping = true;
        verticalVelocity = jumpForce;
        coyoteTimer = 0f;

        if (inputDir.magnitude > 0.05f)
        {
            lockedJumpDir = inputDir.normalized;
            lockedJumpSpeed = speed;
        }
        else
        {
            lockedJumpDir = Vector3.zero;
            lockedJumpSpeed = 0f;
        }

        if (animator)
            animator.SetTrigger(JumpHash);
    }

    private void ApplyMove(Vector3 velocity)
    {
        Vector3 deltaMove = velocity * Time.deltaTime;

        if (walkableArea)
        {
            Vector3 nextPos = transform.position + deltaMove;
            Vector3 clampedPos = walkableArea.ClampPoint(nextPos);
            deltaMove = clampedPos - transform.position;
        }

        cc.Move(deltaMove);
        lastMove = deltaMove;
    }

    private void RotateFromMovement()
    {
        Vector3 flatMove = lastMove;
        flatMove.y = 0f;

        if (flatMove.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatMove);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private Vector3 GetCameraRelativeInput()
    {
        if (!mainCam)
            return Vector3.zero;

        Vector3 forward = mainCam.transform.forward;
        Vector3 right = mainCam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        return Vector3.ClampMagnitude(
            forward.normalized * inputV + right.normalized * inputH,
            1f
        );
    }

    // ==================================================
    // ANIMATION
    // ==================================================
    private void HandleAnimation()
    {
        if (!animator) return;

        animator.SetFloat(SpeedHash, inputMagnitude, 0.1f, Time.deltaTime);
        animator.SetBool(IsGroundedHash, cc.isGrounded);
        animator.SetFloat(VerticalSpeedHash, verticalVelocity);
    }

    // ==================================================
    // THROW SYSTEM
    // ==================================================
    private Vector3 GetThrowVelocity()
    {
        Vector3 dir = throwPoint.forward;
        dir.y = 0f;
        dir.Normalize();

        return dir * throwForce + Vector3.up * arcForce;
    }

    private void DrawTrajectory()
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

    // ==================================================
    // DAMAGE
    // ==================================================
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("car"))
            ReduceConfidence(4);
    }

    public void ReduceConfidence(int value)
    {
        gameManager.TakeHit(value);
    }
}
