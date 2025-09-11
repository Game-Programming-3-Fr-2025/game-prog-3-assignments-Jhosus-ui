using UnityEngine;

public class FC2 : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform player;
    public float smoothSpeed = 0.125f;
    public float fallDistance = 10f;
    public float lineLength = 10f;
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.3f;

    [Header("System Checkpoints")]
    private float currentHighestY;
    private float shakeTimer;
    private float currentShakeMagnitude;
    private Vector3 initialPlayerPosition;
    private Teleport currentCheckpoint;

    void Start()
    {
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
        Vector3 targetPosition = player.position;
        targetPosition.z = -10f;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);

        if (shakeTimer > 0)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * currentShakeMagnitude;
            shakeOffset.z = 0f;
            smoothedPosition += shakeOffset;
        }

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
            ResetToCheckpoint();
        }
    }

    void ResetToCheckpoint()
    {
        if (currentCheckpoint != null)
        {
            player.position = currentCheckpoint.transform.position;
            currentHighestY = currentCheckpoint.transform.position.y;
            transform.position = new Vector3(currentCheckpoint.transform.position.x,
                                            currentCheckpoint.transform.position.y, -10f);
        }
        else
        {
            player.position = initialPlayerPosition;
            currentHighestY = initialPlayerPosition.y;
            transform.position = new Vector3(initialPlayerPosition.x, initialPlayerPosition.y, -10f);
        }
    }

    public void CameraShake()
    {
        shakeTimer = shakeDuration;
        currentShakeMagnitude = shakeMagnitude;
    }

    public void SetCheckpoint(Teleport checkpoint)
    {
        currentCheckpoint = checkpoint;
    }


    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            float deathLineY = Application.isPlaying ? currentHighestY - fallDistance : player.position.y - fallDistance;

            Gizmos.color = Color.red;
            Vector3 lineStart = new Vector3(player.position.x - lineLength / 2, deathLineY, 0);
            Vector3 lineEnd = new Vector3(player.position.x + lineLength / 2, deathLineY, 0);
            Gizmos.DrawLine(lineStart, lineEnd);

            Gizmos.DrawLine(lineStart, lineStart + Vector3.up * 0.5f);
            Gizmos.DrawLine(lineEnd, lineEnd + Vector3.up * 0.5f);
        }
    }

    public void ResetFallLine()
    {
        currentHighestY = player.position.y;
    }
}