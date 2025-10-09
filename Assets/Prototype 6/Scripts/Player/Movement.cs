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

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Combate combate;
    private Evasion evasion;

    private float horizontalInput;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isFacingRight = true;
    private bool isJumping;
    private float jumpTimeCounter;
    private bool canMove = true;

    private int groundLayerMask;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        combate = GetComponent<Combate>();
        evasion = GetComponent<Evasion>();

        groundLayerMask = 1 << LayerMask.NameToLayer("Ground");

        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 4f;
        }
    }

    void Update()
    {
        horizontalInput = canMove ? Input.GetAxisRaw("Horizontal") : 0;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

        if (canMove)
        {
            HandleJumpInput();
            HandleJumpHold();
            HandleFlip();
        }

        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * cutJumpHeight);
            isJumping = false;
        }
    }

    public void Jump()
    {
        isJumping = true;
        jumpTimeCounter = jumpTime;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void HandleJumpHold() //permitir salto sostenido
    {
        if (isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        if (isGrounded)
        {
            isJumping = false;
        }
    }

    void HandleFlip() //girar sprite
    {
        if (evasion != null && (evasion.IsWallSliding() || evasion.IsDashing()))
        {
            return;
        }

        if ((horizontalInput > 0 && !isFacingRight) || (horizontalInput < 0 && isFacingRight))
        {
            isFacingRight = !isFacingRight;
            spriteRenderer.flipX = !isFacingRight;
        }
    }

    public void RestoreFlipToMovementDirection()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !isFacingRight;
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("YVelocity", rb.linearVelocity.y);

        bool isAttacking = combate != null && combate.IsAttacking();
        if (!isAttacking)
        {
            animator.SetBool("IsWalking", Mathf.Abs(horizontalInput) > 0.01f);
        }
    }

    public void SetCanMove(bool state)
    {
        canMove = state;
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