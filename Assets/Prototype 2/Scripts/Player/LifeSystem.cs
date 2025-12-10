using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public enum PlayerType
{
    Player1,
    Player2
}

public class LifeSystem : MonoBehaviour
{
    [Header("Player Identity")]
    public PlayerType playerType = PlayerType.Player1;

    [Header("Referencias UI")]
    public RectTransform uiPanel;
    public Transform heartsContainer;

    [Header("Vida")]
    public int maxHealth = 3;
    public int currentHealth;
    public List<Image> heartImages;
    public Sprite fullHeart, emptyHeart;

    [Header("Detección de Daño")]
    public float damageDetectionRadius = 1f;
    public Color damageGizmoColor = Color.red;
    public LayerMask dangerLayers;

    [Header("Efectos")]
    public float invincibilityTime = 1.5f;
    public float blinkInterval = 0.1f;
    public float damageVibrationIntensity = 0.6f;
    public float damageVibrationDuration = 0.3f;

    [Header("Knockback")]
    public float knockbackForce = 10f;
    public float knockbackIntensity = 1f;

    [Header("Sonido")]
    public AudioClip damageSound;
    public float damageVolume = 1f;

    private bool isInvincible = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private UIPositioner uiPositioner;
    private Dictionary<GameObject, float> objectCooldowns = new Dictionary<GameObject, float>();
    private const float cooldownDuration = 1f;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiPositioner = FindObjectOfType<UIPositioner>();
        UpdateHeartsUI();
    }

    void Update()
    {
        UpdateCooldowns();
        if (!isInvincible) CheckForDamage();
    }

    void UpdateCooldowns()
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (var kvp in objectCooldowns)
        {
            if (kvp.Key == null || Time.time >= kvp.Value)
                toRemove.Add(kvp.Key);
        }

        foreach (var key in toRemove)
        {
            if (key != null) objectCooldowns.Remove(key);
        }
    }

    void CheckForDamage()
    {
        if (currentHealth <= 0 || !enabled) return;

        Collider2D[] dangers = Physics2D.OverlapCircleAll(transform.position, damageDetectionRadius, dangerLayers);

        foreach (Collider2D danger in dangers)
        {
            if (danger == null) continue;

            GameObject dangerObject = danger.gameObject;
            if (objectCooldowns.ContainsKey(dangerObject)) continue;

            DamageSource damageSource = danger.GetComponent<DamageSource>();

            if (damageSource != null && damageSource.CanDamage())
            {
                TakeDamage(damageSource.damageAmount, danger.transform.position);
                damageSource.OnDamageDealt();
                objectCooldowns[dangerObject] = Time.time + cooldownDuration;
                break;
            }
        }
    }

    public void TakeDamage(int damageAmount, Vector2 damageSourcePosition)
    {
        if (isInvincible || currentHealth <= 0 || !enabled) return;

        PlayDamageSound();
        currentHealth = Mathf.Max(0, currentHealth - damageAmount);
        UpdateHeartsUI();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(DamageVibration());

        if (rb != null)
        {
            Vector2 knockbackDir = ((Vector2)transform.position - damageSourcePosition).normalized;
            knockbackDir = new Vector2(knockbackDir.x * 1.2f, Mathf.Clamp(knockbackDir.y + 0.3f, 0.4f, 0.8f)).normalized;
            StartCoroutine(ApplyKnockback(knockbackDir, knockbackForce * knockbackIntensity));
        }

        StartCoroutine(DamageEffects());
    }

    public void TakeDamage(int damageAmount) => TakeDamage(damageAmount, Vector2.zero);

    private void PlayDamageSound()
    {
        if (damageSound != null)
            AudioSource.PlayClipAtPoint(damageSound, transform.position, damageVolume);
    }

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

        float elapsed = 0f;

        while (elapsed < damageVibrationDuration)
        {
            try
            {
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

    private void Die()
    {
        StopAllCoroutines();
        isInvincible = true;
        enabled = false;

        if (Gamepad.current?.added == true)
        {
            try { Gamepad.current.SetMotorSpeeds(0f, 0f); }
            catch (System.InvalidOperationException) { }
        }

        if (spriteRenderer != null) spriteRenderer.enabled = false;
        objectCooldowns.Clear();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHeartsUI();
        StopAllCoroutines();
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        isInvincible = false;
        enabled = true;
        objectCooldowns.Clear();
        if (Gamepad.current?.added == true) Gamepad.current.SetMotorSpeeds(0f, 0f);
    }

    private void OnDestroy()
    {
        if (Gamepad.current?.added == true) Gamepad.current.SetMotorSpeeds(0f, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = damageGizmoColor;
        Gizmos.DrawWireSphere(transform.position, damageDetectionRadius);

        Gizmos.color = new Color(damageGizmoColor.r, damageGizmoColor.g, damageGizmoColor.b, 0.3f);
        Gizmos.DrawSphere(transform.position, damageDetectionRadius * 0.1f);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = new Color(damageGizmoColor.r, damageGizmoColor.g, damageGizmoColor.b, 0.1f);
        Gizmos.DrawSphere(transform.position, damageDetectionRadius);
    }

    public bool IsInvincible => isInvincible;
    public PlayerType GetPlayerType() => playerType;
}