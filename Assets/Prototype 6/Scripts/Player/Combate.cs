using UnityEngine;

public class Combate : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1f;
    public Vector2 attackOffset = new Vector2(0.5f, 0f);
    public float attackDamage = 5f;
    public float attackCooldown = 0.5f;

    [Header("Sound Settings")]
    public AudioClip attackSound;
    public float soundVolume = 1f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private LayerMask enemyLayer;
    private ManagerUp manager;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        enemyLayer = LayerMask.GetMask("Enemy"); // Asegúrar enemigos en la capa "Enemy"
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
        lastAttackTime = Time.time; // Actualiza el tiempo del último ataque

        if (animator != null)
            animator.SetTrigger("Attack");

        PlayAttackSound();
        DealDamage();
        Invoke(nameof(EndAttack), 0.1f);
    }

    void PlayAttackSound()
    {
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound, soundVolume);
        }
    }

    void DealDamage()
    {
        Vector2 attackPos = GetAttackPosition();
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, attackRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            var enemyHealth = hit.GetComponent<BossHealth>();
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

    Vector2 GetAttackPosition() //calcular la posicion del ataque
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