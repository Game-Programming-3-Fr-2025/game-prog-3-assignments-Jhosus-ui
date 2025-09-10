using UnityEngine;

public class FC2 : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Camera Settings")]
    public float smoothSpeed = 0.125f;

    [Header("Fall Settings")]
    public float fallDistance = 10f;
    public float lineLength = 10f;

    [Header("Screen Shake")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.3f;

    private float currentHighestY;
    private float shakeTimer;
    private float currentShakeMagnitude;
    private Camera cam;
    private Vector3 initialPlayerPosition;

    void Start()
    {
        cam = Camera.main;
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        initialPlayerPosition = player.position;
        currentHighestY = player.position.y;
    }

    void LateUpdate()
    {
        if (player == null) return;

        UpdateHighestY();
        FollowPlayer();
        HandleScreenShake();
        CheckFallDeath();
    }

    void UpdateHighestY()
    {
        if (player.position.y > currentHighestY)
        {
            currentHighestY = player.position.y;
        }
    }

    void FollowPlayer()
    {
        if (cam == null) return;

        Vector3 targetPosition = player.position;
        targetPosition.z = -10f;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);

        if (shakeTimer > 0)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * currentShakeMagnitude;
            shakeOffset.z = 0f;
            smoothedPosition += shakeOffset;
        }

        smoothedPosition.z = -10f;
        transform.position = smoothedPosition;
    }

    void HandleScreenShake()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0)
            {
                currentShakeMagnitude = 0f;
            }
        }
    }

    void CheckFallDeath()
    {
        if (player.position.y < currentHighestY - fallDistance)
        {
            ResetLevel();
        }
    }

    void ResetLevel()
    {
        player.position = initialPlayerPosition;
        currentHighestY = initialPlayerPosition.y;
        transform.position = new Vector3(initialPlayerPosition.x, initialPlayerPosition.y, -10f);
    }

    public void CameraShake()
    {
        shakeTimer = shakeDuration;
        currentShakeMagnitude = shakeMagnitude;
    }

    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            // Dibujar línea de muerte por caída
            float deathLineY = Application.isPlaying ? currentHighestY - fallDistance : player.position.y - fallDistance;

            Gizmos.color = Color.red;
            Vector3 lineStart = new Vector3(player.position.x - lineLength / 2, deathLineY, 0);
            Vector3 lineEnd = new Vector3(player.position.x + lineLength / 2, deathLineY, 0);
            Gizmos.DrawLine(lineStart, lineEnd);

            // Dibujar pequeñas líneas verticales en los extremos
            Gizmos.DrawLine(lineStart, lineStart + Vector3.up * 0.5f);
            Gizmos.DrawLine(lineEnd, lineEnd + Vector3.up * 0.5f);
        }
    }
    public void ResetFallLine()
    {
        currentHighestY = player.position.y;
    }
}