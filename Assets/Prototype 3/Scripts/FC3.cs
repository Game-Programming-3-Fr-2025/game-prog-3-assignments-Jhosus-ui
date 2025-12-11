using UnityEngine;

public class FC3 : MonoBehaviour
{
    [Header("Configuración Básica")]
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10f);

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    public void SetTarget(Transform newTarget) => target = newTarget;
    public void SetOffset(Vector3 newOffset) => offset = newOffset;
}