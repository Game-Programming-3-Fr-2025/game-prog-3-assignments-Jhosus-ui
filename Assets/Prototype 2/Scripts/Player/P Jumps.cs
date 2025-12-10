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

    [Header("Visual Effects")]
    public Color airJumpColor = Color.cyan;
    public Color pointJumpColor = Color.yellow;
    public float effectDuration = 0.3f;
    public float trailTime = 0.2f;

    private Rigidbody2D rb;
    private PlayerController player;
    private PDash dashComponent;
    private AudioSource audioSource;
    private Gamepad gamepad;
    private SpriteRenderer sprite;
    private TrailRenderer trail;
    private Color originalColor;

    private bool hasUsedAirJump = false;
    private bool wasGrounded = true;
    private bool inPointTrigger = false;
    private float pointBufferTimer = 0f;
    private float effectTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerController>();
        dashComponent = GetComponent<PDash>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) originalColor = sprite.color;

        ConfigureTrail();
        AsignarGamepad();
    }

    void ConfigureTrail()
    {
        trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = trailTime;
            trail.startWidth = 0.3f;
            trail.endWidth = 0.05f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
        }
        trail.enabled = false;
    }

    void AsignarGamepad()
    {
        if (Gamepad.all.Count > 0)
        {
            int index = player.isPlayer1 ? 0 : Mathf.Min(1, Gamepad.all.Count - 1); //Asignar gamepad segun jugador
            gamepad = Gamepad.all[index];
        }
    }

    void Update()
    {
        if (gamepad == null && Gamepad.all.Count > 0) AsignarGamepad();

        if (isDoubleJumpUnlocked) ManejarSaltoAereo();

        ActualizarReseteoSalto();
        ActualizarEfectos();

        if (pointBufferTimer > 0f) //Actualizar un buffer de punto
            pointBufferTimer -= Time.deltaTime;
    }

    void ManejarSaltoAereo()
    {
        bool inputSalto = DetectarInputSalto();

        if (inputSalto)
        {
            if (inPointTrigger || pointBufferTimer > 0f) // Realizar Point Jump
            {
                RealizarPointJump();
                pointBufferTimer = 0f;
            }
            else if (PuedeSaltarEnAire())
            {
                RealizarSaltoAereo();
            }
        }
    }

    bool DetectarInputSalto()
    {
        if (player.isPlayer1 && Input.GetKeyDown(KeyCode.W)) return true;
        if (!player.isPlayer1 && Input.GetKeyDown(KeyCode.UpArrow)) return true;
        if (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame) return true;
        return false;
    }

    bool PuedeSaltarEnAire()
    {
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

        // Efectos visuales
        ActivarEfectos(airJumpColor);

        if (airJumpSound != null && audioSource != null)
            audioSource.PlayOneShot(airJumpSound);
    }

    void RealizarPointJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, pointJumpForce);

        if (isDoubleJumpUnlocked)
            hasUsedAirJump = false;

        // Efectos visuales
        ActivarEfectos(pointJumpColor);

        if (pointJumpSound != null && audioSource != null)
            audioSource.PlayOneShot(pointJumpSound);
    }

    void ActivarEfectos(Color color)
    {
        effectTimer = effectDuration;

        // Trail
        if (trail != null)
        {
            trail.startColor = color;
            trail.endColor = new Color(color.r, color.g, color.b, 0f);
            trail.enabled = true;
            trail.Clear();
        }

        // Color del sprite probemos aqui (experimentemos)
        if (sprite != null)
            sprite.color = color;
    }

    void ActualizarEfectos()
    {
        if (effectTimer > 0f)
        {
            effectTimer -= Time.deltaTime;
            
            if (effectTimer <= 0f)
            {
                // Desactivar efectos
                if (trail != null)
                    trail.enabled = false;
                
                if (sprite != null)
                    sprite.color = originalColor;
            }
        }
    }

    void ActualizarReseteoSalto()
    {
        if (player.isGrounded && !wasGrounded)
            hasUsedAirJump = false;

        wasGrounded = player.isGrounded;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!isDoubleJumpUnlocked &&
            doubleJumpActivatorObject != null &&
            other.gameObject == doubleJumpActivatorObject) //Activemos disponibilidad de doble salto
        {
            isDoubleJumpUnlocked = true;
            doubleJumpActivatorObject.SetActive(false);
        }

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

    void OnDisable()
    {
        if (trail != null)
            trail.enabled = false;
        if (sprite != null)
            sprite.color = originalColor;
    }

    public bool HasUsedAirJump() => hasUsedAirJump; //Tengamos para saber si ha usado el salto aereo
}