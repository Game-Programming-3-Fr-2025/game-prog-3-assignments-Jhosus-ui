using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null) //obligar a existir un Rigidbody2D
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // Sin gravedad
            rb.freezeRotation = true; // Congelar rotación
        }
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleInput()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");
        movement = new Vector2(moveHorizontal, moveVertical).normalized;
    }

    void HandleMovement()
    {
        if (movement != Vector2.zero)
        {
            Vector2 moveVelocity = movement * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + moveVelocity);
        }
    }

    // para mis animaciones 
    public Vector2 GetMovementDirection()
    {
        return movement;
    }

    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, movement * 1.5f);
    }
}