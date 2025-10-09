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
    public Vector2 damageOffset = Vector2.zero;

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
    public float idleTimeout = 3f;

    [Header("Attack Settings")]
    public float attackCooldown = 0.8f;
    public int meleeDamage = 30;
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;
    public float projectileCooldown = 5f;
    public float fanShotInterval = 15f;
    public float jumpAttackChance = 0.2f; // Reducido para menos saltos aleatorios

    [Header("Flip Settings")]
    public float flipCooldown = 0.3f; // NUEVO: Cooldown para voltear

    [Header("Gizmos Offsets")]
    public Vector2 meleeOffset = Vector2.zero;
    public Vector2 jumpDetectOffset = Vector2.zero;
    public Vector2 activationOffset = Vector2.zero;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Visual Feedback")]
    public float blinkDuration = 0.1f;
    public int blinkCount = 3;

    // Components
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private LayerMask playerLayer;
    private LayerMask groundLayer;

    // State
    private enum BossState { IDLE, CHASE, MELEE_ATTACK, JUMP_ATTACK, STUN, COOLDOWN, PROJECTILE_ATTACK }
    private BossState currentState;
    private int currentPhase = 1;

    // Health & Timers
    private float currentHealth;
    private float lastDamageTime = -999f;
    private float cooldownTimer;
    private float projectileTimer;
    private float fanShotTimer;
    private float idleTimer;
    private float lastFlipTime = -999f; // NUEVO: Control de flip
    private bool isJumping = false;
    private bool hasLandedAfterJump = false; // NUEVO: Evitar saltos repetidos
    private bool isGrounded;
    private bool wasGrounded; // NUEVO: Detectar aterrizaje
    private bool isActive = false;

    // Variable para trackear dirección
    private bool isFacingRight = true;

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
        projectileTimer = projectileCooldown;
        idleTimer = idleTimeout;

        // Auto-crear groundCheck si no existe
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = gc.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        UpdateGroundDetection();

        if (Time.time >= lastDamageTime + damageInterval)
        {
            CheckForPlayerContact();
        }

        UpdatePhase();

        // Solo actualizar state machine si está activo
        if (isActive)
        {
            UpdateStateMachine();
        }
        else
        {
            CheckActivation();
        }

        UpdateAnimations();
        FlipSprite();
        UpdateTimers();
    }

    void CheckActivation()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= activationRange)
        {
            isActive = true;
            TransitionToState(BossState.CHASE);
        }
    }

    void UpdateTimers()
    {
        if (projectileTimer > 0) projectileTimer -= Time.deltaTime;
        if (fanShotTimer > 0 && currentPhase == 3) fanShotTimer -= Time.deltaTime;
    }

    void UpdateGroundDetection()
    {
        wasGrounded = isGrounded;
        Vector2 checkPos = groundCheck != null ? groundCheck.position : transform.position;
        isGrounded = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer);

        // Detectar aterrizaje después de salto
        if (!wasGrounded && isGrounded && isJumping)
        {
            hasLandedAfterJump = true;
        }
    }

    void CheckForPlayerContact()
    {
        Vector2 damageCenter = GetFlippedPosition(damageOffset);
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

    // Método para obtener posición con offset flipado
    Vector2 GetFlippedPosition(Vector2 offset)
    {
        Vector2 flippedOffset = offset;
        if (!isFacingRight)
        {
            flippedOffset.x = -offset.x;
        }
        return (Vector2)transform.position + flippedOffset;
    }

    void UpdatePhase()
    {
        float healthPercent = currentHealth / maxHealth;

        if (currentPhase == 1 && healthPercent <= phase1Threshold)
        {
            StartPhaseTransition(2);
        }
        else if (currentPhase == 2 && healthPercent <= phase2Threshold)
        {
            StartPhaseTransition(3);
        }
    }

    void StartPhaseTransition(int newPhase)
    {
        currentPhase = newPhase;
        currentState = BossState.STUN;
        cooldownTimer = 1.5f;

        StartCoroutine(BlinkEffect());

        if (player != null)
        {
            Vector2 knockbackDir = (transform.position - player.position).normalized;
            rb.linearVelocity = new Vector2(knockbackDir.x * 5f, 3f);
        }
    }

    IEnumerator BlinkEffect()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 0.7f);
            yield return new WaitForSeconds(blinkDuration);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(blinkDuration);
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
                MeleeAttackBehavior();
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
                ProjectileAttackBehavior();
                break;
        }
    }

    void IdleBehavior(float distance)
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0 || distance < activationRange)
        {
            TransitionToState(BossState.CHASE);
        }
    }

    void ChaseBehavior(float distance)
    {
        float currentSpeed = (currentPhase == 1) ? moveSpeed_F1 : moveSpeed_F2_F3;
        Vector2 direction = (player.position - transform.position).normalized;

        rb.linearVelocity = new Vector2(direction.x * currentSpeed, rb.linearVelocity.y);

        // Prioridad de ataques
        if (distance < meleeRange)
        {
            TransitionToState(BossState.MELEE_ATTACK);
        }
        else if (currentPhase == 3 && projectileTimer <= 0 && distance < 10f)
        {
            TransitionToState(BossState.PROJECTILE_ATTACK);
        }
        else if (currentPhase >= 2 && CheckJumpCondition(distance))
        {
            TransitionToState(BossState.JUMP_ATTACK);
        }
    }

    bool CheckJumpCondition(float distance)
    {
        // NO saltar si ya estamos saltando o acabamos de aterrizar
        if (isJumping || !isGrounded || !hasLandedAfterJump)
            return false;

        // Condición: player más alto (más tolerante)
        bool isPlayerHigher = player.position.y > transform.position.y + 2f;

        // Salto aleatorio SOLO cuando el player está en el aire o saltando
        bool isPlayerJumping = false;
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            isPlayerJumping = Mathf.Abs(playerRb.linearVelocity.y) > 0.5f;
        }

        bool randomJump = isPlayerJumping && currentPhase >= 2 && Random.value < jumpAttackChance;

        return distance < jumpDetectRange && distance > meleeRange && (isPlayerHigher || randomJump);
    }

    void MeleeAttackBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0)
        {
            TransitionToState(BossState.COOLDOWN);
        }
    }

    void JumpAttackBehavior()
    {
        if (!isJumping)
        {
            isJumping = true;
            hasLandedAfterJump = false; // IMPORTANTE: Reset al empezar salto
            StartCoroutine(JumpAttackRoutine());
        }
        else if (isGrounded && hasLandedAfterJump)
        {
            // Pequeña pausa después de aterrizar antes de continuar persiguiendo
            isJumping = false;
            TransitionToState(BossState.COOLDOWN);
        }
    }

    IEnumerator JumpAttackRoutine()
    {
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.3f);

        if (player != null)
        {
            Vector2 jumpDirection = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(jumpDirection.x * moveSpeed_F2_F3, jumpForce_F2);

            if (animator != null) animator.SetTrigger("Attack");
        }
    }

    void ProjectileAttackBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= -2f)
        {
            TransitionToState(BossState.COOLDOWN);
        }
    }

    void StunBehavior()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.9f, rb.linearVelocity.y);
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0)
        {
            TransitionToState(BossState.CHASE);
        }
    }

    void CooldownBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0)
        {
            hasLandedAfterJump = true; // NUEVO: Reset después de cooldown
            TransitionToState(BossState.CHASE);
        }
    }

    void TransitionToState(BossState newState)
    {
        if (currentState == BossState.IDLE && newState != BossState.IDLE)
        {
            idleTimer = idleTimeout;
        }

        currentState = newState;

        switch (newState)
        {
            case BossState.IDLE:
                idleTimer = idleTimeout;
                break;

            case BossState.MELEE_ATTACK:
                if (animator != null) animator.SetTrigger("Attack");
                cooldownTimer = attackCooldown;
                break;

            case BossState.JUMP_ATTACK:
                cooldownTimer = 1f;
                break;

            case BossState.PROJECTILE_ATTACK:
                if (animator != null) animator.SetTrigger("Attack");
                StartCoroutine(ProjectileAttackRoutine());
                cooldownTimer = 2f;
                break;

            case BossState.COOLDOWN:
                cooldownTimer = attackCooldown * 0.5f; // Cooldown más corto después de ataques
                break;

            case BossState.STUN:
                StartCoroutine(BlinkEffect());
                cooldownTimer = 0.8f;
                break;
        }
    }

    IEnumerator ProjectileAttackRoutine()
    {
        if (fanShotTimer <= 0)
        {
            // Fan Pattern (360 grados)
            ShootFanPattern(8);
            fanShotTimer = fanShotInterval;
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            // Triple disparo hacia el jugador
            for (int i = 0; i < 3; i++)
            {
                ShootProjectileAtPlayer();
                yield return new WaitForSeconds(0.4f);
            }
        }

        projectileTimer = projectileCooldown;
        TransitionToState(BossState.COOLDOWN);
    }

    void ShootProjectileAtPlayer()
    {
        if (projectilePrefab == null || player == null) return;

        // Spawn ligeramente adelante del boss en su dirección
        Vector2 spawnOffset = isFacingRight ? Vector2.right * 0.5f : Vector2.left * 0.5f;
        Vector2 spawnPosition = (Vector2)transform.position + spawnOffset;

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Vector2 direction = (player.position - transform.position).normalized;

        ProyectilBoss proyectilScript = projectile.GetComponent<ProyectilBoss>();
        if (proyectilScript != null)
        {
            proyectilScript.SetDirection(direction);
        }
        else
        {
            // Fallback
            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = direction * projectileSpeed;
            }
        }
    }

    void ShootFanPattern(int projectileCount)
    {
        if (projectilePrefab == null) return;

        float angleStep = 360f / projectileCount;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            ProyectilBoss proyectilScript = projectile.GetComponent<ProyectilBoss>();
            if (proyectilScript != null)
            {
                proyectilScript.SetDirection(direction);
            }
            else
            {
                Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
                if (projRb != null)
                {
                    projRb.linearVelocity = direction * projectileSpeed;
                }
            }
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        bool isRunning = currentState == BossState.CHASE && Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("YVelocity", rb.linearVelocity.y);
    }

    void FlipSprite()
    {
        if (player == null || spriteRenderer == null) return;

        // No voltear durante ataques o stun
        if (currentState == BossState.MELEE_ATTACK ||
            currentState == BossState.STUN ||
            currentState == BossState.PROJECTILE_ATTACK)
            return;

        // Cooldown para voltear (evita volteos instantáneos)
        if (Time.time < lastFlipTime + flipCooldown)
            return;

        bool shouldFaceRight = player.position.x > transform.position.x;

        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            spriteRenderer.flipX = !isFacingRight;
            lastFlipTime = Time.time;
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
            StartCoroutine(BlinkEffect());

            // Pequeño stun solo si no está atacando
            if (currentState != BossState.MELEE_ATTACK &&
                currentState != BossState.PROJECTILE_ATTACK &&
                currentState != BossState.JUMP_ATTACK)
            {
                TransitionToState(BossState.STUN);
            }
        }
    }

    void Die()
    {
        if (animator != null) animator.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        enabled = false;
        Destroy(gameObject, 2f);
    }

    // ANIMATION EVENTS - Llama estos desde el Animator
    public void OnMeleeAttackHit()
    {
        Vector2 meleeCenter = GetFlippedPosition(meleeOffset);
        Collider2D[] hits = Physics2D.OverlapCircleAll(meleeCenter, meleeRange, playerLayer);
        foreach (Collider2D hit in hits)
        {
            HealthP6 playerHealth = hit.GetComponent<HealthP6>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(meleeDamage);
            }
        }
    }

    public void OnAttackAnimationEnd()
    {
        if (currentState == BossState.MELEE_ATTACK)
        {
            TransitionToState(BossState.COOLDOWN);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Obtener centros con offsets flipados
        Vector2 damageCenter = GetFlippedPosition(damageOffset);
        Vector2 meleeCenter = GetFlippedPosition(meleeOffset);
        Vector2 jumpDetectCenter = GetFlippedPosition(jumpDetectOffset);
        Vector2 activationCenter = GetFlippedPosition(activationOffset);

        // Daño por contacto (Rojo)
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(damageCenter, damageRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(damageCenter, damageRange);

        // Ataque melee (Amarillo)
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(meleeCenter, meleeRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(meleeCenter, meleeRange);

        // Detección de salto (Azul)
        Gizmos.color = new Color(0f, 0f, 1f, 0.2f);
        Gizmos.DrawSphere(jumpDetectCenter, jumpDetectRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(jumpDetectCenter, jumpDetectRange);

        // Rango de activación (Verde)
        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        Gizmos.DrawSphere(activationCenter, activationRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(activationCenter, activationRange);

        // Detección de suelo
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Líneas de referencia
        Gizmos.color = Color.white * 0.5f;
        Gizmos.DrawLine(transform.position, damageCenter);
        Gizmos.DrawLine(transform.position, meleeCenter);
        Gizmos.DrawLine(transform.position, jumpDetectCenter);
        Gizmos.DrawLine(transform.position, activationCenter);

        // Marcadores en los centros
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(damageCenter, Vector3.one * 0.15f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(meleeCenter, Vector3.one * 0.15f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(jumpDetectCenter, Vector3.one * 0.15f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(activationCenter, Vector3.one * 0.15f);

        // Indicador de dirección
        Gizmos.color = Color.cyan;
        Vector3 directionLine = isFacingRight ? Vector3.right : Vector3.left;
        Gizmos.DrawRay(transform.position, directionLine * 1.5f);

        // Texto debug (solo en Scene view)
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
            $"State: {currentState}\nPhase: {currentPhase}\nGrounded: {isGrounded}\nJumping: {isJumping}");
#endif
    }
}