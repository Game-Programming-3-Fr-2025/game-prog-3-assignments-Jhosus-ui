using UnityEngine;

public class EnemyGP : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolRadius = 5f;
    [SerializeField] private float changeDirectionTime = 2f;
    [SerializeField] private float detectionRadius = 6f;
    [SerializeField] private LayerMask playerLayer;

    private Rigidbody2D rb;
    private Vector2 patrolCenter;
    private Vector2 currentDirection;
    private float directionTimer;
    private Transform targetPlayer;
    private const float EDGE_THRESHOLD = 0.9f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolCenter = transform.position;
        SetRandomDirection();
    }

    void Update()
    {
        FindNearestPlayer();

        if (targetPlayer == null)
        {
            directionTimer -= Time.deltaTime;
            if (directionTimer <= 0)
            {
                SetRandomDirection();
                directionTimer = changeDirectionTime;
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 moveDirection = targetPlayer != null
            ? ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized
            : GetPatrolDirection();

        rb.linearVelocity = moveDirection * moveSpeed;
        FlipSprite(moveDirection.x);
    }

    Vector2 GetPatrolDirection()
    {
        float distanceFromCenter = Vector2.Distance(transform.position, patrolCenter);

        if (distanceFromCenter >= patrolRadius * EDGE_THRESHOLD)
        {
            Vector2 toCenter = (patrolCenter - (Vector2)transform.position).normalized;
            currentDirection = toCenter;
        }

        return currentDirection;
    }

    void FindNearestPlayer()
    {
        Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerLayer);

        if (players.Length == 0)
        {
            targetPlayer = null;
            return;
        }

        Transform nearest = players[0].transform;
        float minDistance = Vector2.Distance(transform.position, nearest.position);

        for (int i = 1; i < players.Length; i++)
        {
            float distance = Vector2.Distance(transform.position, players[i].transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = players[i].transform;
            }
        }

        targetPlayer = nearest;
    }

    void SetRandomDirection()
    {
        currentDirection = Random.insideUnitCircle.normalized;
    }

    void FlipSprite(float directionX)
    {
        if (directionX != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(directionX);
            transform.localScale = scale;
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? patrolCenter : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, patrolRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (Application.isPlaying && rb != null)
        {
            Gizmos.color = targetPlayer != null ? Color.red : Color.green;
            Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized * 1.5f);
        }
    }
}