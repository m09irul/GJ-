using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject healthBarGameObj;
    [SerializeField] private int confidence = 100;
    [SerializeField] private Joystick movementJostick;

    [Header("Movement")]
    public float velocity = 5f;
    public float skateboardAdittion = 3.5f;
    public float gravity = 12f;

    private bool isOnSkateboard;

    private float inputHorizontal;
    private float inputVertical;

    [SerializeField] private Animator animator;
    public CharacterController cc;

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

        if (isPreviewingThrow)
            DrawTrajectory();

        if (cc.isGrounded && animator != null)
        {
            animator.SetBool("run", cc.velocity.magnitude > 0.01f);
        }
    }

    void FixedUpdate()
    {
        float velocityAddition = isOnSkateboard ? skateboardAdittion : 0f;

        float directionX = inputHorizontal * (velocity + velocityAddition) * Time.deltaTime;
        float directionZ = inputVertical * (velocity + velocityAddition) * Time.deltaTime;
        float directionY = -gravity * Time.deltaTime;

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * directionZ + camRight * directionX;

        // Rotate player towards movement direction
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 0.15f);
        }

        Vector3 movement = moveDir + Vector3.up * directionY;
        cc.Move(movement);
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
        if (other.CompareTag("mana"))
        {
            Destroy(other.gameObject);
        }

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
