using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthP6 : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 4;
    public float invincibilityTime = 1.5f;
    public float knockbackForce = 8f;

    [Header("UI References")]
    public Image[] healthHearts;

    [Header("Sound Settings")]
    public AudioClip damageSound;
    public AudioClip deathSound;
    public float soundVolume = 1f;

    private int currentHealth;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rb;
    private ManagerUp manager;
    private AudioSource audioSource;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        manager = FindObjectOfType<ManagerUp>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (manager != null)
        {
            int healthLevel = manager.GetHealthLevel();
            maxHealth = 4 + healthLevel;
        }

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

        PlayDamageSound();

        if (currentHealth > 0)
        {
            ApplyKnockback();
            if (animator != null) animator.SetTrigger("Hit");
            StartCoroutine(InvincibilityRoutine());
        }
        else
        {
            Die();
        }
    }

    void PlayDamageSound()
    {
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound, soundVolume);
        }
    }

    void PlayDeathSound()
    {
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound, soundVolume);
        }
    }

    void ApplyKnockback()
    {
        if (rb == null) return;

        Vector2 direction = spriteRenderer.flipX ? Vector2.right : Vector2.left;
        direction.y = 0.2f;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    public void UpdateHealthUI()
    {
        if (healthHearts == null) return;

        for (int i = 0; i < healthHearts.Length; i++)
        {
            if (healthHearts[i] != null)
            {
                healthHearts[i].enabled = i < currentHealth;
            }
        }
    }

    void Die()
    {
        PlayDeathSound();

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        GameManager6.Instance?.PlayerDied();
    }

    public void Respawn()
    {
        if (manager != null)
        {
            int healthLevel = manager.GetHealthLevel();
            maxHealth = 4 + healthLevel;
        }

        currentHealth = maxHealth;
        UpdateHealthUI();
        isInvincible = false;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    public void Heal(int amount = 1)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthUI();
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public bool IsAlive() => currentHealth > 0;
}