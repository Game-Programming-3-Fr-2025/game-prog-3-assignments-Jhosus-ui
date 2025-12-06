using UnityEngine;

public class ProyectilBoss : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 10;
    public float speed = 8f;
    public float lifeTime = 5f;

    private Vector2 direction;
    private float timer;

    void Start()
    {
        timer = lifeTime;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.linearVelocity = direction * speed;
        }
    }

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        timer -= Time.deltaTime;
        if (timer <= 0) Destroy(gameObject);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HealthP6 playerHealth = other.GetComponent<HealthP6>();
            if (playerHealth != null && playerHealth.IsAlive())
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}