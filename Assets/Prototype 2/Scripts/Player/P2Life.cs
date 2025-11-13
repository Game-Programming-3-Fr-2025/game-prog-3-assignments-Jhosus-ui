using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class P2Life : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 3;
    public int currentHealth;
    public List<Image> heartImages;
    public Sprite fullHeart, emptyHeart;

    [Header("Efectos")]
    public float invincibilityTime = 1.5f;
    public float blinkInterval = 0.1f;
    public float damageVibrationIntensity = 0.6f;
    public float damageVibrationDuration = 0.3f;

    [Header("Knockback")]
    public float knockbackForce = 10f; // Fuerza del knockback
    public float knockbackIntensity = 1f; // Multiplicador de intensidad

    private bool isInvincible = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateHeartsUI();
    }

    public void TakeDamage(int damageAmount, Vector2 damageSourcePosition)
    {
        if (isInvincible) return;

        currentHealth = Mathf.Max(0, currentHealth - damageAmount);
        UpdateHeartsUI();

        StartCoroutine(DamageVibration());

        // Aplicar knockback automáticamente
        if (rb != null)
        {
            Vector2 knockbackDir = ((Vector2)transform.position - damageSourcePosition).normalized;
            knockbackDir = new Vector2(knockbackDir.x * 1.2f, Mathf.Clamp(knockbackDir.y + 0.3f, 0.4f, 0.8f)).normalized;
            float finalKnockbackForce = knockbackForce * knockbackIntensity;
            StartCoroutine(ApplyKnockback(knockbackDir, finalKnockbackForce));
        }

        StartCoroutine(DamageEffects());

        if (currentHealth <= 0) Die();
    }

    public void TakeDamage(int damageAmount) => TakeDamage(damageAmount, Vector2.zero);

    private IEnumerator ApplyKnockback(Vector2 direction, float force)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direction * force, ForceMode2D.Impulse);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator DamageVibration()
    {
        if (Gamepad.current?.added != true) yield break;

        // Vibración constante sin importar la dirección
        float elapsed = 0f;

        while (elapsed < damageVibrationDuration)
        {
            try
            {
                // Usar vibración completa constante
                Gamepad.current.SetMotorSpeeds(damageVibrationIntensity, damageVibrationIntensity);
            }
            catch (System.InvalidOperationException)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (Gamepad.current?.added != true) yield break;

        try
        {
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
        catch (System.InvalidOperationException) { }
    }

    private IEnumerator DamageEffects()
    {
        isInvincible = true;
        float timer = 0f;
        bool isVisible = true;

        while (timer < invincibilityTime)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = isVisible;
            isVisible = !isVisible;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        if (spriteRenderer != null) spriteRenderer.enabled = true;
        isInvincible = false;
    }

    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        UpdateHeartsUI();
    }

    private void UpdateHeartsUI()
    {
        for (int i = 0; i < heartImages.Count; i++)
            heartImages[i].sprite = i < currentHealth ? fullHeart : emptyHeart;
    }

    private void Die() => Debug.Log("¡Jugador 2 ha muerto!");

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHeartsUI();
        StopAllCoroutines();
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        isInvincible = false;
        if (Gamepad.current?.added == true) Gamepad.current.SetMotorSpeeds(0f, 0f);
    }

    private void OnDestroy()
    {
        if (Gamepad.current?.added == true) Gamepad.current.SetMotorSpeeds(0f, 0f);
    }

    public bool IsInvincible => isInvincible;
}