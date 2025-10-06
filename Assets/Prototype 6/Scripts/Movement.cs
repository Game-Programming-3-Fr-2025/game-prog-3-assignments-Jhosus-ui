using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float jumpForce = 14f;
    public float jumpTime = 0.35f;
    public float cutJumpHeight = 0.5f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    // Component References
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // State Variables
    private float horizontalInput;
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isJumping;
    private float jumpTimeCounter;

    // Layer mask for ground (automatically set to "Ground" layer)
    private int groundLayerMask;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Auto-configure ground layer mask
        groundLayerMask = 1 << LayerMask.NameToLayer("Ground");

        // Auto-create groundCheck if missing
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }

        // Configure Rigidbody for better physics
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 4f;
        }
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

        HandleJumpInput();
        HandleJumpHold();
        HandleFlip();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        // Movement physics in FixedUpdate
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    void HandleJumpInput()
    {
        // Start jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Variable jump height - release jump early to jump lower
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * cutJumpHeight);
            isJumping = false;
        }
    }

    void HandleJumpHold()
    {
        if (isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                // Maintain jump height while button is held
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        // Reset jumping when grounded
        if (isGrounded)
        {
            isJumping = false;
        }
    }

    void HandleFlip()
    {
        if ((horizontalInput > 0 && !isFacingRight) || (horizontalInput < 0 && isFacingRight))
        {
            isFacingRight = !isFacingRight;
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", Mathf.Abs(horizontalInput) > 0.01f);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("YVelocity", rb.linearVelocity.y);

            // Additional animation triggers for better control
            if (isGrounded)
            {
                if (Mathf.Abs(horizontalInput) > 0.01f)
                    animator.SetTrigger("Run");
                else
                    animator.SetTrigger("Idle");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}