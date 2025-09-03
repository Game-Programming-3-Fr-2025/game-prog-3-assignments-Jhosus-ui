using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Configuraci�n de Movimiento")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Asegurarse de que hay un Rigidbody2D
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // Sin gravedad
            rb.freezeRotation = true; // Congelar rotaci�n
        }
    }

    void Update()
    {
        // Input del jugador
        HandleInput();
    }

    void FixedUpdate()
    {
        // Movimiento f�sico
        HandleMovement();
    }

    void HandleInput()
    {
        // Input b�sico WASD o flechas
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        // Crear vector de movimiento 2D
        movement = new Vector2(moveHorizontal, moveVertical).normalized;
    }

    void HandleMovement()
    {
        // Mover el rigidbody2D
        if (movement != Vector2.zero)
        {
            Vector2 moveVelocity = movement * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + moveVelocity);
        }
    }

    // Opcional: Para animaciones futuras
    public Vector2 GetMovementDirection()
    {
        return movement;
    }

    // Opcional: Debug visual de la direcci�n del movimiento
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, movement * 1.5f);
    }
}