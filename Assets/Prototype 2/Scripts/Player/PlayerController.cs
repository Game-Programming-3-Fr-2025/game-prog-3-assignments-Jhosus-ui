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

    [SerializeField] private ParticleSystem particulas;

    private PDash dashComponent;
    private PJumps jumpComponent;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Gamepad gamepad;

    public bool isGrounded { get; private set; }
    private bool isJumping;
    private float jumpTimeCounter;
    private float horizontalInput;
    private bool wasGrounded;
    private float jumpStartY;
    private float jumpHeight;
    private float coyoteTimeCounter;
    private bool usingCoyoteTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dashComponent = GetComponent<PDash>();
        jumpComponent = GetComponent<PJumps>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        animator = animator ?? GetComponent<Animator>();

        AsignarGamepad();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 4f;
        }

        wasGrounded = isGrounded;
    }

    void AsignarGamepad() // Agregar Sistema De Control
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

        KeyCode left = isPlayer1 ? KeyCode.A : KeyCode.LeftArrow;
        KeyCode right = isPlayer1 ? KeyCode.D : KeyCode.RightArrow;

        if (Input.GetKey(left)) inputTeclado -= 1f;
        if (Input.GetKey(right)) inputTeclado += 1f;

        if (gamepad != null) // Por si no hay gamepad conectado
        {
            float stickX = gamepad.leftStick.x.ReadValue();
            inputGamepad = Mathf.Abs(stickX) > 0.1f ? stickX : 0f;
        }

        horizontalInput = Mathf.Abs(inputGamepad) > 0.1f ? inputGamepad : inputTeclado; // Priorizar gamepad sobre teclado
    }

    void ManejarMovimiento()  // Movimiento Horizontal
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(horizontalInput) > 0.1f)
            transform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1, 1);
    }

    void ActualizarCoyoteTime() // Tiempo de Coyote
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            usingCoyoteTime = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    void ManejarSalto() 
    {
        bool inputSalto = DetectarInput(InputType.Pressed);
        bool soltarSalto = DetectarInput(InputType.Released);

        if (inputSalto && !EstaEnDash()) // Iniciar Salto
        {
            if (isGrounded)
            {
                IniciarSaltoNormal();
                usingCoyoteTime = false;
            }
            else if (coyoteTimeCounter > 0)
            {
                IniciarSaltoCoyote();
                coyoteTimeCounter = 0;
                usingCoyoteTime = true;
            }
        }

        if (soltarSalto && rb.linearVelocity.y > 0) // Necesario para cortar el salto
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * cutJumpHeight);
            isJumping = false;
            jumpHeight = transform.position.y - jumpStartY;
        }
    }

    void ManejarSaltoMantenido()
    {
        if (isJumping && DetectarInput(InputType.Hold) && !usingCoyoteTime && (!EstaEnDash() || PuedeSaltarDuranteDash()))
        {
            if (jumpTimeCounter > 0) // Continuar salto mientras se mantenga el boton
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
            usingCoyoteTime = false;
        }
    }

    void IniciarSalto(bool permitirMantenido) // Sirve para iniciar el salto normal con tiempo de coyote
    {
        isJumping = permitirMantenido;
        jumpTimeCounter = permitirMantenido ? jumpTime : 0;

        Vector2 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;

        jumpStartY = transform.position.y;

        if (particulas != null) particulas.Play();
        if (audioSource != null && jumpSound != null) audioSource.PlayOneShot(jumpSound);
    }

    void IniciarSaltoNormal() => IniciarSalto(true);
    void IniciarSaltoCoyote() => IniciarSalto(false);

    enum InputType { Pressed, Released, Hold }

    bool DetectarInput(InputType tipo) //Detectar Controles de Salto
    {
        KeyCode key = isPlayer1 ? KeyCode.W : KeyCode.UpArrow;

        bool teclado = tipo switch 
        {
            InputType.Pressed => Input.GetKeyDown(key),
            InputType.Released => Input.GetKeyUp(key),
            InputType.Hold => Input.GetKey(key),
            _ => false
        };

        bool control = gamepad != null && tipo switch // Por si no hay gamepad conectado
        {
            InputType.Pressed => gamepad.buttonSouth.wasPressedThisFrame,
            InputType.Released => gamepad.buttonSouth.wasReleasedThisFrame,
            InputType.Hold => gamepad.buttonSouth.isPressed,
            _ => false
        }; 

        return teclado || control;
    }

    public bool PuedeSaltarDuranteDash() => EstaEnDash() && jumpComponent != null && !jumpComponent.HasUsedAirJump(); // Permitir salto durante dash si tiene salto en aire

    void ActualizarAnimaciones() 
    {
        if (animator == null || EstaEnDash()) return;

        bool isMoving = Mathf.Abs(horizontalInput) > 0.1f;
        animator.SetTrigger(isGrounded && isMoving ? "Run" : "Idle");
    }

    void VerificarSuelo()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer); //Ayuda a verificar si el jugador esta en el suelo
        isGrounded = hit.collider != null;
    }

    bool EstaEnDash() => dashComponent != null && dashComponent.IsDashing(); // Verificar si el jugador esta en dash

    void VerificarVibracionAterrizar()
    {
        if (isGrounded && !wasGrounded)
        {
            if (jumpHeight == 0) jumpHeight = transform.position.y - jumpStartY;

            if (gamepad != null)
            {
                float intensidad = jumpHeight > 3f ? bigJumpVibration : smallJumpVibration;
                StartCoroutine(VibrarControl(intensidad, vibrationDuration)); // Vibrar al aterrizar
            }

            jumpHeight = 0;
        }

        wasGrounded = isGrounded;
    }

    private System.Collections.IEnumerator VibrarControl(float intensidad, float duracion) // Coroutine para manejar la vibración
    {
        if (gamepad != null) // Por si no hay gamepad conectado
        {
            gamepad.SetMotorSpeeds(intensidad, intensidad);
            yield return new WaitForSeconds(duracion);
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }

    void OnDestroy()
    {
        if (gamepad != null) gamepad.SetMotorSpeeds(0f, 0f); // Asegurarse de detener la vibración
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
        if (nuevoIndice < Gamepad.all.Count) gamepad = Gamepad.all[nuevoIndice]; // Reasignar gamepad manualmente
    }

    public float GetHorizontalInput() => horizontalInput; // Obtener el input horizontal actual
}