using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector2 movementDirection = new Vector2(1, 0);
    public float movementDistance = 3f;
    public float movementSpeed = 2f;

    [Header("Gizmo Settings")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.cyan;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool movingToTarget = true;
    private bool hasBeenActivated = false;
    private bool hasPlayer = false;

    void Start()
    {
        startPos = transform.position;
        // Normalizamos la dirección para que tenga magnitud 1 y luego multiplicamos por la distancia
        targetPos = startPos + (Vector3)movementDirection.normalized * movementDistance;
    }

    void Update()
    {
        // Solo se mueve si ha sido activada por un jugador
        if (!hasBeenActivated) return;

        Vector3 target = movingToTarget ? targetPos : startPos;
        transform.position = Vector3.MoveTowards(transform.position, target, movementSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
            movingToTarget = !movingToTarget;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player 1") || collision.gameObject.CompareTag("Player 2"))
        {
            hasPlayer = true;
            collision.transform.SetParent(transform);

            // Activar el movimiento solo la primera vez que un jugador pisa la plataforma
            if (!hasBeenActivated)
            {
                hasBeenActivated = true;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player 1") || collision.gameObject.CompareTag("Player 2"))
        {
            hasPlayer = false;
            collision.transform.SetParent(null);
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Vector3 start = Application.isPlaying ? startPos : transform.position;
        Vector3 end = Application.isPlaying ? targetPos : transform.position + (Vector3)movementDirection.normalized * movementDistance;

        Gizmos.color = gizmoColor;
        Gizmos.DrawLine(start, end);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(start, Vector3.one * 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(end, Vector3.one * 0.3f);

        Vector3 dir = (end - start).normalized * 0.5f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(start, dir);
        Vector3 arrowTip = start + dir;
        Gizmos.DrawRay(arrowTip, Quaternion.Euler(0, 0, 135) * dir * 0.6f);
        Gizmos.DrawRay(arrowTip, Quaternion.Euler(0, 0, -135) * dir * 0.6f);
    }

    // Método para validar y normalizar la dirección en el Inspector
    void OnValidate()
    {
        // Asegurarnos de que la dirección no sea cero
        if (movementDirection == Vector2.zero)
        {
            movementDirection = Vector2.right;
        }
    }
}