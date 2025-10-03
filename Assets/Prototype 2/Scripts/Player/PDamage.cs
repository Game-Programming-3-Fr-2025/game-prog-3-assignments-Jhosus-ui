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

    [Header("Sound")]
    public AudioClip damageSound;

    private int currentLives;
    private bool isInvulnerable;
    private float invulnerabilityTimer;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private FC2 cameraController;
    private AudioSource audioSource;

    void Start()
    {
        currentLives = maxLives;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        cameraController = Camera.main.GetComponent<FC2>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hit") && !isInvulnerable)
        {
            Vector2 hitDirection = (transform.position - other.transform.position).normalized;
            TakeDamage(hitDirection);
        }
    }

    void TakeDamage(Vector2 hitDirection)
    {
        currentLives--;
        UpdateLivesUI();

        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        if (currentLives <= 0)
        {
            GameOver();
            return;
        }

        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityTime;

        ApplyKnockback(hitDirection);

        if (cameraController != null)
        {
            cameraController.CameraShake();
        }
    }

    void ApplyKnockback(Vector2 hitDirection)
    {
        Vector2 knockbackDirection = new Vector2(hitDirection.x, 0.5f).normalized;
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