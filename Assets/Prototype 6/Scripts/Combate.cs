using UnityEngine;

public class Combate : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1f;
    public Vector2 attackOffset = new Vector2(0.5f, 0f);
    public float attackDamage = 5f;
    public float attackCooldown = 0.5f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private LayerMask enemyLayer;
    private ManagerUp manager;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyLayer = LayerMask.GetMask("Enemy");
        manager = FindObjectOfType<ManagerUp>();
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
            animator.SetTrigger("Attack");

        DealDamage();
        Invoke(nameof(EndAttack), 0.1f);
    }

    void DealDamage()
    {
        Vector2 attackPos = GetAttackPosition();
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, attackRange, enemyLayer);

        // Registrar golpe en ManagerUp si hay hits
        if (manager != null && hits.Length > 0)
        {
            manager.RegisterHit();
        }

        foreach (Collider2D hit in hits)
        {
            var enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }
        }
    }

    void EndAttack()
    {
        isAttacking = false;
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