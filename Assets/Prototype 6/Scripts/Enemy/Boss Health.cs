using UnityEngine;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 1000f;

    [Header("Damage Settings")]
    public int damageToPlayer = 2;
    public float damageRange = 1.5f;
    public float damageInterval = 1f;
    public Vector2 damageOffset = Vector2.zero; // NUEVO: Offset para el daño

    [Header("Phase Settings")]
    public float phase1Threshold = 0.6f;
    public float phase2Threshold = 0.3f;

    [Header("Movement Settings")]
    public float moveSpeed_F1 = 4.0f;
    public float moveSpeed_F2_F3 = 7.0f;
    public float meleeRange = 1.5f;
    public float jumpDetectRange = 4.0f;
    public float jumpForce_F2 = 10f;
    public float activationRange = 15f;
    public float idleTimeout = 3f; // NUEVO: Timeout para idle

    [Header("Attack Settings")]
    public float attackCooldown = 0.8f;
    public int meleeDamage = 30;
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;
    public float fanShotInterval = 15f;

    // Components
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private LayerMask playerLayer;
    private LayerMask groundLayer;

    // State Management
    private enum BossState { IDLE, CHASE, MELEE_ATTACK, JUMP_ATTACK, STUN, COOLDOWN, PROJECTILE_ATTACK }
    private BossState currentState;
    private int currentPhase = 1;

    // Health & Timers
    private float currentHealth;
    private float lastDamageTime = -999f;
    private float cooldownTimer;
    private float projectileTimer;
    private float fanShotTimer;
    private float idleTimer; // NUEVO: Timer para idle
    private bool isJumping = false;
    private Vector2 jumpTarget;

    // Ground Detection
    private bool isGrounded;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    void Start()
    {
        currentHealth = maxHealth;
        playerLayer = LayerMask.GetMask("Player");
        groundLayer = LayerMask.GetMask("Ground");

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        currentState = BossState.IDLE;
        fanShotTimer = fanShotInterval;
        projectileTimer = 2f; // Iniciar timer de proyectiles
        idleTimer = idleTimeout;
    }

    void Update()
    {
        if (player == null) return;

        if (Time.time >= lastDamageTime + damageInterval)
        {
            CheckForPlayerContact();
        }

        UpdatePhase();
        UpdateStateMachine();
        UpdateAnimations();
        FlipSprite();
        UpdateTimers(); // NUEVO: Actualizar timers

        // Timer para disparos en abanico (Fase 3)
        if (currentPhase == 3)
        {
            fanShotTimer -= Time.deltaTime;
        }
    }

    // NUEVO: Método para actualizar timers importantes
    void UpdateTimers()
    {
        if (projectileTimer > 0) projectileTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        UpdateGroundDetection();
    }

    void UpdateGroundDetection()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            isGrounded = Physics2D.OverlapCircle(transform.position, groundCheckRadius, groundLayer);
        }
    }

    void CheckForPlayerContact()
    {
        // NUEVO: Usar offset para el daño
        Vector2 damageCenter = (Vector2)transform.position + damageOffset;
        Collider2D playerCollider = Physics2D.OverlapCircle(damageCenter, damageRange, playerLayer);

        if (playerCollider != null)
        {
            HealthP6 playerHealth = playerCollider.GetComponent<HealthP6>();
            if (playerHealth != null && playerHealth.IsAlive())
            {
                playerHealth.TakeDamage(damageToPlayer);
                lastDamageTime = Time.time;
            }
        }
    }

    void UpdatePhase()
    {
        float healthPercent = currentHealth / maxHealth;

        if (currentPhase == 1 && healthPercent < phase1Threshold)
        {
            StartPhaseTransition(2);
        }
        else if (currentPhase == 2 && healthPercent < phase2Threshold)
        {
            StartPhaseTransition(3);
        }
    }

    void StartPhaseTransition(int newPhase)
    {
        currentPhase = newPhase;
        currentState = BossState.STUN;
        cooldownTimer = 1f;
        animator.SetTrigger("Hit");

        if (player != null)
        {
            Vector2 knockbackDir = (transform.position - player.position).normalized;
            rb.linearVelocity = new Vector2(knockbackDir.x * 3f, rb.linearVelocity.y);
        }
    }

    void UpdateStateMachine()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case BossState.IDLE:
                IdleBehavior(distanceToPlayer);
                break;

            case BossState.CHASE:
                ChaseBehavior(distanceToPlayer);
                break;

            case BossState.MELEE_ATTACK:
                MeleeAttackBehavior(); // NUEVO: Comportamiento para melee
                break;

            case BossState.JUMP_ATTACK:
                JumpAttackBehavior();
                break;

            case BossState.STUN:
                StunBehavior();
                break;

            case BossState.COOLDOWN:
                CooldownBehavior();
                break;

            case BossState.PROJECTILE_ATTACK:
                ProjectileAttackBehavior(); // NUEVO: Comportamiento para proyectiles
                break;
        }
    }

    // NUEVO: Comportamiento para ataque melee
    void MeleeAttackBehavior()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0)
        {
            TransitionToState(BossState.COOLDOWN);
        }
    }

    // NUEVO: Comportamiento para ataque con proyectiles
    void ProjectileAttackBehavior()
    {
        // Este estado ahora se maneja principalmente por la corrutina
        // Pero añadimos un timeout por seguridad
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= -1f) // Timeout de seguridad
        {
            TransitionToState(BossState.COOLDOWN);
        }
    }

    void IdleBehavior(float distance)
    {
        // Timeout para idle - si pasa mucho tiempo en idle, volver a chase
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            TransitionToState(BossState.CHASE);
            return;
        }

        // Solo activarse si el player está en rango
        if (distance < activationRange)
        {
            TransitionToState(BossState.CHASE);
        }
    }

    void ChaseBehavior(float distance)
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float currentSpeed = (currentPhase == 1) ? moveSpeed_F1 : moveSpeed_F2_F3;

        rb.linearVelocity = new Vector2(direction.x * currentSpeed, rb.linearVelocity.y);

        if (distance < meleeRange)
        {
            TransitionToState(BossState.MELEE_ATTACK);
        }
        else if (currentPhase >= 2 && CheckJumpCondition(distance))
        {
            TransitionToState(BossState.JUMP_ATTACK);
        }
        else if (currentPhase == 3 && projectileTimer <= 0)
        {
            TransitionToState(BossState.PROJECTILE_ATTACK);
        }
    }

    bool CheckJumpCondition(float distance)
    {
        bool isPlayerHigher = player.position.y > transform.position.y + 0.5f;
        return distance < jumpDetectRange && isPlayerHigher && isGrounded;
    }

    void JumpAttackBehavior()
    {
        if (!isJumping)
        {
            isJumping = true;
            StartCoroutine(JumpAttackRoutine());
            return;
        }

        if (isGrounded && isJumping)
        {
            isJumping = false;
            TransitionToState(BossState.COOLDOWN);
        }
    }

    IEnumerator JumpAttackRoutine()
    {
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.2f);

        if (player != null)
        {
            jumpTarget = player.position;
            Vector2 jumpDirection = (jumpTarget - (Vector2)transform.position).normalized;
            rb.linearVelocity = new Vector2(jumpDirection.x * moveSpeed_F2_F3, jumpForce_F2);
            animator.SetTrigger("Attack");
        }
    }

    void StunBehavior()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.9f, rb.linearVelocity.y);
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0)
        {
            BossState nextState = (currentPhase == 1) ? BossState.IDLE : BossState.CHASE;
            TransitionToState(nextState);
        }
    }

    void CooldownBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0)
        {
            TransitionToState(BossState.CHASE);
        }
    }

    void TransitionToState(BossState newState)
    {
        // Reset idle timer cuando salimos de idle
        if (currentState == BossState.IDLE && newState != BossState.IDLE)
        {
            idleTimer = idleTimeout;
        }

        // Detener movimiento al cambiar de estado
        if (newState != BossState.CHASE && newState != BossState.JUMP_ATTACK)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        currentState = newState;

        switch (newState)
        {
            case BossState.IDLE:
                idleTimer = idleTimeout;
                break;

            case BossState.MELEE_ATTACK:
                animator.SetTrigger("Attack");
                cooldownTimer = attackCooldown;
                break;

            case BossState.JUMP_ATTACK:
                cooldownTimer = 0.2f;
                break;

            case BossState.PROJECTILE_ATTACK:
                animator.SetTrigger("Attack");
                StartCoroutine(ProjectileAttackRoutine());
                cooldownTimer = 2f; // Timeout de seguridad
                break;

            case BossState.COOLDOWN:
                cooldownTimer = attackCooldown;
                break;

            case BossState.STUN:
                animator.SetTrigger("Hit");
                cooldownTimer = 0.5f;
                break;
        }
    }

    IEnumerator ProjectileAttackRoutine()
    {
        if (fanShotTimer <= 0)
        {
            ShootFanPattern(8);
            fanShotTimer = fanShotInterval;
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                ShootProjectileAtPlayer();
                yield return new WaitForSeconds(0.3f);
            }
        }

        projectileTimer = 3f;
        // La transición a COOLDOWN ahora se hace en OnAttackAnimationEnd
    }

    void ShootProjectileAtPlayer()
    {
        if (projectilePrefab == null || player == null) return;

        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Vector2 direction = (player.position - transform.position).normalized;

        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.linearVelocity = direction * projectileSpeed;
        }
    }

    void ShootFanPattern(int projectileCount)
    {
        if (projectilePrefab == null) return;

        float angleStep = 360f / projectileCount;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = direction * projectileSpeed;
            }
        }
    }

    void UpdateAnimations()
    {
        animator.SetBool("IsRunning", currentState == BossState.CHASE && Mathf.Abs(rb.linearVelocity.x) > 0.1f);
    }

    void FlipSprite()
    {
        if (player != null && currentState != BossState.MELEE_ATTACK && currentState != BossState.STUN)
        {
            spriteRenderer.flipX = player.position.x < transform.position.x;
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentState == BossState.STUN) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (currentState != BossState.STUN)
            {
                TransitionToState(BossState.STUN);

                if (player != null)
                {
                    Vector2 knockbackDir = (transform.position - player.position).normalized;
                    if (currentPhase == 1)
                    {
                        rb.AddForce(new Vector2(knockbackDir.x * 5f, 0), ForceMode2D.Impulse);
                    }
                    else
                    {
                        rb.AddForce(knockbackDir * 5f, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }

    void Die()
    {
        animator.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        enabled = false;
        Destroy(gameObject, 2f);
    }

    // ANIMATION EVENTS
    public void OnMeleeAttackHit()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, meleeRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                HealthP6 playerHealth = hit.GetComponent<HealthP6>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(meleeDamage);
                }
            }
        }
    }

    public void OnAttackAnimationEnd()
    {
        if (currentState == BossState.MELEE_ATTACK || currentState == BossState.PROJECTILE_ATTACK)
        {
            TransitionToState(BossState.COOLDOWN);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Gizmo para daño al player con offset
        Gizmos.color = Color.red;
        Vector2 damageCenter = (Vector2)transform.position + damageOffset;
        Gizmos.DrawWireSphere(damageCenter, damageRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, jumpDetectRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        // Gizmo para detección de suelo
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // NUEVO: Mostrar el punto de offset de daño
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(damageCenter, Vector3.one * 0.2f);
    }
}