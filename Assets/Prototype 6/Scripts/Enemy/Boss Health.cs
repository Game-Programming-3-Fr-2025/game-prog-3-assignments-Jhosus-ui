using UnityEngine;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 1000f;

    [Header("Combat")]
    public int contactDamage = 2;
    public int meleeDamage = 30;
    public float contactRange = 1.5f;
    public float meleeRange = 1.5f;
    public Vector2 contactOffset = Vector2.zero;
    public Vector2 meleeOffset = Vector2.zero;

    [Header("Movement")]
    public float chaseSpeed = 4f;
    public float phase2Speed = 7f;
    public float activationRange = 15f;
    public float jumpForce = 10f;
    public float jumpRange = 4f;

    [Header("Attacks")]
    public float attackCooldown = 0.8f;
    public GameObject projectilePrefab;
    public float projectileCooldown = 5f;
    public float fanShotCooldown = 15f;
    public float jumpChance = 0.2f;

    [Header("Visuals")]
    public float blinkDuration = 0.1f;
    public int blinkCount = 3;
    public float flipCooldown = 0.3f;

    [Header("Detection")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;

    // Privadas
    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private float currentHealth;
    private int phase = 1;
    private bool isActive = false;
    private bool isGrounded = false;
    private bool isJumping = false;
    private bool facingRight = true;

    // Timers
    private float contactTimer = 0f;
    private float attackTimer = 0f;
    private float projectileTimer = 0f;
    private float fanTimer = 0f;
    private float flipTimer = 0f;
    private float stunTimer = 0f;

    // Estados simples
    private enum State { Idle, Chase, Attacking, Stunned }
    private State currentState = State.Idle;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Crear groundCheck si no existe
        if (groundCheck == null) CreateGroundCheck();

        projectileTimer = projectileCooldown * 0.5f; // Primer ataque más rápido
    }

    void Update()
    {
        if (player == null) return;

        UpdateGroundDetection();
        UpdateTimers();
        UpdateState();
        UpdateVisuals();
    }

    void UpdateGroundDetection()
    {
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, LayerMask.GetMask("Ground"));
    }

    void UpdateTimers()
    {
        float delta = Time.deltaTime;

        if (contactTimer > 0) contactTimer -= delta;
        if (attackTimer > 0) attackTimer -= delta;
        if (projectileTimer > 0) projectileTimer -= delta;
        if (fanTimer > 0) fanTimer -= delta;
        if (flipTimer > 0) flipTimer -= delta;
        if (stunTimer > 0) stunTimer -= delta;
    }

    void UpdateState()
    {
        if (!isActive)
        {
            CheckActivation();
            return;
        }

        if (stunTimer > 0)
        {
            currentState = State.Stunned;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.9f, rb.linearVelocity.y);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                UpdateIdle(distance);
                break;
            case State.Chase:
                UpdateChase(distance);
                break;
            case State.Attacking:
                UpdateAttacking();
                break;
            case State.Stunned:
                if (stunTimer <= 0) currentState = State.Chase;
                break;
        }
    }

    void CheckActivation()
    {
        if (Vector2.Distance(transform.position, player.position) <= activationRange)
        {
            isActive = true;
            currentState = State.Chase;
        }
    }

    void UpdateIdle(float distance)
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (distance < activationRange)
            currentState = State.Chase;
    }

    void UpdateChase(float distance)
    {
        // Movimiento hacia el player
        Vector2 direction = (player.position - transform.position).normalized;
        float speed = phase == 1 ? chaseSpeed : phase2Speed;
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);

        // Flip sprite
        if (flipTimer <= 0)
        {
            bool shouldFaceRight = player.position.x > transform.position.x;
            if (shouldFaceRight != facingRight)
            {
                facingRight = shouldFaceRight;
                sprite.flipX = !facingRight;
                flipTimer = flipCooldown;
            }
        }

        // Daño por contacto
        if (contactTimer <= 0)
        {
            CheckContactDamage();
            contactTimer = 1f;
        }

        // Decidir ataque
        if (attackTimer <= 0)
        {
            if (distance < meleeRange)
            {
                StartMeleeAttack();
            }
            else if (phase >= 3 && projectileTimer <= 0 && distance < 10f)
            {
                StartProjectileAttack();
            }
            else if (phase >= 2 && distance < jumpRange && isGrounded && !isJumping)
            {
                TryJumpAttack();
            }
        }
    }

    void UpdateAttacking()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (attackTimer <= 0)
        {
            currentState = State.Chase;
        }
    }

    void CheckContactDamage()
    {
        Vector2 damagePos = GetFlippedPosition(contactOffset);
        Collider2D hit = Physics2D.OverlapCircle(damagePos, contactRange, LayerMask.GetMask("Player"));

        if (hit != null)
        {
            HealthP6 health = hit.GetComponent<HealthP6>();
            if (health != null && health.IsAlive())
            {
                health.TakeDamage(contactDamage);
            }
        }
    }

    void StartMeleeAttack()
    {
        currentState = State.Attacking;
        attackTimer = attackCooldown;
        if (anim != null) anim.SetTrigger("Attack");
    }

    void StartProjectileAttack()
    {
        currentState = State.Attacking;
        attackTimer = attackCooldown;
        projectileTimer = projectileCooldown;

        if (anim != null) anim.SetTrigger("Attack");
        StartCoroutine(ExecuteProjectileAttack());
    }

    void TryJumpAttack()
    {
        bool playerHigher = player.position.y > transform.position.y + 1f;
        bool playerJumping = IsPlayerJumping();
        bool shouldJump = playerHigher || (playerJumping && Random.value < jumpChance);

        if (shouldJump)
        {
            StartJumpAttack();
        }
    }

    void StartJumpAttack()
    {
        isJumping = true;
        currentState = State.Attacking;
        attackTimer = attackCooldown;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * phase2Speed, jumpForce);

        if (anim != null) anim.SetTrigger("Attack");
    }

    IEnumerator ExecuteProjectileAttack()
    {
        yield return new WaitForSeconds(0.2f); // Pequeño delay para sincronizar con animación

        if (fanTimer <= 0)
        {
            // Disparo en abanico
            ShootFanProjectiles(8);
            fanTimer = fanShotCooldown;
        }
        else
        {
            // Triple disparo dirigido
            for (int i = 0; i < 3; i++)
            {
                ShootAtPlayer();
                yield return new WaitForSeconds(0.4f);
            }
        }
    }

    void ShootAtPlayer()
    {
        if (projectilePrefab == null || player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        ProyectilBoss proj = projectile.GetComponent<ProyectilBoss>();
        if (proj != null) proj.SetDirection(direction);
    }

    void ShootFanProjectiles(int count)
    {
        if (projectilePrefab == null) return;

        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            ProyectilBoss proj = projectile.GetComponent<ProyectilBoss>();
            if (proj != null) proj.SetDirection(direction);
        }
    }

    void UpdateVisuals()
    {
        if (anim == null) return;

        bool running = currentState == State.Chase && Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        anim.SetBool("IsRunning", running);
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetFloat("YVelocity", rb.linearVelocity.y);

        // Detectar aterrizaje después de salto
        if (isJumping && isGrounded)
        {
            isJumping = false;
        }
    }

    Vector2 GetFlippedPosition(Vector2 offset)
    {
        Vector2 flipped = offset;
        if (!facingRight) flipped.x = -offset.x;
        return (Vector2)transform.position + flipped;
    }

    bool IsPlayerJumping()
    {
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        return playerRb != null && Mathf.Abs(playerRb.linearVelocity.y) > 0.5f;
    }

    void CreateGroundCheck()
    {
        GameObject check = new GameObject("GroundCheck");
        check.transform.SetParent(transform);
        check.transform.localPosition = new Vector3(0, -1f, 0);
        groundCheck = check.transform;
    }

    public void TakeDamage(float damage)
    {
        if (stunTimer > 0) return;

        currentHealth -= damage;
        StartCoroutine(BlinkEffect());
        stunTimer = 0.3f; // Stun corto

        CheckPhaseTransition();

        if (currentHealth <= 0) Die();
    }

    void CheckPhaseTransition()
    {
        float healthPercent = currentHealth / maxHealth;

        if (phase == 1 && healthPercent <= 0.6f)
        {
            phase = 2;
            StartCoroutine(PhaseTransition());
        }
        else if (phase == 2 && healthPercent <= 0.3f)
        {
            phase = 3;
            StartCoroutine(PhaseTransition());
        }
    }

    IEnumerator PhaseTransition()
    {
        stunTimer = 1.5f;
        StartCoroutine(BlinkEffect());

        // Pequeño knockback
        if (player != null)
        {
            Vector2 knockback = (transform.position - player.position).normalized;
            rb.linearVelocity = new Vector2(knockback.x * 5f, 3f);
        }

        yield return new WaitForSeconds(1.5f);
        currentState = State.Chase;
    }

    IEnumerator BlinkEffect()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            sprite.color = new Color(1f, 0.5f, 0.5f, 0.7f);
            yield return new WaitForSeconds(blinkDuration);
            sprite.color = Color.white;
            yield return new WaitForSeconds(blinkDuration);
        }
    }

    void Die()
    {
        if (anim != null) anim.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        enabled = false;
        Destroy(gameObject, 2f);
    }

    // Animation Events
    public void OnMeleeHit()
    {
        Vector2 attackPos = GetFlippedPosition(meleeOffset);
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, meleeRange, LayerMask.GetMask("Player"));

        foreach (Collider2D hit in hits)
        {
            HealthP6 health = hit.GetComponent<HealthP6>();
            if (health != null) health.TakeDamage(meleeDamage);
        }
    }

    public void OnAttackEnd()
    {
        if (currentState == State.Attacking)
        {
            currentState = State.Chase;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Contact damage
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetFlippedPosition(contactOffset), contactRange);

        // Melee attack
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GetFlippedPosition(meleeOffset), meleeRange);

        // Jump range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, jumpRange);

        // Activation range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        // Ground check
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }

    public void ResetBoss()
    {
        currentHealth = maxHealth;
        currentState = State.Idle;
        phase = 1;
        isActive = false;
        isJumping = false;

        // Resetear todos los timers
        attackTimer = 0;
        projectileTimer = projectileCooldown * 0.5f;
        fanTimer = 0;

        // Resetear física
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Resetear animación
        if (anim != null)
        {
            anim.SetBool("IsRunning", false);
            anim.Play("Idle");
        }
    }
}