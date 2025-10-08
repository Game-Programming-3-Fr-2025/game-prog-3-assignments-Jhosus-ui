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
    public float doubleJumpForceMultiplier = 0.8f; // 80% del salto normal

    [Header("Wall Settings")]
    public float wallSlideSpeed = 2f;
    public float wallJumpForce = 15f;
    public Vector2 wallJumpDirection = new Vector2(1.2f, 1.5f);
    public float wallCheckDistance = 0.6f;

    private Movement movement;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer; // ← AGREGAR REFERENCIA
    private LayerMask wallLayer;

    private bool isDashing = false;
    private float dashTimeLeft;
    private float lastDashTime = -999f;
    private bool canDoubleJump = true;
    private bool isWallSliding = false;
    private bool isTouchingWall = false;
    private bool isOnRightWall = false; // ← NUEVA VARIABLE
    private bool isOnLeftWall = false;  // ← NUEVA VARIABLE

    void Start()
    {
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // ← OBTENER REFERENCIA
        wallLayer = 1 << LayerMask.NameToLayer("Wall");
    }

    void Update()
    {
        if (hasDash) HandleDash();
        if (hasDoubleJump) HandleDoubleJump();
        if (hasWallClimb) HandleWallClimb();
    }

    void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + dashCooldown && movement.isGrounded)
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

        // Double jump input
        if (Input.GetKeyDown(KeyCode.Space) && !movement.isGrounded && canDoubleJump && !isWallSliding)
        {
            // Aplicar salto con porcentaje de fuerza
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Reset Y velocity
            float doubleJumpForce = movement.jumpForce * doubleJumpForceMultiplier;
            rb.AddForce(Vector2.up * doubleJumpForce, ForceMode2D.Impulse);

            canDoubleJump = false;
            if (animator != null) animator.SetTrigger("DoubleJump");
        }
    }

    void HandleWallClimb()
    {
        // Detectar paredes a ambos lados usando la capa Wall
        Vector2 rightDir = Vector2.right;
        Vector2 leftDir = Vector2.left;

        RaycastHit2D rightHit = Physics2D.Raycast(transform.position, rightDir, wallCheckDistance, wallLayer);
        RaycastHit2D leftHit = Physics2D.Raycast(transform.position, leftDir, wallCheckDistance, wallLayer);

        isOnRightWall = rightHit.collider != null && !movement.isGrounded;
        isOnLeftWall = leftHit.collider != null && !movement.isGrounded;
        isTouchingWall = isOnRightWall || isOnLeftWall;

        // Wall sliding
        if (isTouchingWall && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));

            // ← INVERTIR SPRITE SEGÚN LA PARED
            if (spriteRenderer != null)
            {
                if (isOnRightWall)
                {
                    spriteRenderer.flipX = true; // Mirando hacia la izquierda en pared derecha
                }
                else if (isOnLeftWall)
                {
                    spriteRenderer.flipX = false; // Mirando hacia la derecha en pared izquierda
                }
            }

            if (animator != null) animator.SetBool("IsWallSliding", true);
        }
        else
        {
            isWallSliding = false;
            if (animator != null) animator.SetBool("IsWallSliding", false);

            // ← RESTAURAR FLIP AL SALIR DE LA PARED (opcional)
            // El Movement.cs se encargará del flip normal durante el movimiento
        }

        // Wall jump
        if (isWallSliding && Input.GetKeyDown(KeyCode.Space))
        {
            // Determinar dirección opuesta a la pared
            float wallDirection = isOnRightWall ? -1f : 1f;

            Vector2 jumpDir = new Vector2(wallDirection * wallJumpDirection.x, wallJumpDirection.y);
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(jumpDir * wallJumpForce, ForceMode2D.Impulse);

            // ← RESTAURAR ORIENTACIÓN DESPUÉS DEL WALL JUMP
            if (spriteRenderer != null)
            {
                // Después del wall jump, mirar en la dirección del salto
                spriteRenderer.flipX = (wallDirection > 0) ? false : true;
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

    // Para debug visual
    void OnDrawGizmosSelected()
    {
        if (hasWallClimb)
        {
            Gizmos.color = isOnRightWall ? Color.blue : (isOnLeftWall ? Color.cyan : Color.red);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * wallCheckDistance);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.left * wallCheckDistance);
        }
    }
}