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
    public float coyoteTime = 0.1f;

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

    private PDash dashComponent;
    private PJumps jumpComponent;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Gamepad gamepad;

    [SerializeField] private ParticleSystem particulas;

    public bool isGrounded { get; private set; }
    private bool isJumping;
    private float jumpTimeCounter;
    private float horizontalInput;
    private bool wasGrounded;
    private float jumpStartY;
    private float jumpHeight;
    private float coyoteTimeCounter;
    private bool usingCoyoteTime; // Nuevo: para detectar si estamos usando Coyote Time

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dashComponent = GetComponent<PDash>();
        jumpComponent = GetComponent<PJumps>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (animator == null) animator = GetComponent<Animator>();

        AsignarGamepad();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 4f;
        }

        wasGrounded = isGrounded;
    }

    void AsignarGamepad()
    {
        if (Gamepad.all.Count > 0)
        {
            int index = isPlayer1 ? 0 : Mathf.Min(1, Gamepad.all.Count - 1);
            gamepad = Gamepad.all[index];
        }
    }

    void Update()
    {
        if (gamepad == null && Gamepad.all.Count > 0) AsignarGamepad();

        ObtenerInput();
        ActualizarCoyoteTime();
        ManejarSalto();
        ManejarSaltoMantenido();
        ActualizarAnimaciones();
        VerificarVibracionAterrizar();
    }

    void FixedUpdate()
    {
        if (!EstaEnDash()) ManejarMovimiento();
        VerificarSuelo();
    }

    void ObtenerInput()
    {
        float inputTeclado = 0f;
        float inputGamepad = 0f;

        if (isPlayer1)
        {
            if (Input.GetKey(KeyCode.A)) inputTeclado -= 1f;
            if (Input.GetKey(KeyCode.D)) inputTeclado += 1f;
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftArrow)) inputTeclado -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) inputTeclado += 1f;
        }

        if (gamepad != null)
            inputGamepad = Mathf.Abs(gamepad.leftStick.x.ReadValue()) > 0.1f ? gamepad.leftStick.x.ReadValue() : 0f;

        horizontalInput = Mathf.Abs(inputGamepad) > 0.1f ? inputGamepad : inputTeclado;
    }

    void ManejarMovimiento()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (horizontalInput > 0.1f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < -0.1f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void ActualizarCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            usingCoyoteTime = false; // Resetear cuando toca el suelo
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    void ManejarSalto()
    {
        bool inputSalto = DetectarInputSalto();
        bool soltarSalto = DetectarSoltarSalto();

        // Salto normal cuando está en el suelo
        if (inputSalto && isGrounded && !EstaEnDash())
        {
            IniciarSaltoNormal();
            usingCoyoteTime = false;
        }
        // Salto con Coyote Time cuando no está en el suelo pero tiene tiempo
        else if (inputSalto && coyoteTimeCounter > 0 && !EstaEnDash())
        {
            IniciarSaltoCoyote();
            coyoteTimeCounter = 0;
            usingCoyoteTime = true; // Marcar que estamos usando Coyote Time
        }

        // Cortar salto
        if (soltarSalto && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * cutJumpHeight);
            isJumping = false;
            jumpHeight = transform.position.y - jumpStartY;
        }
    }

    void ManejarSaltoMantenido()
    {
        bool saltoMantenido = DetectarSaltoMantenido();

        // Solo permitir salto mantenido si NO estamos usando Coyote Time
        if (isJumping && saltoMantenido && !usingCoyoteTime && (!EstaEnDash() || PuedeSaltarDuranteDash()))
        {
            if (jumpTimeCounter > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;

                if (particulas != null) particulas.Play();
            }
            else
            {
                isJumping = false;
            }
        }

        if (isGrounded)
        {
            isJumping = false;
            usingCoyoteTime = false; // Resetear al tocar el suelo
        }
    }

    void IniciarSaltoNormal()
    {
        isJumping = true;
        jumpTimeCounter = jumpTime;

        // Salto normal con posibilidad de mantenerse
        Vector2 currentVelocity = rb.linearVelocity;
        currentVelocity.y = jumpForce;
        rb.linearVelocity = currentVelocity;

        jumpStartY = transform.position.y;

        if (particulas != null) particulas.Play();
        if (audioSource != null && jumpSound != null) audioSource.PlayOneShot(jumpSound);
    }

    void IniciarSaltoCoyote()
    {
        isJumping = false; // Importante: NO activar isJumping para Coyote Time
        jumpTimeCounter = 0; // No permitir salto mantenido

        // Salto simple sin mantenimiento
        Vector2 currentVelocity = rb.linearVelocity;
        currentVelocity.y = 0; // Resetear velocidad vertical
        currentVelocity.y = jumpForce; // Aplicar fuerza de salto simple
        rb.linearVelocity = currentVelocity;

        jumpStartY = transform.position.y;

        if (particulas != null) particulas.Play();
        if (audioSource != null && jumpSound != null) audioSource.PlayOneShot(jumpSound);
    }

    bool DetectarInputSalto()
    {
        if (isPlayer1)
            return Input.GetKeyDown(KeyCode.W) || (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
        else
            return Input.GetKeyDown(KeyCode.UpArrow) || (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
    }

    bool DetectarSoltarSalto()
    {
        if (isPlayer1)
            return Input.GetKeyUp(KeyCode.W) || (gamepad != null && gamepad.buttonSouth.wasReleasedThisFrame);
        else
            return Input.GetKeyUp(KeyCode.UpArrow) || (gamepad != null && gamepad.buttonSouth.wasReleasedThisFrame);
    }

    bool DetectarSaltoMantenido()
    {
        if (isPlayer1)
            return Input.GetKey(KeyCode.W) || (gamepad != null && gamepad.buttonSouth.isPressed);
        else
            return Input.GetKey(KeyCode.UpArrow) || (gamepad != null && gamepad.buttonSouth.isPressed);
    }

    public bool PuedeSaltarDuranteDash()
    {
        return EstaEnDash() && jumpComponent != null && !jumpComponent.HasUsedAirJump();
    }

    void ActualizarAnimaciones()
    {
        if (animator == null) return;

        if (!EstaEnDash())
        {
            if (Mathf.Abs(horizontalInput) > 0.1f && isGrounded)
                animator.SetTrigger("Run");
            else if (isGrounded)
                animator.SetTrigger("Idle");
        }
    }

    void VerificarSuelo()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
    }

    bool EstaEnDash() => dashComponent != null && dashComponent.IsDashing();

    void VerificarVibracionAterrizar()
    {
        if (isGrounded && !wasGrounded)
        {
            if (jumpHeight == 0)
                jumpHeight = transform.position.y - jumpStartY;

            if (gamepad != null)
            {
                float intensidad = jumpHeight > 3f ? bigJumpVibration : smallJumpVibration;
                StartCoroutine(VibrarControl(intensidad, vibrationDuration));
            }

            jumpHeight = 0;
        }

        wasGrounded = isGrounded;
    }

    private System.Collections.IEnumerator VibrarControl(float intensidad, float duracion)
    {
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(intensidad, intensidad);
            yield return new WaitForSeconds(duracion);
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }

    void OnDestroy()
    {
        if (gamepad != null)
            gamepad.SetMotorSpeeds(0f, 0f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, 0.05f);
    }

    public void ReasignarGamepad(int nuevoIndice)
    {
        if (nuevoIndice < Gamepad.all.Count)
            gamepad = Gamepad.all[nuevoIndice];
    }

    public float GetHorizontalInput() => horizontalInput;
}