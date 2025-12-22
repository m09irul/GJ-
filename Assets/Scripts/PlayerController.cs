
using UnityEngine;
using System.Collections;
public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject healthBarGameObj;
    [SerializeField] private int confidence = 100;
    [SerializeField] Joystick movementJostick;
    [Tooltip("Speed ​​at which the character moves. It is not affected by gravity or jumping.")]
    public float velocity = 5f;
    [Tooltip("This value is added to the speed value while the character is sprinting.")]
    public float skateboardAdittion = 3.5f;
    [Space]
    [Tooltip("Force that pulls the player down. Changing this value causes all movement, jumping and falling to be changed as well.")]
    public float gravity = 12f;
    // Player states
    bool isOnSkateboard = false;

    // Inputs
    float inputHorizontal;
    float inputVertical;

    [SerializeField] Animator animator;
    public CharacterController cc;
    GameManager gameManager;
    UIManager uIManager;

    public ManaBar manaBar;
    SegmentedBarUI confidenceBar;
    [Header("Throw")]
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float minThrowInput = 0.15f;

    private Vector3 lastThrowDirection = Vector3.forward;
    void HandleManaFinished()
    {
        Debug.Log("Mana finished! Player knows it.");
    }


    void DrawTrajectory()
    {
        if (!throwPoint) return;

        Vector3 startPos = throwPoint.position;

        Vector3 velocity =
            lastThrowDirection * throwForce +
            Vector3.up * arcForce;

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
    void Start()
    {
        gameManager = GameManager.Instance;
        uIManager = UIManager.Instance;

        confidenceBar = uIManager.confidenceBar.GetComponent<SegmentedBarUI>();

        AudioManager.instance.play("main");
        manaBar.OnManaFinished += HandleManaFinished;

        // Message informing the user that they forgot to add an animator
        if (animator == null)
            Debug.LogWarning("Hey buddy, you don't have the Animator component in your player. Without it, the animations won't work.");
    }
[Header("Throw Preview")]
[SerializeField] private LineRenderer trajectoryLine;
[SerializeField] private int trajectoryPoints = 20;
[SerializeField] private float trajectoryTimeStep = 0.1f;

[SerializeField] private float throwForce = 10f;
[SerializeField] private float arcForce = 4f;

private bool isPreviewingThrow;
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
    // Update is only being used here to identify keys and trigger animations
    void Update()
    {
        // Input checkers
        inputHorizontal = movementJostick.Horizontal;
        inputVertical = movementJostick.Vertical;
        if (isPreviewingThrow)
            DrawTrajectory();
        // Run and Crouch animation
        // If dont have animator component, this block wont run
        if (cc.isGrounded && animator != null)
        {
            Debug.Log(cc.velocity.magnitude);
            animator.SetBool("run", cc.velocity.magnitude > 0.01f);

        }
    }


    // With the inputs and animations defined, FixedUpdate is responsible for applying movements and actions to the player
    private void FixedUpdate()
    {
        // Sprinting velocity boost or crounching desacelerate
        float velocityAdittion = 0;


        if (isOnSkateboard)
            velocityAdittion = skateboardAdittion;

        // Direction movement
        float directionX = inputHorizontal * (velocity + velocityAdittion) * Time.deltaTime;
        float directionZ = inputVertical * (velocity + velocityAdittion) * Time.deltaTime;
        float directionY = 0;

        // Add gravity to Y axis
        directionY = directionY - gravity * Time.deltaTime;

        // --- Character rotation --- 

        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        // Relate the front with the Z direction (depth) and right with X (lateral movement)
        forward = forward * directionZ;
        right = right * directionX;

        if (directionX != 0 || directionZ != 0)
        {
            float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
        }

        // --- End rotation ---


        Vector3 verticalDirection = Vector3.up * directionY;
        Vector3 horizontalDirection = forward + right;


        if (horizontalDirection.magnitude > minThrowInput)
        {
            lastThrowDirection = horizontalDirection.normalized;
        }


        Vector3 moviment = verticalDirection + horizontalDirection;
        cc.Move(moviment);

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("mana"))
        {
            manaBar.Activate();
            Destroy(other.gameObject);   // pickup disappears
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
    public void ThrowItem(GameObject projectilePrefab)
    {
        if (!projectilePrefab || !throwPoint) return;

        GameObject proj = Instantiate(
            projectilePrefab,
            throwPoint.position,
            Quaternion.identity
        );

        ThrowableItem throwable = proj.GetComponent<ThrowableItem>();
        if (throwable != null)
        {
            throwable.Throw(lastThrowDirection);
        }
    }
}
