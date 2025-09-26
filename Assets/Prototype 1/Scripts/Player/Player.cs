using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float jumpDuration = 0.4f;
    public float invulnerabilityTime = 0.5f;
    public KeyCode jumpKey = KeyCode.Space;
    public AudioClip jumpSound;
    public AudioClip deathSound;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isJumping = false;
    private bool isInvulnerable = false;
    private AudioSource audioSource;
    private Vector3 originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        originalScale = transform.localScale;
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (Input.GetKeyDown(jumpKey) && !isJumping) StartCoroutine(Jump());
    }

    void FixedUpdate()
    {
        if (movement != Vector2.zero)
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    System.Collections.IEnumerator Jump()
    {
        isJumping = true;
        isInvulnerable = true;
        if (jumpSound != null) audioSource.PlayOneShot(jumpSound);

        Vector2 jumpMovement = movement != Vector2.zero ? movement : Vector2.up;
        rb.AddForce(jumpMovement * jumpForce, ForceMode2D.Impulse);

        transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(jumpDuration);

        transform.localScale = originalScale;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(invulnerabilityTime - jumpDuration);
        isInvulnerable = false;
        isJumping = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hit") && !isInvulnerable) Die();
    }

    void Die()
    {
        if (deathSound != null) audioSource.PlayOneShot(deathSound);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public Vector2 GetMovementDirection() => movement;
    public bool IsJumping() => isJumping;
}