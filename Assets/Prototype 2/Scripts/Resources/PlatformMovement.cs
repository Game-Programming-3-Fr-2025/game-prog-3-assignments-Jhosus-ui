using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    [Header("Movement")]
    public bool moveX, moveY;
    public bool positiveDirection = true;
    public float distance = 3f;
    public float speed = 2f;

    [Header("Activation")]
    public bool requirePlayer = true;

    private Vector3 startPos;
    private Vector3 direction;
    private float pingPong;
    private bool hasPlayer;

    void Start()
    {
        startPos = transform.position;
        int dir = positiveDirection ? 1 : -1;
        direction = new Vector3(moveX ? dir : 0, moveY ? dir : 0, 0);
    }

    void Update()
    {
        if (requirePlayer && !hasPlayer) return;

        pingPong = Mathf.PingPong(Time.time * speed, distance);
        transform.position = startPos + direction * pingPong;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            hasPlayer = true;
            other.transform.SetParent(transform); // El jugador se mueve con la plataforma
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            hasPlayer = false;
            other.transform.SetParent(null); // El jugador deja de moverse con la plataforma
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = hasPlayer ? Color.green : Color.cyan;
        Vector3 currentStart = Application.isPlaying ? startPos : transform.position;
        int dir = positiveDirection ? 1 : -1;
        Vector3 targetPos = currentStart + new Vector3(
            moveX ? distance * dir : 0,
            moveY ? distance * dir : 0, 0);

        Gizmos.DrawLine(currentStart, targetPos);
        Gizmos.DrawWireCube(currentStart, Vector3.one * 0.3f);
        Gizmos.DrawWireCube(targetPos, Vector3.one * 0.3f);

        DrawArrow(currentStart, (targetPos - currentStart).normalized, 0.5f);
    }

    void DrawArrow(Vector3 pos, Vector3 dir, float size)
    {
        Gizmos.DrawRay(pos, dir * size);
        Gizmos.DrawRay(pos + dir * size, Quaternion.Euler(0, 0, 135) * dir * 0.3f);
        Gizmos.DrawRay(pos + dir * size, Quaternion.Euler(0, 0, -135) * dir * 0.3f);
    }
}