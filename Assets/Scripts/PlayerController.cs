using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using System;

[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    // ==================================================
    // REFERENCES
    // ==================================================
    [Header("Refs")]
    [SerializeField] private GameObject parcel;
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
    [SerializeField] private Transform dustFXPos;

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

    [Header("Hide")]
    [SerializeField] float hideMoveDuration = 0.4f;
    [SerializeField] float jumpHeight = 1.2f; // adjustable per box if needed

    private Sequence hideSequence;
    private Transform hideAnchor;
    private Transform exitAnchor;

    private bool isHiding;


    // ==================================================
    // INTERNAL
    // ==================================================
    private CharacterController cc;
    private Camera mainCam;
    private GameManager gameManager;

    private float inputH;
    private float inputV;
    private bool jumpPressed;

    private float inputMagnitude;

    private float verticalVelocity;
    private float coyoteTimer;

    private Vector3 lastMove;
    private bool isPreviewingThrow;

    // Air movement (free steering replaces locked-direction system)
    private Vector3 horizontalVelocity;
    private bool isJumping;

    // Jump buffering
    private float jumpBufferTimer;
    [SerializeField] private float jumpBufferTime = 0.12f;

    // Input smoothing
    private float currentSpeed;
    [SerializeField] private float accelerationGround = 25f;
    [SerializeField] private float decelerationGround = 20f;

    // Ground stability (ANTI-JITTER)
    public bool isGroundedStable;
    private float groundedGraceTimer;
    [SerializeField] private float groundedGraceTime = 0.08f;

    private const float GroundStickForce = -2f;
    private const float GroundSnapThreshold = -5f;
    private float actualMoveMagnitude;
    public bool canMove = false;
    public Joystick movementStick;

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
        if (isHiding && movementStick.Direction.magnitude > 0.1f)
        {
            ExitHide();
            return;
        }

        // Correct order: read input → update ground state → move
        ReadInput();
        UpdateGroundedState();

        if (!isHiding)
            HandleMovement();

        if (isPreviewingThrow)
            DrawTrajectory();
    }
    public void ToggleParcel(bool stat)
    {
        parcel.SetActive(stat);
    }
    public void StartHide(Transform insideAnchor, Transform outsideAnchor, Action onComplete = null)
    {
        if (isHiding) return;
        UIManager.Instance.hudPanel.SetActive(false);
        ToggleParcel(false);

        hideAnchor = insideAnchor;
        exitAnchor = outsideAnchor;

        movementStick.ResetJoystick();
        canMove = false;

        isHiding = true;

        hideSequence?.Kill();

        Vector3 startPos = transform.position;
        Vector3 jumpPeak = startPos + Vector3.up * jumpHeight;

        hideSequence = DOTween.Sequence();

        // Play animation (visual jump)
        animator.Play("hide"); // jump → sit
        AudioManager.instance.play("cat hide");

        hideSequence
            // jump up (Y only)
            .Append(transform.DOMoveY(jumpPeak.y, hideMoveDuration * 0.4f)
                .SetEase(Ease.Linear))

            // move into box while falling
            .Append(transform.DOMove(hideAnchor.position, hideMoveDuration * 0.6f)
                .SetEase(Ease.Linear))

            .OnComplete(() =>
            {
                transform.rotation = hideAnchor.rotation;
                canMove = true;
                if (onComplete == null)
                    UIManager.Instance.hudPanel.SetActive(true);

                onComplete.Invoke();
            });
    }
    public void ExitHide()
    {
        movementStick.ResetJoystick();
        UIManager.Instance.hudPanel.SetActive(false);
        hideSequence?.Kill();
        canMove = false;

        Vector3 exitPeak = exitAnchor.position + Vector3.up * jumpHeight;

        hideSequence = DOTween.Sequence();


        animator.Play("unhide"); // reverse animation
        AudioManager.instance.play("cat unhide");

        hideSequence
            // jump up from inside
            .Append(transform.DOMoveY(exitPeak.y, hideMoveDuration * 0.4f)
                .SetEase(Ease.Linear))

            // land outside
            .Append(transform.DOMove(exitAnchor.position, hideMoveDuration * 0.6f)
                .SetEase(Ease.Linear))

            .Join(transform.DORotateQuaternion(
                exitAnchor.rotation,
                hideMoveDuration))

            .OnComplete(() =>
            {
                isHiding = false;
                canMove = true;
                UIManager.Instance.hudPanel.SetActive(true);
                ToggleParcel(true);

            });
    }
    // ==================================================
    // INPUT
    // ==================================================
    private void ReadInput()
    {
        ReadMovementInput();
        ReadJumpInput();
    }

    private void ReadMovementInput()
    {
        if (CinemachineController.Instance.brain.IsBlending)
            movementStick.ResetJoystick();

        inputH = movementStick.Horizontal;
        inputV = movementStick.Vertical;
    }

    public void ReadJumpInput(bool stat = false)
    {
        jumpPressed = Input.GetButtonDown("Jump") || stat;

        // Buffer the jump press so near-landing taps aren't lost
        if (jumpPressed)
            jumpBufferTimer = jumpBufferTime;
    }

    public void SnapPlayerPosition(Vector3 newPos)
    {
        cc.enabled = false;
        transform.position = newPos;
        cc.enabled = true;
    }

    // ==================================================
    // GROUND STABILITY
    // ==================================================
    private void UpdateGroundedState()
    {
        if (cc.isGrounded)
        {
            groundedGraceTimer = groundedGraceTime;
            isGroundedStable = true;

            // Reset jump flag once truly landed (descending or neutral)
            // This prevents the ground-stick from killing the
            // initial upward launch (verticalVelocity > 0).
            if (isJumping && verticalVelocity <= 0f)
                isJumping = false;
        }
        else
        {
            groundedGraceTimer -= Time.deltaTime;
            if (groundedGraceTimer <= 0f)
                isGroundedStable = false;
        }
    }

    // ==================================================
    // MOVEMENT
    // ==================================================
    private void HandleMovement()
    {
        // Guard: only apply gravity when movement is disabled
        if (!canMove)
        {
            if (!isGroundedStable)
                verticalVelocity -= gravity * Time.deltaTime;
            else
                verticalVelocity = GroundStickForce;

            cc.Move(Vector3.up * verticalVelocity * Time.deltaTime);
            return;
        }

        Vector3 inputDir = GetCameraRelativeInput();
        inputMagnitude = inputDir.magnitude;

        // Tick the jump buffer down
        jumpBufferTimer -= Time.deltaTime;

        // Determine if we should jump (pressed OR buffered)
        bool wantsJump = jumpBufferTimer > 0f;

        if (isGroundedStable && !isJumping)
        {
            HandleGroundedMovement(inputDir, wantsJump);
        }
        else
        {
            HandleAirMovement(inputDir, wantsJump);
        }

        RotateFromMovement();
        HandleAnimation();
    }

    // ------- GROUND -------
    private void HandleGroundedMovement(Vector3 inputDir, bool wantsJump)
    {
        isJumping = false;
        coyoteTimer = coyoteTime;

        // Snap vertical velocity on landing to prevent accumulation
        if (verticalVelocity < GroundSnapThreshold)
            verticalVelocity = GroundStickForce;
        else
            verticalVelocity = GroundStickForce; // always stick to ground

        // Target speed based on walk/run threshold
        float targetSpeed = 0f;
        if (inputMagnitude > 0.05f)
            targetSpeed = (inputMagnitude >= runThreshold) ? runSpeed : walkSpeed;

        // Smooth acceleration / deceleration
        float accelRate = (inputMagnitude > 0.05f) ? accelerationGround : decelerationGround;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed * inputMagnitude, accelRate * Time.deltaTime);

        // Build horizontal velocity
        if (inputMagnitude > 0.05f)
            horizontalVelocity = inputDir.normalized * currentSpeed;
        else
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, decelerationGround * Time.deltaTime);

        // Jump
        if (wantsJump)
        {
            StartJump();
        }

        Vector3 groundVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
        ApplyMove(groundVelocity);
    }

    // ------- AIR -------
    private void HandleAirMovement(Vector3 inputDir, bool wantsJump)
    {
        coyoteTimer -= Time.deltaTime;
        verticalVelocity -= gravity * Time.deltaTime;

        // Coyote time jump
        if (!isJumping && coyoteTimer > 0f && wantsJump)
        {
            StartJump();
        }

        // Free air steering: blend horizontal velocity toward input direction
        if (inputMagnitude > 0.05f)
        {
            float currentHSpeed = horizontalVelocity.magnitude;
            float maxAirSpeed = Mathf.Max(currentHSpeed, runSpeed);

            // Desired direction * current speed
            Vector3 desiredHVel = inputDir.normalized * maxAirSpeed;

            // Smoothly steer toward desired direction using airControlStrength
            float airAccel = airControlStrength * accelerationGround;
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                desiredHVel,
                airAccel * Time.deltaTime
            );

            // Clamp to max air speed to prevent acceleration exploits
            if (horizontalVelocity.magnitude > maxAirSpeed)
                horizontalVelocity = horizontalVelocity.normalized * maxAirSpeed;
        }
        else
        {
            // No input in air: apply light friction so velocity doesn't linger
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity, Vector3.zero,
                decelerationGround * 0.3f * Time.deltaTime
            );
        }

        Vector3 airVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
        ApplyMove(airVelocity);
    }

    private void StartJump()
    {
        isJumping = true;
        verticalVelocity = jumpForce;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f; // consume the buffer

        // Clear grounded state so the ground-stick doesn't kill the jump
        isGroundedStable = false;
        groundedGraceTimer = 0f;

        var dustFX = PrefabDatabase.Instance.GetPrefab(8);
        AudioManager.instance.play("cat jump");
        Instantiate(dustFX, dustFXPos.position, dustFXPos.rotation);
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

        if (isGroundedStable && deltaMove.sqrMagnitude < 0.00001f)
            deltaMove = Vector3.zero;

        cc.Move(deltaMove);

        lastMove = deltaMove;

        // actual horizontal movement (what REALLY happened)
        Vector3 flatMove = deltaMove;
        flatMove.y = 0f;
        actualMoveMagnitude = flatMove.magnitude / Time.deltaTime;
    }
    private void RotateFromMovement()
    {
        Vector3 flatMove = lastMove;
        flatMove.y = 0f;

        if (flatMove.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatMove);

            // Sharper rotation on ground, softer in air
            float rotSpeed = isGroundedStable ? rotationSpeed * 2f : rotationSpeed;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotSpeed * Time.deltaTime
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

        animator.SetBool(IsGroundedHash, isGroundedStable);
        animator.SetFloat(VerticalSpeedHash, verticalVelocity);

        float normalizedSpeed =
    (actualMoveMagnitude > 0.01f)
        ? actualMoveMagnitude / runSpeed
        : 0f;

        animator.SetFloat(SpeedHash, normalizedSpeed, 0.1f, Time.deltaTime);
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
        AudioManager.instance.play("throw prep");

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
        AudioManager.instance.play("throw item");
        GameObject proj = Instantiate(
            projectilePrefab,
            throwPoint.position,
            Quaternion.identity
        );

        if (proj.TryGetComponent(out Rigidbody rb))
            rb.velocity = GetThrowVelocity();
    }


    public void ReduceConfidence(int value)
    {
        gameManager.TakeHit(value);
    }
}
