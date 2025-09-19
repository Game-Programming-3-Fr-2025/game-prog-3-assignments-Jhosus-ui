using UnityEngine;

public class CHanges : MonoBehaviour
{
    public GameObject destinationPoint;
    public float activationDistance = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && destinationPoint != null)
            other.transform.position = destinationPoint.transform.position;
    }

    void OnDrawGizmosSelected()
    {
        if (destinationPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, destinationPoint.transform.position);
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            Gizmos.DrawWireSphere(destinationPoint.transform.position, 0.3f);
        }
    }
}