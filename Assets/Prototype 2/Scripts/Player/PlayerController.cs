using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public bool isPlayer1 = true;

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

    [Header("Vibration Settings")]
    public float smallJumpVibration = 0.2f;
    public float bigJumpVibration = 0.4f;
    public float vibrationDuration = 0.2f;

    // Variables para vibración
    private bool wasGrounded;
    private float jumpStartY;
    private float jumpHeight;

    private PDash dashComponent;
    private Rigidbody2D rb;
    public bool isGrounded;
    private bool isJumping;
    private float jumpTimeCounter;
    private float horizontalInput;
    private AudioSource audioSource;

    private PJumps jumpComponent;
    [SerializeField] private ParticleSystem particulas;

    // Variables para el control con mando
    public Gamepad gamepad;
    private int gamepadIndex = -1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dashComponent = GetComponent<PDash>();
        jumpComponent = GetComponent<PJumps>();
        if (animator == null) animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Asignar mandos
        AssignGamepad();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 4f;
        }

        wasGrounded = isGrounded;
    }

    void AssignGamepad()
    {
        if (Gamepad.all.Count > 0)
        {
            if (isPlayer1)
            {
                // Player 1: primer mando conectado
                gamepadIndex = 0;
                if (Gamepad.all.Count > gamepadIndex)
                    gamepad = Gamepad.all[gamepadIndex];
            }
            else
            {
                // Player 2: segundo mando (si existe) o mismo que Player 1
                gamepadIndex = Gamepad.all.Count > 1 ? 1 : 0;
                if (Gamepad.all.Count > gamepadIndex)
                    gamepad = Gamepad.all[gamepadIndex];
            }
        }
    }

    void Update()
    {
        // Re-asignar mando si se desconectó
        if (gamepad == null || (gamepadIndex >= 0 && gamepadIndex < Gamepad.all.Count && Gamepad.all[gamepadIndex] != gamepad))
        {
            AssignGamepad();
        }

        GetInput();
        HandleJumpInput();
        HandleJumpHold();
        UpdateAnimations();
        CheckLandingVibration();
    }

    void FixedUpdate()
    {
        if (!IsDashing()) HandleMovement();
        CheckGrounded();
    }

    void GetInput()
    {
        float keyboardInput = 0f;
        float gamepadInput = 0f;

        // INPUT DE TECLADO
        if (isPlayer1)
        {
            // Player 1: A/D
            if (Input.GetKey(KeyCode.A)) keyboardInput -= 1f;
            if (Input.GetKey(KeyCode.D)) keyboardInput += 1f;
        }
        else
        {
            // Player 2: Flechas
            if (Input.GetKey(KeyCode.LeftArrow)) keyboardInput -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) keyboardInput += 1f;
        }

        // INPUT DE MANDO (PlayStation)
        if (gamepad != null)
        {
            // Stick izquierdo para movimiento
            gamepadInput = Mathf.Abs(gamepad.leftStick.x.ReadValue()) > 0.1f ? gamepad.leftStick.x.ReadValue() : 0f;
        }

        // Combinar inputs (priorizar mando si hay input significativo)
        if (Mathf.Abs(gamepadInput) > 0.1f)
            horizontalInput = gamepadInput;
        else
            horizontalInput = keyboardInput;
    }

    void HandleMovement()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Flip del sprite según dirección
        if (horizontalInput > 0.1f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < -0.1f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void HandleJumpInput()
    {
        bool jumpInput = false;

        // DETECTAR INPUT DE SALTO
        if (isPlayer1)
        {
            // Player 1: Tecla W o botón X de PlayStation (cruz)
            jumpInput = Input.GetKeyDown(KeyCode.W) ||
                       (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
        }
        else
        {
            // Player 2: Flecha arriba o botón X de PlayStation (cruz)
            jumpInput = Input.GetKeyDown(KeyCode.UpArrow) ||
                       (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
        }

        if (jumpInput && isGrounded && !IsDashing())
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            if (particulas != null)
                particulas.Play();

            // Guardar posición inicial del salto
            jumpStartY = transform.position.y;

            if (jumpSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
        }

        // DETECTAR LIBERACIÓN DE SALTO
        bool jumpReleased = false;

        if (isPlayer1)
        {
            jumpReleased = Input.GetKeyUp(KeyCode.W) ||
                          (gamepad != null && gamepad.buttonSouth.wasReleasedThisFrame);
        }
        else
        {
            jumpReleased = Input.GetKeyUp(KeyCode.UpArrow) ||
                          (gamepad != null && gamepad.buttonSouth.wasReleasedThisFrame);
        }

        if (jumpReleased && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * cutJumpHeight);
            isJumping = false;

            // Calcular altura del salto
            jumpHeight = transform.position.y - jumpStartY;
        }
    }

    void HandleJumpHold()
    {
        bool jumpHeld = false;

        // DETECTAR SALTO MANTENIDO
        if (isPlayer1)
        {
            jumpHeld = Input.GetKeyDown(KeyCode.W) ||
                      (gamepad != null && gamepad.buttonSouth.isPressed);
        }
        else
        {
            jumpHeld = Input.GetKeyDown(KeyCode.UpArrow) ||
                      (gamepad != null && gamepad.buttonSouth.isPressed);
        }

        if (isJumping && jumpHeld && !IsDashing())
        {
            if (jumpTimeCounter > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;

                if (particulas != null)
                    particulas.Play();
            }
            else
            {
                isJumping = false;
            }
        }

        if (isGrounded)
        {
            isJumping = false;
        }
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
            if (Mathf.Abs(horizontalInput) > 0.1f && isGrounded)
                animator.SetTrigger("Run");
            else if (isGrounded)
                animator.SetTrigger("Idle");
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

    void CheckLandingVibration()
    {
        // Detectar cuando aterriza
        if (isGrounded && !wasGrounded)
        {
            // Calcular la altura final del salto si no se calculó antes
            if (jumpHeight == 0)
            {
                jumpHeight = transform.position.y - jumpStartY;
            }

            // Aplicar vibración según la altura del salto
            if (gamepad != null)
            {
                float vibrationIntensity = smallJumpVibration;

                // Si el salto fue alto, usar vibración más fuerte
                if (jumpHeight > 3f)
                {
                    vibrationIntensity = bigJumpVibration;
                }

                StartCoroutine(VibrateController(vibrationIntensity, vibrationDuration));
            }

            // Resetear altura del salto
            jumpHeight = 0;
        }

        wasGrounded = isGrounded;
    }

    private System.Collections.IEnumerator VibrateController(float intensity, float duration)
    {
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(intensity, intensity);
            yield return new WaitForSeconds(duration);
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }

    void OnDestroy()
    {
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }

    // Método público para cambiar el mando asignado
    public void ReassignGamepad(int newIndex)
    {
        if (newIndex < Gamepad.all.Count)
        {
            gamepadIndex = newIndex;
            gamepad = Gamepad.all[newIndex];
        }
    }

    public float GetHorizontalInput()
    {
        return horizontalInput;
    }
}