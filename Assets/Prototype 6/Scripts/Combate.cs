using UnityEngine;

public class Combate : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1f;
    public Vector2 attackOffset = new Vector2(0.5f, 0f);
    public float attackDamage = 10f;
    public float attackCooldown = 0.5f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Movement movement; // ← AGREGAR ESTA REFERENCIA
    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private LayerMask enemyLayer;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<Movement>(); // ← OBTENER LA REFERENCIA

        enemyLayer = LayerMask.GetMask("Enemy");
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && CanAttack())
        {
            Attack();
        }
    }

    bool CanAttack()
    {
        return !isAttacking && Time.time >= lastAttackTime + attackCooldown;
    }

    void Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (animator != null)
        {
            // ← FORZAR LOS PARÁMETROS PARA QUE LA ANIMACIÓN DE ATAQUE FUNCIONE
            animator.SetBool("IsGrounded", true); // Temporalmente forzar grounded
            animator.SetFloat("YVelocity", 0); // Temporalmente forzar YVelocity = 0
            animator.SetTrigger("Attack");
        }

        DealDamage();
        Invoke(nameof(EndAttack), 0.3f);
    }

    void DealDamage()
    {
        Vector2 attackPos = GetAttackPosition();
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, attackRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            var enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }
            else
            {
                Debug.Log($"Hit {hit.name} for {attackDamage} damage");
            }
        }
    }

    void EndAttack()
    {
        isAttacking = false;
        // Los parámetros IsGrounded y YVelocity se restaurarán automáticamente 
        // en el próximo Update() del Movement.cs
    }

    Vector2 GetAttackPosition()
    {
        float offsetX = spriteRenderer != null && spriteRenderer.flipX ? -attackOffset.x : attackOffset.x;
        return (Vector2)transform.position + new Vector2(offsetX, attackOffset.y);
    }

    public bool IsAttacking() => isAttacking;

    void OnDrawGizmosSelected()
    {
        Vector2 attackPos = GetAttackPosition();
        Gizmos.color = isAttacking ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(attackPos, attackRange);
        Gizmos.DrawLine(transform.position, attackPos);
    }
}