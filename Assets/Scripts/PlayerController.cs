using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject healthBarGameObj;
    [SerializeField] private int confidence = 100;
    [SerializeField] private Joystick movementJostick;

    [Header("Movement")]
    public float velocity = 5f;
    public float gravity = 12f;

    private float inputHorizontal;
    private float inputVertical;

    [SerializeField] private Animator animator;
    public CharacterController cc;
    private float verticalVelocity;
    private GameManager gameManager;
    private UIManager uIManager;

    [Header("Throw")]
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float arcForce = 4f;

    [Header("Throw Preview")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPoints = 20;
    [SerializeField] private float trajectoryTimeStep = 0.1f;

    private bool isPreviewingThrow;


    public float jumpForce = 6.5f;
    public WalkableArea walkableArea;

    // --------------------------------------------------

    void Start()
    {
        gameManager = GameManager.Instance;
        uIManager = UIManager.Instance;

        AudioManager.instance.play("main");

        if (animator == null)
            Debug.LogWarning("Animator missing on PlayerController");
    }

    void Update()
    {
        // inputHorizontal = movementJostick.Horizontal;
        // inputVertical = movementJostick.Vertical;

        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");

        bool hasMoveInput =
            Mathf.Abs(inputHorizontal) > 0.05f ||
            Mathf.Abs(inputVertical) > 0.05f;

        if (animator != null)
            animator.SetBool("run", hasMoveInput && cc.isGrounded);
        // ---- JUMP ----
        if (cc.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = 0;

            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = jumpForce;
            }
        }

        if (isPreviewingThrow)
            DrawTrajectory();
    }

    void FixedUpdate()
    {
        Vector3 inputMove =
            CameraForwardFlat() * inputVertical +
            CameraRightFlat() * inputHorizontal;

        inputMove *= velocity;

        // gravity
        verticalVelocity -= gravity * Time.fixedDeltaTime;

        Vector3 finalMove =
            inputMove * Time.fixedDeltaTime +
            Vector3.up * verticalVelocity * Time.fixedDeltaTime;

        cc.Move(finalMove);

        if (walkableArea)
        {
            Vector3 clamped = walkableArea.ClampPoint(transform.position);
            transform.position = clamped;
        }
        RotateTowardsMovement(inputMove);
    }

    // --------------------------------------------------
    // RAIL CLAMP
    // --------------------------------------------------


    // --------------------------------------------------
    // ROTATION
    // --------------------------------------------------

    void RotateTowardsMovement(Vector3 desiredMove)
    {
        Vector3 flat = desiredMove;
        flat.y = 0;

        if (flat.sqrMagnitude < 0.001f)
            return;

        float angle = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;
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
        Vector3 dir = throwPoint.forward;
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
