using UnityEngine;

public class Player2 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 16f;
    public float jumpTime = 0.35f;
    public float groundCheckDistance = 0.6f;
    public float cutJumpHeight = 0.5f;

    [Header("Physics")]
    public LayerMask groundLayer;

    [Header("Animation")]
    public Animator animator;

    [Header("Sound")]
    public AudioClip jumpSound; 

    private PDash dashComponent;
    private Rigidbody2D rb;
    public bool isGrounded;
    private bool isJumping;
    private float jumpTimeCounter;
    private float horizontalInput;
    private AudioSource audioSource;

    private PJumps jumpComponent;
    [SerializeField] private ParticleSystem particulas;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dashComponent = GetComponent<PDash>();
        jumpComponent = GetComponent<PJumps>();
        if (animator == null) animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 4f;
        }
    }

    void Update()
    {
        GetInput();
        HandleJumpInput();
        HandleJumpHold();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (!IsDashing()) HandleMovement();
        CheckGrounded();
    }

    void GetInput() => horizontalInput = Input.GetAxisRaw("Horizontal");

    void HandleMovement()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        if (horizontalInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !IsDashing())
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            particulas.Play();
            if (jumpSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
        }

        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * cutJumpHeight);
            isJumping = false;
        }
    }

    void HandleJumpHold()
    {
        if (isJumping && !IsDashing())
        {
            if (jumpTimeCounter > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
                particulas.Play();
            }
            else isJumping = false;
        }
        if (isGrounded) isJumping = false;
    }

    public bool CanJumpDuringDash()
    {
        return IsDashing() && jumpComponent != null && !jumpComponent.HasUsedAirJump();
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        if (!IsDashing())
        {
            if (Mathf.Abs(horizontalInput) > 0.1f && isGrounded) animator.SetTrigger("Run");
            else if (isGrounded) animator.SetTrigger("Idle");
        }
    }

    void CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
    }

    bool IsDashing() => dashComponent != null && dashComponent.IsDashing();

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, 0.05f);
    }
}