using UnityEngine;

public class ExpZone : MonoBehaviour
{
    [Header("EXP Zone Settings")]
    public string playerTag = "Player";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            ManagerUp manager = FindObjectOfType<ManagerUp>();
            manager?.EnterExpZone();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            ManagerUp manager = FindObjectOfType<ManagerUp>();
            manager?.ExitExpZone();
        }
    }

    void OnDrawGizmos()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position, collider.bounds.size);
        }
    }
}