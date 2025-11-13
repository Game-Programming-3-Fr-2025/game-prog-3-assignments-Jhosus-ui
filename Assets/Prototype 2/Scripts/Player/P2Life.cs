using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class P2Life : MonoBehaviour
{
    [Header("Configuraci�n de Vida")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Interfaz de Corazones")]
    public List<Image> heartImages;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Efectos de Da�o")]
    public float invincibilityTime = 1.5f;
    public float blinkInterval = 0.1f;

    private bool isInvincible = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateHeartsUI();
    }

    // Funci�n para recibir da�o con knockback
    public void TakeDamage(int damageAmount, Vector2 knockbackDirection = default, float knockbackForce = 0f)
    {

        if (isInvincible) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);

        UpdateHeartsUI();

        if (knockbackForce > 0f && rb != null)
        {
            ApplyKnockback(knockbackDirection, knockbackForce);
        }

        StartCoroutine(DamageEffects());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Funci�n para recibir da�o normal (sobrecarga para compatibilidad)
    public void TakeDamage(int damageAmount)
    {
        TakeDamage(damageAmount, Vector2.zero, 0f);
    }

    // Aplicar efecto de knockback
    private void ApplyKnockback(Vector2 direction, float force)
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        }
    }

    // Corrutina para efectos de da�o
    private IEnumerator DamageEffects()
    {
        isInvincible = true;

        float timer = 0f;
        bool isVisible = true;

        while (timer < invincibilityTime)
        {

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = isVisible;
                isVisible = !isVisible;
            }
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isInvincible = false;
    }

    // Funci�n para curar/recuperar vida
    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHeartsUI();
    }

    // Funci�n para actualizar los corazones en la UI
    private void UpdateHeartsUI()
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < currentHealth)
            {
                heartImages[i].sprite = fullHeart;
            }
            else
            {
                heartImages[i].sprite = emptyHeart;
            }
        }
    }

    // Funci�n que se llama cuando el jugador muere
    private void Die()
    {
        Debug.Log("�Jugador 2 ha muerto!");
    }

    // Funci�n para reiniciar la vida al m�ximo
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHeartsUI();

        // Detener efectos de da�o si est�n activos
        StopAllCoroutines();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        isInvincible = false;
    }

    // Propiedad p�blica para verificar invencibilidad
    public bool IsInvincible
    {
        get { return isInvincible; }
    }
}