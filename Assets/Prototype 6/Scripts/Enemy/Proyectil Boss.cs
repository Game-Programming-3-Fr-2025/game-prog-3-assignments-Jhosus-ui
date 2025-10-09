using UnityEngine;

public class ProyectilBoss : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 10;
    public float lifeTime = 5f;

    [Header("Movement")]
    public float speed = 8f;

    private Vector2 moveDirection;
    private bool hasHit = false;
    private float timer = 0f;

    void Start()
    {
        timer = lifeTime;

        // Desactivar gravity si hay Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic; // Modo Kinematic para control manual
        }
    }

    void Update()
    {
        // Mover manualmente sin f�sica
        transform.position += (Vector3)moveDirection * speed * Time.deltaTime;

        // Rotar hacia la direcci�n
        if (moveDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Timer de autodestrucci�n
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }

    // M�todo llamado desde BossHealth
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        // Debug para verificar
        Debug.Log($"Proyectil direcci�n seteada: {moveDirection}, Speed: {speed}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        Debug.Log($"Proyectil colision� con: {other.gameObject.name}, Tag: {other.tag}");

        // Da�o al jugador
        if (other.CompareTag("Player"))
        {
            HealthP6 playerHealth = other.GetComponent<HealthP6>();
            if (playerHealth != null && playerHealth.IsAlive())
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Proyectil hizo {damage} de da�o al jugador");
            }
            hasHit = true;
            Destroy(gameObject);
        }
        // Destruir contra paredes/suelo
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;

        Debug.Log($"Proyectil colision� (Collision) con: {collision.gameObject.name}");

        if (collision.gameObject.CompareTag("Player"))
        {
            HealthP6 playerHealth = collision.gameObject.GetComponent<HealthP6>();
            if (playerHealth != null && playerHealth.IsAlive())
            {
                playerHealth.TakeDamage(damage);
            }
            hasHit = true;
            Destroy(gameObject);
        }
        else
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }

    void OnDrawGizmos()
    {
        // Mostrar direcci�n de movimiento
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, moveDirection * 1f);
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
    }
}