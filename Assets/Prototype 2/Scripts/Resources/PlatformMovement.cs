using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    public enum Direction { Right, Left, Up, Down, UpRight, UpLeft, DownRight, DownLeft }

    [Header("Movement Settings")]
    public Direction movementDirection = Direction.Right;
    public float movementDistance = 3f;
    public float movementSpeed = 2f;
    public bool moveOnlyWithPlayer = true;

    [Header("Gizmo Settings")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.cyan;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool movingToTarget = true;
    private bool hasPlayer = false;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + GetDirectionVector() * movementDistance;
    }

    void Update()
    {
        if (moveOnlyWithPlayer && !hasPlayer) return;

        Vector3 target = movingToTarget ? targetPos : startPos;
        transform.position = Vector3.MoveTowards(transform.position, target, movementSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
            movingToTarget = !movingToTarget;
    }

    Vector3 GetDirectionVector()
    {
        switch (movementDirection)
        {
            case Direction.Right: return Vector3.right;
            case Direction.Left: return Vector3.left;
            case Direction.Up: return Vector3.up;
            case Direction.Down: return Vector3.down;
            case Direction.UpRight: return new Vector3(1, 1, 0).normalized;
            case Direction.UpLeft: return new Vector3(-1, 1, 0).normalized;
            case Direction.DownRight: return new Vector3(1, -1, 0).normalized;
            case Direction.DownLeft: return new Vector3(-1, -1, 0).normalized;
            default: return Vector3.right;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player 1") || collision.gameObject.CompareTag("Player 2"))
        {
            hasPlayer = true;
            collision.transform.SetParent(transform);
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
        Vector3 end = Application.isPlaying ? targetPos : transform.position + GetDirectionVector() * movementDistance;

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
}