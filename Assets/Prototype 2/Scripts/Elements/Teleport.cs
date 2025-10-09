using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform destination;
    public FC2 cameraController;
    public bool isCheckpoint = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (destination != null)
            {
                other.transform.position = destination.position;
            }

            if (isCheckpoint && cameraController != null)
            {
                cameraController.SetCheckpoint(this);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (destination != null)
        {
            Gizmos.color = isCheckpoint ? Color.green : Color.cyan;
            Gizmos.DrawWireCube(destination.position, new Vector3(1, 1, 0.1f));

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, destination.position);
        }

        if (isCheckpoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(1.2f, 1.2f, 0.1f));
        }
    }
}