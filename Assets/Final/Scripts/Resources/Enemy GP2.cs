using UnityEngine;

public class EnemyGP2 : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private bool startFacingRight = true;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float leftLimit;
    private float rightLimit;
    private int direction;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        leftLimit = transform.position.x - patrolDistance; // Definir los límites de patrulla
        rightLimit = transform.position.x + patrolDistance;
        direction = startFacingRight ? 1 : -1;

        spriteRenderer.flipX = !startFacingRight;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y); 

        if ((direction > 0 && transform.position.x >= rightLimit) ||
            (direction < 0 && transform.position.x <= leftLimit)) // Cambiar dirección al llegar a los límites
        {
            direction = -direction;
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
    }

    void OnDrawGizmosSelected()
    {
        float left = Application.isPlaying ? leftLimit : transform.position.x - patrolDistance; 
        float right = Application.isPlaying ? rightLimit : transform.position.x + patrolDistance;
        float y = transform.position.y;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(left, y), new Vector3(right, y));
        Gizmos.DrawSphere(new Vector3(left, y), 0.1f);
        Gizmos.DrawSphere(new Vector3(right, y), 0.1f);

        if (Application.isPlaying)
        {
            Gizmos.DrawRay(transform.position, Vector3.right * direction); // Indicar dirección actual
        }
    }
}