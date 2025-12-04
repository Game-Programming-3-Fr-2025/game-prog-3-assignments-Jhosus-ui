using UnityEngine;

public class EnemyGP2 : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private bool startFacingRight = true;
    [SerializeField] private Color gizmoColor = Color.cyan;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 startPosition;
    private bool movingRight;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        startPosition = transform.position;
        movingRight = startFacingRight;
        FlipSprite();
    }

    void FixedUpdate()
    {
        // Mover
        float direction = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        
        // Girar al límite
        float distance = Mathf.Abs(transform.position.x - startPosition.x);
        if (distance >= patrolDistance)
        {
            movingRight = !movingRight;
            FlipSprite();
            startPosition = transform.position;
        }
    }

    void FlipSprite()
    {
        spriteRenderer.flipX = !movingRight;
    }

    void OnDrawGizmosSelected()
    {
        // Mostrar área de patrulla
        Gizmos.color = gizmoColor;
        Vector3 pos = Application.isPlaying ? (Vector3)startPosition : transform.position;
        
        Vector3 leftLimit = pos + Vector3.left * patrolDistance;
        Vector3 rightLimit = pos + Vector3.right * patrolDistance;
        
        Gizmos.DrawLine(leftLimit, rightLimit);
        Gizmos.DrawSphere(leftLimit, 0.1f);
        Gizmos.DrawSphere(rightLimit, 0.1f);
        
        // Mostrar dirección
        if (Application.isPlaying)
        {
            Vector3 dir = movingRight ? Vector3.right : Vector3.left;
            Gizmos.DrawRay(transform.position, dir * 1f);
        }
    }
}