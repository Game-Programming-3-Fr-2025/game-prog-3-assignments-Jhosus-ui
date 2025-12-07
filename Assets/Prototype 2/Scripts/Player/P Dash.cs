using UnityEngine;
using UnityEngine.InputSystem;

public class PDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public int maxDashes = 1;

    [Header("Vibration Settings")]
    public float vibrationIntensity = 0.8f;
    public float vibrationDuration = 0.3f;

    [Header("Trigger Settings")]
    public float triggerThreshold = 0.7f;

    [Header("Dash Unlock System")]
    public bool isDashUnlocked = true;
    public GameObject dashActivatorObject;

    [Header("Visual Effects")]
    public Color dashColor = Color.cyan;
    public float dashTrailTime = 0.1f;
    public float dashTrailWidth = 0.5f;

    private Rigidbody2D rb;
    private PlayerController player;
    private Animator animator;
    private Gamepad playerGamepad;
    private TrailRenderer trail;
    private SpriteRenderer sprite;
    private Color originalColor;

    private bool isDashing;
    private bool canDash = true;
    private int dashesRemaining;
    private float dashTimeCounter;
    private float dashCooldownTimer;
    private float vibrationTimer;
    private bool triggerReady = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        dashesRemaining = maxDashes;

        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) originalColor = sprite.color;

        trail = GetComponent<TrailRenderer>();
        if (trail == null) trail = gameObject.AddComponent<TrailRenderer>();

        ConfigureTrailRenderer();
        FindGamepad();
    }

    void ConfigureTrailRenderer()
    {
        if (trail != null)
        {
            trail.time = dashTrailTime;
            trail.startWidth = dashTrailWidth;
            trail.endWidth = 0.1f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = dashColor;
            trail.endColor = new Color(dashColor.r, dashColor.g, dashColor.b, 0f);
            trail.enabled = false;
        }
    }

    void FindGamepad()
    {
        if (Gamepad.all.Count > 0)
        {
            playerGamepad = player.isPlayer1 ? Gamepad.all[0] :
                           (Gamepad.all.Count > 1 ? Gamepad.all[1] : Gamepad.all[0]);
            Debug.Log($"Mando asignado: {playerGamepad.name}");
        }
    }

    void Update()
    {
        if (!isDashUnlocked) return;

        if (playerGamepad == null && Gamepad.all.Count > 0)
            FindGamepad();

        if (CheckDashInput() && canDash && dashesRemaining > 0 && dashCooldownTimer <= 0)
            StartDash();

        UpdateDash();
        UpdateVibration();
    }

    bool CheckDashInput()
    {
        // Teclado
        if (player.isPlayer1 && Input.GetKeyDown(KeyCode.LeftShift)) return true;
        if (!player.isPlayer1 && Input.GetKeyDown(KeyCode.Comma)) return true;

        // Gamepad
        if (playerGamepad != null)
        {
            float triggerValue = playerGamepad.rightTrigger.ReadValue();

            if (triggerValue >= triggerThreshold && triggerReady)
            {
                triggerReady = false;
                Debug.Log($"Dash por R2: {triggerValue:F2}");
                return true;
            }

            if (triggerValue < 0.2f)
                triggerReady = true;

            if (playerGamepad.rightShoulder.ReadValue() > 0.5f && triggerReady)
            {
                triggerReady = false;
                Debug.Log("Dash por R1");
                return true;
            }
        }

        return false;
    }

    void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashesRemaining--;
        dashTimeCounter = dashDuration;
        dashCooldownTimer = dashCooldown;

        float horizontalInput = player.GetHorizontalInput();
        Vector2 dashDirection = horizontalInput != 0 ?
            new Vector2(horizontalInput, 0) :
            new Vector2(transform.localScale.x, 0);

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.linearVelocity = dashDirection * dashSpeed;

        ApplyDashVisualEffects(true);
        StartVibration();

        if (animator != null)
            animator.SetTrigger("Sprint");
    }

    void UpdateDash()
    {
        if (isDashing)
        {
            dashTimeCounter -= Time.deltaTime;
            if (dashTimeCounter <= 0)
            {
                isDashing = false;
                rb.gravityScale = 4f;
                ApplyDashVisualEffects(false);
            }
        }

        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        if (player.isGrounded && dashesRemaining < maxDashes)
        {
            dashesRemaining = maxDashes;
            canDash = true;
        }
    }

    void ApplyDashVisualEffects(bool activate)
    {
        if (trail != null)
        {
            trail.enabled = activate;
            if (activate) trail.Clear();
        }

        if (sprite != null)
            sprite.color = activate ? dashColor : originalColor;
    }

    void StartVibration()
    {
        if (playerGamepad != null)
        {
            vibrationTimer = vibrationDuration;
            playerGamepad.SetMotorSpeeds(vibrationIntensity, vibrationIntensity);
        }
    }

    void UpdateVibration()
    {
        if (vibrationTimer > 0)
        {
            vibrationTimer -= Time.deltaTime;
            if (vibrationTimer <= 0)
                StopVibration();
        }
    }

    void StopVibration()
    {
        if (playerGamepad != null)
            playerGamepad.SetMotorSpeeds(0f, 0f);
    }

    void OnDisable()
    {
        StopVibration();
        ApplyDashVisualEffects(false);
    }

    public bool IsDashing() => isDashing;
}