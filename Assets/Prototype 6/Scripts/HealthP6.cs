using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthP6 : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public float invincibilityTime = 1.5f;
    public float knockbackForce = 8f; // ← AGREGAR: Fuerza de retroceso

    [Header("UI References")]
    public Image[] healthHearts;

    private int currentHealth;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>(); // ← AGREGAR
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void Update()
    {
        if (isInvincible && spriteRenderer != null)
        {
            spriteRenderer.enabled = Time.time % 0.2f > 0.1f;
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (isInvincible || currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthUI();

        // ← AGREGAR: Activar animación de Hit y retroceso
        if (currentHealth > 0)
        {
            ApplyKnockback();
            if (animator != null)
                animator.SetTrigger("Hit");
        }

        if (currentHealth > 0)
        {
            StartCoroutine(InvincibilityRoutine());
        }
        else
        {
            Die();
        }

        Debug.Log($"Player health: {currentHealth}");
    }

    // ← AGREGAR: Método para aplicar retroceso
    private void ApplyKnockback()
    {
        if (rb != null)
        {
            // Calcular dirección opuesta basada en la mirada del jugador
            Vector2 knockbackDirection = spriteRenderer.flipX ?
                Vector2.right : Vector2.left;

            knockbackDirection.y = 0.2f; // Un poco hacia arriba

            // Aplicar fuerza de retroceso
            rb.linearVelocity = Vector2.zero; // Resetear velocidad
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    void UpdateHealthUI()
    {
        if (healthHearts == null) return;

        for (int i = 0; i < healthHearts.Length; i++)
        {
            if (healthHearts[i] != null)
                healthHearts[i].enabled = i < currentHealth;
        }
    }

    void Die()
    {
        Debug.Log("Player died!");
        if (animator != null)
            animator.SetTrigger("Die");
    }

    public void Heal(int amount = 1)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthUI();
    }

    public bool IsAlive() => currentHealth > 0;
}