using UnityEngine;
using UnityEngine.InputSystem;

public class PJumps : MonoBehaviour
{
    [Header("Jump Settings")]
    public bool isDoubleJumpUnlocked = false;
    public float doubleJumpForce = 14f;
    public GameObject doubleJumpActivatorObject;

    [Header("Point Jump")]
    public float pointJumpForce = 18f;
    public float pointBufferTime = 0.2f;

    [Header("Jump Sounds")]
    public AudioClip airJumpSound;
    public AudioClip pointJumpSound;

    private Rigidbody2D rb;
    private PlayerController player;
    private PDash dashComponent;
    private AudioSource audioSource;
    private Gamepad gamepad;

    private bool hasUsedAirJump = false;
    private bool wasGrounded = true;
    private bool inPointTrigger = false;
    private float pointBufferTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerController>();
        dashComponent = GetComponent<PDash>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        AsignarGamepad();
    }

    void AsignarGamepad()
    {
        if (Gamepad.all.Count > 0)
        {
            int index = player.isPlayer1 ? 0 : Mathf.Min(1, Gamepad.all.Count - 1);
            gamepad = Gamepad.all[index];
        }
    }

    void Update()
    {
        if (gamepad == null && Gamepad.all.Count > 0) AsignarGamepad();

        if (isDoubleJumpUnlocked) ManejarSaltoAereo();

        ActualizarReseteoSalto();

        if (pointBufferTimer > 0f)
            pointBufferTimer -= Time.deltaTime;
    }

    void ManejarSaltoAereo()
    {
        bool inputSalto = DetectarInputSalto();

        if (inputSalto)
        {
            // Prioridad 1: Point Jump (si está en trigger o buffer activo)
            if (inPointTrigger || pointBufferTimer > 0f)
            {
                RealizarPointJump();
                pointBufferTimer = 0f;
            }
            // Prioridad 2: Salto aéreo (solo si puede hacerlo)
            else if (PuedeSaltarEnAire())
            {
                RealizarSaltoAereo();
            }
        }
    }

    bool DetectarInputSalto()
    {
        // Teclado
        if (player.isPlayer1 && Input.GetKeyDown(KeyCode.W)) return true;
        if (!player.isPlayer1 && Input.GetKeyDown(KeyCode.UpArrow)) return true;

        // Gamepad
        if (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame) return true;

        return false;
    }

    bool PuedeSaltarEnAire()
    {
        // Puede saltar en aire si:
        // 1. NO está en el suelo
        // 2. NO ha usado el salto aéreo
        // 3. NO está en dash (o puede saltar durante dash según PlayerController)
        bool estaEnDash = dashComponent != null && dashComponent.IsDashing();
        bool puedeSaltarDuranteDash = player != null && player.PuedeSaltarDuranteDash();

        return !player.isGrounded &&
               !hasUsedAirJump &&
               (!estaEnDash || puedeSaltarDuranteDash);
    }

    void RealizarSaltoAereo()
    {
        hasUsedAirJump = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);

        if (airJumpSound != null && audioSource != null)
            audioSource.PlayOneShot(airJumpSound);
    }

    void RealizarPointJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, pointJumpForce);

        // Point jump resetea el salto aéreo si está desbloqueado
        if (isDoubleJumpUnlocked)
            hasUsedAirJump = false;

        if (pointJumpSound != null && audioSource != null)
            audioSource.PlayOneShot(pointJumpSound);
    }

    void ActualizarReseteoSalto()
    {
        // Resetear salto aéreo al tocar el suelo
        if (player.isGrounded && !wasGrounded)
            hasUsedAirJump = false;

        wasGrounded = player.isGrounded;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Desbloquear doble salto
        if (!isDoubleJumpUnlocked &&
            doubleJumpActivatorObject != null &&
            other.gameObject == doubleJumpActivatorObject)
        {
            isDoubleJumpUnlocked = true;
            doubleJumpActivatorObject.SetActive(false);
        }

        // Detectar punto de salto
        if (other.CompareTag("Points"))
            inPointTrigger = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Points"))
        {
            inPointTrigger = false;
            pointBufferTimer = pointBufferTime;
        }
    }

    public bool HasUsedAirJump() => hasUsedAirJump;
}