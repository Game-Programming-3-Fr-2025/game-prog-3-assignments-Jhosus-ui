using UnityEngine;
using System.Collections;

public class Dameable : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Efectos de Daño")]
    public Color damageColor = Color.red;
    public float damageFlashDuration = 0.2f;
    public float knockbackForce = 3f;

    [Header("Referencias")]
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;

    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = spriteRenderer ?? GetComponent<SpriteRenderer>();
        rb = rb ?? GetComponent<Rigidbody2D>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int damage) // Aplique daño sin dirección de atacante
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (spriteRenderer != null)
            StartCoroutine(FlashDamage());

        if (rb != null)
            ApplySimpleKnockback();

        if (currentHealth <= 0)
            Die();
    }

    public void TakeDamage(int damage, Vector2 attackerPosition) //Verifiquemos dano junto con la direccion 
    {
        TakeDamage(damage);

        if (rb != null) //Apliquemos un pequeno retroceso 
        {
            Vector2 knockbackDir = ((Vector2)transform.position - attackerPosition).normalized; 
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    IEnumerator FlashDamage() //Creemos un efecto de parpadeo al recibir daño
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(damageFlashDuration); 
        spriteRenderer.color = originalColor;
    }

    void ApplySimpleKnockback()
    {
        rb.AddForce(Vector2.up * 2f + Vector2.left * 1f * knockbackForce, ForceMode2D.Impulse);
    }

    void Die()
    {
        gameObject.SetActive(false);
    }

    public void Heal(int amount) //Opcion de Curar dejemoslos pero no es necesaria
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount); 
    }

    public void ResetObject() 
    {
        currentHealth = maxHealth;
        gameObject.SetActive(true);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 barPos = transform.position + Vector3.up * 0.8f;
        float healthPercent = (float)currentHealth / maxHealth;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(barPos, new Vector3(1f, 0.15f, 0));

        Gizmos.color = Color.green;
        Gizmos.DrawCube(
            barPos - Vector3.right * (0.5f - 0.5f * healthPercent),
            new Vector3(1f * healthPercent, 0.1f, 0)
        );
    }
}