using UnityEngine;

public class ProyectilBoss : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 10;
    public float lifeTime = 5f;
    public float damageRange = 0.5f;
    public Vector2 damageOffset = Vector2.zero;

    [Header("Movement")]
    public float speed = 8f;

    private Rigidbody2D rb;
    private LayerMask playerLayer;
    private float timer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerLayer = LayerMask.GetMask("Player");
        timer = lifeTime;

        // Destruir autom�ticamente despu�s del tiempo de vida
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Verificar da�o continuo (opcional - depende de si quieres da�o por frame o solo una vez)
        CheckForPlayerDamage();

        // Timer manual como backup
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }

    void CheckForPlayerDamage()
    {
        Vector2 damageCenter = (Vector2)transform.position + damageOffset;
        Collider2D playerCollider = Physics2D.OverlapCircle(damageCenter, damageRange, playerLayer);

        if (playerCollider != null)
        {
            HealthP6 playerHealth = playerCollider.GetComponent<HealthP6>();
            if (playerHealth != null && playerHealth.IsAlive())
            {
                playerHealth.TakeDamage(damage);
                Destroy(gameObject); // Destruir al hacer da�o
            }
        }
    }

    // M�todo para inicializar la direcci�n del proyectil
    public void SetDirection(Vector2 direction)
    {
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Destruir al chocar con obst�culos (opcional)
        if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Gizmo para el �rea de da�o
        Gizmos.color = Color.red;
        Vector2 damageCenter = (Vector2)transform.position + damageOffset;
        Gizmos.DrawWireSphere(damageCenter, damageRange);

        // Marcador del centro de da�o
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(damageCenter, Vector3.one * 0.1f);
    }
}