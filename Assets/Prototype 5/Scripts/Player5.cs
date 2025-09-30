using UnityEngine;

public class Player5 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        // Obtener el componente Rigidbody2D
        rb = GetComponent<Rigidbody2D>();

        // Si no existe, lo a�adimos autom�ticamente
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            // Configurar para movimiento kinem�tico o por f�sica seg�n prefieras
            rb.gravityScale = 0;
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        // Input handling en Update (mejor para resposividad)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Normalizar el vector para que el movimiento diagonal no sea m�s r�pido
        movement = movement.normalized;
    }

    void FixedUpdate()
    {
        // Movimiento por f�sica en FixedUpdate (mejor para consistencia)
        rb.linearVelocity = movement * moveSpeed;
    }
}