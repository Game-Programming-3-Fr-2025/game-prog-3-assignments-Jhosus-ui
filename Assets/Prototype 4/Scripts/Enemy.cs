using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float health = 50f;
    public float speed = 2f;
    public float damage = 20f;
    public int moneyReward = 10;

    [Header("Combat")]
    public float attackRange = 1.5f;
    public float attackRate = 1f; // ataques por segundo

    private float lastAttackTime;
    private SimpleVehicle targetVehicle;
    private bool isDead = false;

    void Start()
    {
        // Buscar el vehículo del jugador
        targetVehicle = FindObjectOfType<SimpleVehicle>();

        // Asegurar que tiene el tag correcto
        gameObject.tag = "Enemy";

        // Asegurar que tiene collider
        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (isDead || targetVehicle == null) return;

        MoveTowardsVehicle();
        TryAttackVehicle();
    }

    void MoveTowardsVehicle()
    {
        // Moverse hacia el vehículo
        Vector3 direction = (targetVehicle.transform.position - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime);

        // Voltear el sprite si se mueve hacia la izquierda
        if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }

    void TryAttackVehicle()
    {
        // Solo atacar si está cerca y ha pasado suficiente tiempo
        float distance = Vector3.Distance(transform.position, targetVehicle.transform.position);

        if (distance <= attackRange && Time.time >= lastAttackTime + (1f / attackRate))
        {
            AttackVehicle();
            lastAttackTime = Time.time;
        }
    }

    void AttackVehicle()
    {
        targetVehicle.TakeDamage(damage);

        // Efecto visual simple de ataque
        CreateAttackEffect();
    }

    void CreateAttackEffect()
    {
        // Crear un flash rojo simple
        GameObject effect = new GameObject("AttackEffect");
        effect.transform.position = transform.position;

        SpriteRenderer sr = effect.AddComponent<SpriteRenderer>();
        sr.color = Color.red;
        sr.sortingOrder = 10;

        // Destruir el efecto después de 0.2 segundos
        Destroy(effect, 0.2f);
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        health -= damageAmount;

        // Flash rojo al recibir daño
        StartCoroutine(FlashRed());

        if (health <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color originalColor = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;
    }

    void Die()
    {
        isDead = true;

        // Dar dinero al jugador
        if (targetVehicle != null)
        {
            targetVehicle.AddMoney(moneyReward);
        }

        // Efecto de muerte simple
        CreateDeathEffect();

        // Destruir el enemigo
        Destroy(gameObject);
    }

    void CreateDeathEffect()
    {
        GameObject effect = new GameObject("DeathEffect");
        effect.transform.position = transform.position;

        SpriteRenderer sr = effect.AddComponent<SpriteRenderer>();
        sr.color = Color.yellow;
        sr.sortingOrder = 10;

        Destroy(effect, 0.5f);
    }

    // Para que las balas puedan detectar al enemigo
    void OnTriggerEnter2D(Collider2D other)
    {
        // Si una bala del jugador toca al enemigo
        if (other.CompareTag("PlayerBullet"))
        {
            TakeDamage(25f); // Daño fijo por simplicidad
            Destroy(other.gameObject); // Destruir la bala
        }
    }
}