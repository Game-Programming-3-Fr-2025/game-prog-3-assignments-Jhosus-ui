using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PDamage : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxLives = 3;
    public float invulnerabilityTime = 1.5f;
    public float knockbackForce = 5f;

    [Header("UI Reference")]
    public TextMeshProUGUI livesText;

    private int currentLives;
    private bool isInvulnerable;
    private float invulnerabilityTimer;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private FC2 cameraController;

    void Start()
    {
        currentLives = maxLives;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        cameraController = Camera.main.GetComponent<FC2>();
        UpdateLivesUI();
    }

    void Update()
    {
        HandleInvulnerability();
    }

    void HandleInvulnerability()
    {
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            spriteRenderer.enabled = Mathf.Sin(Time.time * 20f) > 0;

            if (invulnerabilityTimer <= 0)
            {
                isInvulnerable = false;
                spriteRenderer.enabled = true;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Hit") && !isInvulnerable)
        {
            TakeDamage(collision.GetContact(0).normal);
        }
    }

    void TakeDamage(Vector2 hitNormal)
    {
        currentLives--;
        UpdateLivesUI();

        if (currentLives <= 0)
        {
            GameOver();
            return;
        }

        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityTime;

        ApplyKnockback(hitNormal);

        if (cameraController != null)
        {
            cameraController.CameraShake();
        }
    }

    void ApplyKnockback(Vector2 hitNormal)
    {
        Vector2 knockbackDirection = new Vector2(-hitNormal.x, 0.5f).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
    }
    void GameOver()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {currentLives}";
        }
    }
}