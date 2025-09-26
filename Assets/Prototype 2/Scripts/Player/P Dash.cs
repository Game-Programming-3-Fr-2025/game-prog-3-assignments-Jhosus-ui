using UnityEngine;

public class PDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public int maxDashes = 1;

    [Header("Dash Unlock System")]
    public bool isDashUnlocked = false;
    public GameObject dashActivatorObject;

    private Rigidbody2D rb;
    private Player2 player;
    private Animator animator;

    private bool isDashing;
    private bool canDash = true;
    private int dashesRemaining;
    private float dashTimeCounter;
    private float dashCooldownTimer;
    private Vector2 dashDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player2>();
        animator = GetComponent<Animator>();
        dashesRemaining = maxDashes;
    }

    void Update()
    {
        if (!isDashUnlocked) return;

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.C)) &&
            canDash && dashesRemaining > 0 && dashCooldownTimer <= 0)
        {
            StartDash();
        }

        UpdateDash();
    }

    void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashesRemaining--;
        dashTimeCounter = dashDuration;
        dashCooldownTimer = dashCooldown;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        dashDirection = horizontalInput != 0 ?
            new Vector2(horizontalInput, 0) :
            new Vector2(transform.localScale.x, 0);

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        if (animator != null) animator.SetTrigger("Sprint");
    }

    void UpdateDash()
    {
        if (isDashing)
        {
            if (dashTimeCounter > 0)
            {
                rb.linearVelocity = dashDirection * dashSpeed;
                dashTimeCounter -= Time.deltaTime;
            }
            else
            {
                isDashing = false;
                rb.gravityScale = 4f;
            }
        }

        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        if (player.isGrounded && dashesRemaining < maxDashes)
        {
            dashesRemaining = maxDashes;
            canDash = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDashUnlocked && other.gameObject == dashActivatorObject)
        {
            isDashUnlocked = true;
            dashActivatorObject.SetActive(false);
        }
    }

    public bool IsDashing() => isDashing;
}