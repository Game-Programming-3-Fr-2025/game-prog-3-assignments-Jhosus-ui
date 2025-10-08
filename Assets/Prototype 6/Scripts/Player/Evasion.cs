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
    public float doubleJumpForceMultiplier = 0.7f; // ← 70% del salto normal (como Hollow Knight)

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
    private bool canDoubleJump = true; // ← Cambiar de jumpsLeft a canDoubleJump
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
        // Reset double jump cuando toca el suelo
        if (movement.isGrounded)
        {
            canDoubleJump = true;
        }

        // Double jump input - con porcentaje de fuerza reducida
        if (Input.GetKeyDown(KeyCode.Space) && !movement.isGrounded && canDoubleJump && !isWallSliding)
        {
            // ✅ APLICAR SALTO CON PORCENTAJE REDUCIDO (como Hollow Knight)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Reset Y velocity
            float doubleJumpForce = movement.jumpForce * doubleJumpForceMultiplier;
            rb.AddForce(Vector2.up * doubleJumpForce, ForceMode2D.Impulse);

            canDoubleJump = false;
            if (animator != null) animator.SetTrigger("DoubleJump");

            Debug.Log($"Double Jump! Fuerza: {doubleJumpForce} ({doubleJumpForceMultiplier * 100}% del salto normal)");
        }
    }

    void HandleWallClimb()
    {
        Vector2 dir = movement.isFacingRight ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, wallCheckDistance, wallLayer);

        bool touchingWall = hit.collider != null && !movement.isGrounded;

        // Guardar estado anterior
        wasWallSliding = isWallSliding;

        if (touchingWall && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));

            // VOLTEAR SPRITE para mirar hacia la pared
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = movement.isFacingRight;
            }

            if (animator != null) animator.SetBool("IsWallSliding", true);
        }
        else
        {
            // RESTAURAR FLIP al salir de la pared
            if (wasWallSliding && !isWallSliding && spriteRenderer != null)
            {
                spriteRenderer.flipX = !movement.isFacingRight;
            }

            isWallSliding = false;
            if (animator != null) animator.SetBool("IsWallSliding", false);
        }

        // Wall jump
        if (isWallSliding && Input.GetKeyDown(KeyCode.Space))
        {
            Vector2 jumpDir = new Vector2(-dir.x * wallJumpDirection.x, wallJumpDirection.y);
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(jumpDir * wallJumpForce, ForceMode2D.Impulse);

            // Actualizar dirección del personaje después del wall jump
            movement.isFacingRight = dir.x < 0;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !movement.isFacingRight;
            }

            // Reset double jump después del wall jump
            canDoubleJump = true;

            if (animator != null) animator.SetTrigger("WallJump");
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