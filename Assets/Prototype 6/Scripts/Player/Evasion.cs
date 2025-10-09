using UnityEngine;

public class Evasion : MonoBehaviour
{
    [Header("Abilities")]
    public bool hasDash = false;
    public bool hasDoubleJump = false;
    public bool hasWallClimb = false;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Double Jump Settings")]
    public float doubleJumpForceMultiplier = 0.7f;

    [Header("Wall Settings")]
    public float wallSlideSpeed = 2f;
    public float wallJumpForce = 15f;
    public Vector2 wallJumpDirection = new Vector2(1, 1.5f);
    public float wallCheckDistance = 0.6f;

    private Movement movement;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private LayerMask wallLayer;

    private bool isDashing = false;
    private float dashTimeLeft;
    private float lastDashTime = -999f;
    private bool canDoubleJump = true;
    private bool isWallSliding = false;
    private bool wasWallSliding = false;

    void Start()
    {
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        wallLayer = LayerMask.GetMask("Wall");
    }

    void Update()
    {
        if (hasDash) HandleDash();
        if (hasDoubleJump) HandleDoubleJump();
        if (hasWallClimb) HandleWallClimb();
    }

    void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + dashCooldown && !isDashing)
        {
            StartDash();
        }

        if (isDashing)
        {
            if (dashTimeLeft > 0)
            {
                float dir = movement.isFacingRight ? 1f : -1f;
                rb.linearVelocity = new Vector2(dir * dashSpeed, 0);
                dashTimeLeft -= Time.deltaTime;
                if (animator != null) animator.SetBool("IsDashing", true);
            }
            else
            {
                EndDash();
            }
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        lastDashTime = Time.time;
        if (movement != null) movement.SetCanMove(false);
    }

    void EndDash()
    {
        isDashing = false;
        if (animator != null) animator.SetBool("IsDashing", false);
        if (movement != null) movement.SetCanMove(true);
    }

    void HandleDoubleJump()
    {
        if (movement.isGrounded)
        {
            canDoubleJump = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) && !movement.isGrounded && canDoubleJump && !isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            float doubleJumpForce = movement.jumpForce * doubleJumpForceMultiplier;
            rb.AddForce(Vector2.up * doubleJumpForce, ForceMode2D.Impulse);

            canDoubleJump = false;
            if (animator != null) animator.SetTrigger("DoubleJump");
        }
    }

    void HandleWallClimb()
    {
        Vector2 checkDirection = movement.isFacingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, checkDirection, wallCheckDistance, wallLayer);

        bool touchingWall = hit.collider != null && !movement.isGrounded;
        bool isOnRightWall = hit.collider != null && checkDirection.x > 0;
        bool isOnLeftWall = hit.collider != null && checkDirection.x < 0;

        wasWallSliding = isWallSliding;

        if (touchingWall && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = isOnLeftWall;
            }

            if (animator != null) animator.SetBool("IsWallSliding", true);
        }
        else
        {
            if (wasWallSliding)
            {
                if (movement != null) movement.RestoreFlipToMovementDirection();
            }

            isWallSliding = false;
            if (animator != null) animator.SetBool("IsWallSliding", false);
        }

        if (isWallSliding && Input.GetKeyDown(KeyCode.Space))
        {
            float wallDirection = isOnRightWall ? -1f : 1f;
            Vector2 jumpDir = new Vector2(wallDirection * wallJumpDirection.x, wallJumpDirection.y);

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(jumpDir * wallJumpForce, ForceMode2D.Impulse);

            movement.isFacingRight = wallDirection > 0;
            if (movement != null) movement.RestoreFlipToMovementDirection();

            canDoubleJump = true;
            ForceExitWallSlide();

            if (animator != null) animator.SetTrigger("WallJump");
        }
    }

    public void ForceExitWallSlide()
    {
        if (isWallSliding)
        {
            isWallSliding = false;
            if (movement != null) movement.RestoreFlipToMovementDirection();
            if (animator != null) animator.SetBool("IsWallSliding", false);
        }
    }

    public void UnlockDash() => hasDash = true;
    public void UnlockDoubleJump() => hasDoubleJump = true;
    public void UnlockWallClimb() => hasWallClimb = true;
    public bool IsDashing() => isDashing;
    public bool IsWallSliding() => isWallSliding;

    void OnDrawGizmosSelected()
    {
        if (hasWallClimb && movement != null)
        {
            Vector2 dir = movement.isFacingRight ? Vector2.right : Vector2.left;
            Gizmos.color = isWallSliding ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * wallCheckDistance);
        }
    }
}