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
    public RectTransform uiPanel; // Panel de UI asignado por UIPositioner
    public Transform heartsContainer; // Contenedor de corazones

    [Header("Vida")]
    public int maxHealth = 3;
    public int currentHealth;
    public List<Image> heartImages;
    public Sprite fullHeart, emptyHeart;

    [Header("Prefabs UI (si se generan dinámicamente)")]
    public GameObject heartPrefab;
    public Vector2 heartSpacing = new Vector2(50f, 0f);

    [Header("Detección de Daño (GIZMO ONLY)")]
    public float damageDetectionRadius = 1f;
    public Color damageGizmoColor = Color.red;
    public LayerMask dangerLayers; // Layer "Dangers" para objetos peligrosos

    [Header("Efectos")]
    public float invincibilityTime = 1.5f;
    public float blinkInterval = 0.1f;
    public float damageVibrationIntensity = 0.6f;
    public float damageVibrationDuration = 0.3f;

    [Header("Knockback")]
    public float knockbackForce = 10f;
    public float knockbackIntensity = 1f;

    private bool isInvincible = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private UIPositioner uiPositioner;
    private float lastDamageTime = 0f;
    private float damageCooldown = 0.5f; // Cooldown entre daños

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Buscar UIPositioner en la escena
        uiPositioner = FindObjectOfType<UIPositioner>();

        // Configurar UI
        SetupHealthUI();

        UpdateHeartsUI();
    }

    void SetupHealthUI()
    {
        // Si no hay contenedor de corazones, buscarlo
        if (heartsContainer == null)
        {
            // Crear un nuevo objeto para los corazones
            GameObject heartsGO = new GameObject("HeartsContainer");
            heartsContainer = heartsGO.transform;

            // Si tenemos un panel UI, parentearlo allí
            if (uiPanel != null)
            {
                heartsContainer.SetParent(uiPanel);
                heartsContainer.localPosition = Vector3.zero;
            }
            else
            {
                // Si no hay panel, buscar el panel correspondiente
                FindAndAssignUIPanel();
            }
        }

        // Si no hay imágenes de corazones, generarlas
        if (heartImages == null || heartImages.Count == 0)
        {
            GenerateHeartsUI();
        }

        // Posicionar los corazones
        PositionHeartsInPanel();
    }

    void FindAndAssignUIPanel()
    {
        if (uiPositioner == null) return;

        // Asignar el panel correcto según el tipo de jugador
        if (playerType == PlayerType.Player1 && uiPositioner.player1UIPanel != null)
        {
            uiPanel = uiPositioner.player1UIPanel;
            heartsContainer.SetParent(uiPanel);
            heartsContainer.localPosition = Vector3.zero;
        }
        else if (playerType == PlayerType.Player2 && uiPositioner.player2UIPanel != null)
        {
            uiPanel = uiPositioner.player2UIPanel;
            heartsContainer.SetParent(uiPanel);
            heartsContainer.localPosition = Vector3.zero;
        }
    }

    void GenerateHeartsUI()
    {
        heartImages = new List<Image>();

        // Destruir corazones existentes
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }

        // Crear nuevos corazones
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heartGO;

            if (heartPrefab != null)
            {
                heartGO = Instantiate(heartPrefab, heartsContainer);
                Image heartImage = heartGO.GetComponent<Image>();
                if (heartImage != null)
                {
                    heartImages.Add(heartImage);
                }
            }
            else
            {
                // Crear un corazón básico si no hay prefab
                heartGO = new GameObject($"Heart_{i}");
                heartGO.transform.SetParent(heartsContainer);

                // Añadir Image component
                Image heartImage = heartGO.AddComponent<Image>();
                heartImage.rectTransform.sizeDelta = new Vector2(50f, 50f);

                // Asignar sprite si está disponible
                if (fullHeart != null)
                {
                    heartImage.sprite = fullHeart;
                }

                heartImages.Add(heartImage);
            }
        }
    }

    void PositionHeartsInPanel()
    {
        if (heartsContainer == null || heartImages.Count == 0) return;

        // Posicionar corazones horizontalmente
        for (int i = 0; i < heartImages.Count; i++)
        {
            RectTransform heartRT = heartImages[i].GetComponent<RectTransform>();

            if (heartRT != null)
            {
                // Anclas al centro
                heartRT.anchorMin = new Vector2(0.5f, 0.5f);
                heartRT.anchorMax = new Vector2(0.5f, 0.5f);
                heartRT.pivot = new Vector2(0.5f, 0.5f);

                // Posición basada en el índice
                float xPos = (i - (heartImages.Count - 1) / 2f) * heartSpacing.x;
                heartRT.anchoredPosition = new Vector2(xPos, 0f);
            }
        }
    }

    void Update()
    {
        // Solo detectar daño si no es invencible
        if (!isInvincible && CanTakeDamage())
        {
            CheckForDamage();
        }
    }

    bool CanTakeDamage()
    {
        // Verificar cooldown entre daños
        return Time.time >= lastDamageTime + damageCooldown;
    }

    void CheckForDamage()
    {
        // Detectar objetos peligrosos dentro del radio de detección
        Collider2D[] dangers = Physics2D.OverlapCircleAll(
            transform.position,
            damageDetectionRadius,
            dangerLayers
        );

        foreach (Collider2D danger in dangers)
        {
            DamageSource damageSource = danger.GetComponent<DamageSource>();
            if (damageSource != null && damageSource.CanDamage())
            {
                // Calcular dirección del daño para knockback
                Vector2 damageDirection = (transform.position - danger.transform.position).normalized;
                TakeDamage(damageSource.damageAmount, danger.transform.position);
                damageSource.OnDamageDealt();
                lastDamageTime = Time.time;
                break; // Solo un daño por frame
            }
        }
    }

    public void TakeDamage(int damageAmount, Vector2 damageSourcePosition)
    {
        if (isInvincible) return;

        currentHealth = Mathf.Max(0, currentHealth - damageAmount);
        UpdateHeartsUI();

        StartCoroutine(DamageVibration());

        // Aplicar knockback
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
        {
            if (i < heartImages.Count)
            {
                heartImages[i].sprite = i < currentHealth ? fullHeart : emptyHeart;
            }
        }
    }

    private void Die()
    {
        Debug.Log($"{playerType} ha muerto!");
        // Aquí puedes agregar lógica de muerte
        // Ejemplo: Desactivar control, animación de muerte, etc.
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHeartsUI();
        StopAllCoroutines();
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        isInvincible = false;
        lastDamageTime = 0f;
        if (Gamepad.current?.added == true) Gamepad.current.SetMotorSpeeds(0f, 0f);
    }

    private void OnDestroy()
    {
        if (Gamepad.current?.added == true) Gamepad.current.SetMotorSpeeds(0f, 0f);
    }

    // Dibujar gizmo para visualizar el área de detección de daño
    private void OnDrawGizmosSelected()
    {
        // Área de detección de daño (SOLO VISUAL)
        Gizmos.color = damageGizmoColor;
        Gizmos.DrawWireSphere(transform.position, damageDetectionRadius);

        // Indicador visual más pequeño en el centro
        Gizmos.color = new Color(damageGizmoColor.r, damageGizmoColor.g, damageGizmoColor.b, 0.3f);
        Gizmos.DrawSphere(transform.position, damageDetectionRadius * 0.1f);
    }

    // Dibujar siempre el gizmo (opcional, para verlo siempre)
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = new Color(damageGizmoColor.r, damageGizmoColor.g, damageGizmoColor.b, 0.1f);
        Gizmos.DrawSphere(transform.position, damageDetectionRadius);
    }

    // Método para actualizar la posición UI cuando cambia el splitscreen
    public void UpdateUIPosition()
    {
        if (uiPositioner != null)
        {
            FindAndAssignUIPanel();
            PositionHeartsInPanel();
        }
    }

    public bool IsInvincible => isInvincible;
    public PlayerType GetPlayerType() => playerType;
}