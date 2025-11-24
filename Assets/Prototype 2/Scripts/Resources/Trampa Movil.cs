using UnityEngine;

public class TrampaMovil : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector2 movementDirection = new Vector2(1, 0);
    public float movementDistance = 3f;
    public float movementSpeed = 2f;
    public bool resetOnReturn = false;

    [Header("Detection Settings")]
    public Vector2 detectionSize = new Vector2(2f, 1f);
    public Vector2 detectionOffset = Vector2.zero;
    public bool showDetectionGizmo = true;
    public Color detectionGizmoColor = Color.red;
    public float detectionCheckRate = 0.1f;

    [Header("Movement Gizmo Settings")]
    public bool showMovementGizmo = true;
    public Color movementGizmoColor = Color.yellow;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool movingToTarget = true;
    private bool hasBeenActivated = false;
    private bool isMoving = false;
    private float lastDetectionTime;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + (Vector3)movementDirection.normalized * movementDistance;
    }

    void Update()
    {
        // Verificar detección del jugador
        if (!hasBeenActivated && Time.time - lastDetectionTime >= detectionCheckRate)
        {
            CheckForPlayer();
            lastDetectionTime = Time.time;
        }

        // Movimiento
        if (!hasBeenActivated) return;

        if (isMoving)
        {
            Vector3 target = movingToTarget ? targetPos : startPos;
            transform.position = Vector3.MoveTowards(transform.position, target, movementSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.01f)
            {
                if (resetOnReturn && !movingToTarget)
                {
                    isMoving = false;
                    hasBeenActivated = false;
                }
                movingToTarget = !movingToTarget;
            }
        }
    }

    void CheckForPlayer()
    {
        // Calcular posición del área de detección
        Vector2 detectionCenter = (Vector2)transform.position + detectionOffset;

        // Verificar si hay jugadores en el área
        Collider2D[] colliders = Physics2D.OverlapBoxAll(detectionCenter, detectionSize, 0f);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player 1") || collider.CompareTag("Player 2"))
            {
                hasBeenActivated = true;
                isMoving = true;
                break;
            }
        }
    }

    void OnDrawGizmos()
    {
        Vector3 currentPos = Application.isPlaying ? startPos : transform.position;
        Vector3 endPos = Application.isPlaying ? targetPos : transform.position + (Vector3)movementDirection.normalized * movementDistance;

        // Gizmo del área de detección
        if (showDetectionGizmo)
        {
            Gizmos.color = detectionGizmoColor;
            Vector3 detectionPosition = currentPos + (Vector3)detectionOffset;
            Gizmos.DrawWireCube(detectionPosition, new Vector3(detectionSize.x, detectionSize.y, 0));

            // Relleno semitransparente
            Gizmos.color = new Color(detectionGizmoColor.r, detectionGizmoColor.g, detectionGizmoColor.b, 0.3f);
            Gizmos.DrawCube(detectionPosition, new Vector3(detectionSize.x, detectionSize.y, 0));
        }

        // Gizmo del movimiento
        if (showMovementGizmo)
        {
            Gizmos.color = movementGizmoColor;
            Gizmos.DrawLine(currentPos, endPos);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(currentPos, Vector3.one * 0.3f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(endPos, Vector3.one * 0.3f);
        }
    }

    void OnValidate()
    {
        if (movementDirection == Vector2.zero)
        {
            movementDirection = Vector2.right;
        }

        detectionSize.x = Mathf.Max(0.1f, detectionSize.x);
        detectionSize.y = Mathf.Max(0.1f, detectionSize.y);
        detectionCheckRate = Mathf.Max(0.01f, detectionCheckRate);
    }
}