using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 30f;

    [Header("Damage Settings")]
    public int damageToPlayer = 1;
    public float damageRange = 0.8f;
    public float damageInterval = 1f;

    private float currentHealth;
    private float lastDamageTime = -999f;
    private LayerMask playerLayer;

    void Start()
    {
        currentHealth = maxHealth;
        playerLayer = LayerMask.GetMask("Player");
    }

    void Update()
    {
        // Revisar si el player está en rango para hacer daño
        if (Time.time >= lastDamageTime + damageInterval)
        {
            CheckForPlayerContact();
        }
    }

    void CheckForPlayerContact()
    {
        // Detectar al jugador en el rango de daño
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, damageRange, playerLayer);

        if (playerCollider != null)
        {
            HealthP6 playerHealth = playerCollider.GetComponent<HealthP6>();
            if (playerHealth != null && playerHealth.IsAlive())
            {
                playerHealth.TakeDamage(damageToPlayer);
                lastDamageTime = Time.time;
                Debug.Log($"{gameObject.name} damaged player for {damageToPlayer}");
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRange);
    }
}