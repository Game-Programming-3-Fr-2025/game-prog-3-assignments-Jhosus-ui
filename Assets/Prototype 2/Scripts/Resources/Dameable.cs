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

        // Obtener referencias automáticamente
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        // Guardar color original
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    // Método específico para el script Attacks
    public void TakeDamage(int damage)
    {
        // Reducir vida
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        // Debug
        Debug.Log($"{gameObject.name} dañado: -{damage} HP ({currentHealth}/{maxHealth})");

        // Efectos visuales
        if (spriteRenderer != null)
            StartCoroutine(FlashDamage());

        // Knockback simple
        if (rb != null)
            ApplySimpleKnockback();

        // Verificar muerte
        if (currentHealth <= 0)
            Die();
    }

    // Versión con posición para knockback direccional
    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        TakeDamage(damage);

        // Knockback direccional
        if (rb != null)
        {
            Vector2 knockbackDir = ((Vector2)transform.position - attackerPosition).normalized;
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    IEnumerator FlashDamage()
    {
        if (spriteRenderer == null) yield break;

        // Flash rojo
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(damageFlashDuration);

        // Volver al color original
        spriteRenderer.color = originalColor;
    }

    void ApplySimpleKnockback()
    {
        // Knockback simple hacia atrás
        rb.AddForce(Vector2.up * 2f + Vector2.left * 1f * knockbackForce, ForceMode2D.Impulse);
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} eliminado!");

        // Opción 1: Desactivar
        gameObject.SetActive(false);

        // Opción 2: Destruir después de delay
        // Destroy(gameObject, 0.1f);
    }

    // Método para curar
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    // Resetear objeto
    public void ResetObject()
    {
        currentHealth = maxHealth;
        gameObject.SetActive(true);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    // Para debug
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Barra de vida mini
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