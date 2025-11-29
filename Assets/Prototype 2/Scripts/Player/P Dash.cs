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

    private Rigidbody2D rb;
    private PlayerController player;
    private Animator animator;
    private Gamepad playerGamepad;

    private bool isDashing;
    private bool canDash = true;
    private int dashesRemaining;
    private float dashTimeCounter;
    private float dashCooldownTimer;
    private Vector2 dashDirection;
    private float vibrationTimer;
    private bool triggerReady = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        dashesRemaining = maxDashes;

        FindGamepad();
    }

    void FindGamepad()
    {
        if (Gamepad.all.Count > 0)
        {
            if (player.isPlayer1)
            {
                playerGamepad = Gamepad.all[0];
            }
            else
            {
                playerGamepad = Gamepad.all.Count > 1 ? Gamepad.all[1] : Gamepad.all[0];
            }
            Debug.Log($"Mando asignado: {playerGamepad.name}");
        }
    }

    void Update()
    {
        if (!isDashUnlocked) return;

        // Solo buscar mando si no tenemos uno asignado
        if (playerGamepad == null && Gamepad.all.Count > 0)
        {
            FindGamepad();
        }

        // Detectar input de dash
        if (CheckDashInput() && canDash && dashesRemaining > 0 && dashCooldownTimer <= 0)
        {
            StartDash();
        }

        UpdateDash();
        UpdateVibration();
    }

    bool CheckDashInput()
    {
        // INPUT DE TECLADO - SIEMPRE DISPONIBLE
        if (player.isPlayer1 && Input.GetKeyDown(KeyCode.LeftShift))
            return true;

        if (!player.isPlayer1 && Input.GetKeyDown(KeyCode.Comma))
            return true;

        // INPUT DE MANDO - SOLO SI HAY MANDO CONECTADO
        if (playerGamepad != null)
        {
            float triggerValue = playerGamepad.rightTrigger.ReadValue();

            // Si el gatillo supera el umbral Y está listo para detectar
            if (triggerValue >= triggerThreshold && triggerReady)
            {
                triggerReady = false;
                Debug.Log($"Dash por R2: {triggerValue:F2}");
                return true;
            }

            // Resetear cuando el gatillo se suelta
            if (triggerValue < 0.2f)
            {
                triggerReady = true;
            }

            // También detectar R1
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

        // Dirección del dash
        float horizontalInput = player.GetHorizontalInput();
        dashDirection = horizontalInput != 0 ?
            new Vector2(horizontalInput, 0) :
            new Vector2(transform.localScale.x, 0);

        // Aplicar dash
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.linearVelocity = dashDirection * dashSpeed;

        // Vibración
        StartVibration();

        // Animación
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
            {
                StopVibration();
            }
        }
    }

    void StopVibration()
    {
        if (playerGamepad != null)
        {
            playerGamepad.SetMotorSpeeds(0f, 0f);
        }
    }

    void OnDisable()
    {
        StopVibration();
    }

    public bool IsDashing() => isDashing;
}