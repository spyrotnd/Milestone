using UnityEngine;
using UnityEngine.InputSystem;

// comments labeled in explaining rationale

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.2f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float crouchSpeed = 1.4f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float turnSpeed = 720f;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float fallGravityMultiplier = 2f;   // fall faster than you rise, so jumps don't feel floaty
    [SerializeField] private float coyoteTime = 0.1f;            // can still jump just after walking off a ledge
    [SerializeField] private float jumpBufferTime = 0.1f;        // a press just before landing still counts

    [Header("Ground check")]
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private float groundCheckDistance = 0.35f;

    [Header("Crouching")]
    [SerializeField] private float standingHeight = 1.9f;
    [SerializeField] private float crouchHeight = 1.2f;

    private Rigidbody rb;
    private Animator animator;
    private BoxCollider box;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction jumpAction;
    private InputAction crouchAction;

    private Vector3 moveDirection;
    private float currentSpeed;
    private float targetSpeed;

    private bool isGrounded = true;
    private bool isCrouching;
    private float lastGroundedTime = -10f;
    private float lastJumpPressedTime = -10f;
    private float jumpStartTime = -10f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        box = GetComponent<BoxCollider>();

        // Actions come from PlayerControls.inputactions via the Player Input component
        PlayerInput playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        sprintAction = playerInput.actions["Sprint"];
        jumpAction = playerInput.actions["Jump"];
        crouchAction = playerInput.actions["Crouch"];

        // Prevent the player from tipping over
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Keyboard gives 0 or 1, a gamepad stick anything in between
        Vector2 input = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        moveDirection = new Vector3(input.x, 0f, input.y);

        if (jumpAction.WasPressedThisFrame())
            lastJumpPressedTime = Time.time;

        // Hold to crouch, only while on the ground
        isCrouching = crouchAction.IsPressed() && isGrounded;
        SetColliderHeight(isCrouching ? crouchHeight : standingHeight);

        bool sprinting = sprintAction.IsPressed() && !isCrouching;
        float maxSpeed = isCrouching ? crouchSpeed : (sprinting ? runSpeed : walkSpeed);
        targetSpeed = input.magnitude * maxSpeed;

        // Blend trees: 0 = idle, 0.5 = walk (or crouch walk), 1 = run
        float blend = sprinting ? input.magnitude : input.magnitude * 0.5f;
        animator.SetFloat("MoveX", 0f);
        animator.SetFloat("MoveZ", blend, 0.1f, Time.deltaTime);
        animator.SetBool("Crouch", isCrouching);
        animator.SetBool("Grounded", isGrounded);
    }

    void FixedUpdate()
    {
        CheckGround();
        TryJump();

        // Ease toward the target speed so starts and stops feel weighty but snappy
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);

        Vector3 direction = moveDirection.sqrMagnitude > 0.01f ? moveDirection.normalized : transform.forward;
        rb.MovePosition(rb.position + direction * currentSpeed * Time.fixedDeltaTime);

        // Turn to face the direction we're moving
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, target, turnSpeed * Time.fixedDeltaTime));
        }

        // Extra gravity on the way down
        if (!isGrounded && rb.linearVelocity.y < 0f)
            rb.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
    }

    private void CheckGround()
    {
        // Ignore the ground for a moment after jumping so we don't count as landed on take off
        if (Time.time - jumpStartTime < 0.15f)
        {
            isGrounded = false;
            return;
        }

        // Sphere cast down from just above the feet. It starts inside our own collider, which casts ignore.
        Vector3 origin = transform.position + Vector3.up * (groundCheckRadius + 0.05f);
        isGrounded = Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out _,
            groundCheckDistance, ~0, QueryTriggerInteraction.Ignore);

        if (isGrounded)
            lastGroundedTime = Time.time;
    }

    private void TryJump()
    {
        bool jumpBuffered = Time.time - lastJumpPressedTime <= jumpBufferTime;
        bool canJump = Time.time - lastGroundedTime <= coyoteTime;

        if (!jumpBuffered || !canJump)
            return;

        // Velocity needed to reach jumpHeight: v = sqrt(2 * g * h)
        float jumpVelocity = Mathf.Sqrt(2f * -Physics.gravity.y * jumpHeight);
        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(v.x, jumpVelocity, v.z);

        jumpStartTime = Time.time;
        lastJumpPressedTime = -10f;
        lastGroundedTime = -10f;
        isGrounded = false;
        animator.SetTrigger("Jump");
    }

    private void SetColliderHeight(float height)
    {
        box.size = new Vector3(box.size.x, height, box.size.z);
        box.center = new Vector3(box.center.x, height / 2f, box.center.z);
    }
}
