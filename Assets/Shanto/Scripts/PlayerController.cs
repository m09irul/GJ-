
using UnityEngine;
public class PlayerController : MonoBehaviour
{

    [SerializeField] Joystick movementJostick;
    [Tooltip("Speed ​​at which the character moves. It is not affected by gravity or jumping.")]
    public float velocity = 5f;
    [Tooltip("This value is added to the speed value while the character is sprinting.")]
    public float skateboardAdittion = 3.5f;
    [Space]
    [Tooltip("Force that pulls the player down. Changing this value causes all movement, jumping and falling to be changed as well.")]
    public float gravity = 9.8f;

    // Player states
    bool isOnSkateboard = false;

    // Inputs
    float inputHorizontal;
    float inputVertical;

    Animator animator;
    CharacterController cc;

    public ManaBar manaBar;

    void HandleManaFinished()
    {
        Debug.Log("Mana finished! Player knows it.");
    }

    void Start()
    {
        AudioManager.instance.play("main");
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        manaBar.OnManaFinished += HandleManaFinished;

        // Message informing the user that they forgot to add an animator
        if (animator == null)
            Debug.LogWarning("Hey buddy, you don't have the Animator component in your player. Without it, the animations won't work.");
    }


    // Update is only being used here to identify keys and trigger animations
    void Update()
    {

        // Input checkers
        inputHorizontal = movementJostick.Horizontal;
        inputVertical = movementJostick.Vertical;

        // Run and Crouch animation
        // If dont have animator component, this block wont run
        if ( cc.isGrounded && animator != null )
        { 
            // Run
            float minimumSpeed = 0.9f;
            animator.SetBool("run", cc.velocity.magnitude > minimumSpeed );

            // Sprint
            // isOnSkateboard = cc.velocity.magnitude > minimumSpeed && inputSprint;
            // animator.SetBool("sprint", isOnSkateboard );

        }
    }


    // With the inputs and animations defined, FixedUpdate is responsible for applying movements and actions to the player
    private void FixedUpdate()
    {
        // Sprinting velocity boost or crounching desacelerate
        float velocityAdittion = 0;
        
        
        if ( isOnSkateboard )
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

        Vector3 moviment = verticalDirection + horizontalDirection;
        cc.Move( moviment );

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("mana"))
        {
            manaBar.Activate();
            Destroy(other.gameObject);   // pickup disappears
        }
    }

}
