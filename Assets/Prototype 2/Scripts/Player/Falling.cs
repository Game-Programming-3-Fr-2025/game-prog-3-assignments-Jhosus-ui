using UnityEngine;
using System.Collections.Generic;

public class Falling : MonoBehaviour
{
    [Header("Configuración de Límites")]
    public float margin = 0.5f;

    [Header("Detección de Respawn Points")]
    public string respawnTag = "RespawnPoint";

    [Header("Cámaras")]
    public Camera playerCamera;

    [Header("Dirección de Nivel")]
    public LevelDirection levelDirection = LevelDirection.VerticalUp;

    private LifeSystem playerLife;
    private int outOfBoundsCount = 0;
    private Vector3 lastSafePosition;
    private bool isPlayer1;
    private List<Transform> respawnPoints = new List<Transform>();

    public enum LevelDirection
    {
        VerticalUp,     // Cámara sube (niveles normales)
        VerticalDown,   // Cámara baja (niveles rotados 180°)
        HorizontalRight,// Cámara va a la derecha
        HorizontalLeft  // Cámara va a la izquierda
    }

    void Start()
    {
        playerLife = GetComponent<LifeSystem>();
        lastSafePosition = transform.position;

        isPlayer1 = gameObject.CompareTag("Player 1");
        FindPlayerCamera();
        FindAllRespawnPoints();

        Debug.Log($"{GetPlayerName()} - Dirección: {levelDirection}, Respawns: {respawnPoints.Count}");
    }

    void FindPlayerCamera()
    {
        if (playerCamera == null)
        {
            string cameraName = isPlayer1 ? "MainCameraP1" : "MainCameraP2";
            GameObject camObj = GameObject.Find(cameraName);
            if (camObj != null) playerCamera = camObj.GetComponent<Camera>();

            if (playerCamera == null) playerCamera = Camera.main;
        }
    }

    void FindAllRespawnPoints()
    {
        GameObject[] respawnObjects = GameObject.FindGameObjectsWithTag(respawnTag);
        respawnPoints.Clear();

        foreach (GameObject obj in respawnObjects)
        {
            respawnPoints.Add(obj.transform);
        }

        // Ordenar según dirección del nivel
        SortRespawnsByDirection();
    }

    void SortRespawnsByDirection()
    {
        switch (levelDirection)
        {
            case LevelDirection.VerticalUp:
                // Ordenar por Y ascendente (de abajo hacia arriba)
                respawnPoints.Sort((a, b) => a.position.y.CompareTo(b.position.y));
                break;

            case LevelDirection.VerticalDown:
                // Ordenar por Y descendente (de arriba hacia abajo)
                respawnPoints.Sort((a, b) => b.position.y.CompareTo(a.position.y));
                break;

            case LevelDirection.HorizontalRight:
                // Ordenar por X ascendente (de izquierda a derecha)
                respawnPoints.Sort((a, b) => a.position.x.CompareTo(b.position.x));
                break;

            case LevelDirection.HorizontalLeft:
                // Ordenar por X descendente (de derecha a izquierda)
                respawnPoints.Sort((a, b) => b.position.x.CompareTo(a.position.x));
                break;
        }
    }

    void Update()
    {
        if (playerCamera == null) return;

        // Detectar si está fuera de límites basado en dirección
        bool isOutOfBounds = CheckOutOfBoundsByDirection();

        if (isOutOfBounds)
        {
            HandleOutOfBounds();
        }
        else
        {
            lastSafePosition = transform.position;

            // Actualizar respawn points ocasionalmente
            if (Time.frameCount % 60 == 0)
            {
                FindAllRespawnPoints();
            }
        }
    }

    bool CheckOutOfBoundsByDirection()
    {
        Vector3 screenPos = playerCamera.WorldToViewportPoint(transform.position);

        switch (levelDirection)
        {
            case LevelDirection.VerticalUp:
                // Fuera si está muy abajo o a los lados
                return screenPos.y < -margin ||
                       screenPos.x < -margin || screenPos.x > 1 + margin;

            case LevelDirection.VerticalDown:
                // Fuera si está muy arriba o a los lados
                return screenPos.y > 1 + margin ||
                       screenPos.x < -margin || screenPos.x > 1 + margin;

            case LevelDirection.HorizontalRight:
                // Fuera si está muy a la izquierda o arriba/abajo
                return screenPos.x < -margin ||
                       screenPos.y < -margin || screenPos.y > 1 + margin;

            case LevelDirection.HorizontalLeft:
                // Fuera si está muy a la derecha o arriba/abajo
                return screenPos.x > 1 + margin ||
                       screenPos.y < -margin || screenPos.y > 1 + margin;

            default:
                // Check general como fallback
                return screenPos.x < -margin || screenPos.x > 1 + margin ||
                       screenPos.y < -margin || screenPos.y > 1 + margin;
        }
    }

    void HandleOutOfBounds()
    {
        Debug.Log($"{GetPlayerName()} salió de límites en dirección {levelDirection}");

        outOfBoundsCount++;

        if (outOfBoundsCount >= 2 && playerLife != null)
        {
            playerLife.TakeDamage(1);
            outOfBoundsCount = 0;
        }

        // Encontrar respawn adecuado según dirección
        Transform bestRespawn = FindAppropriateRespawn();

        if (bestRespawn != null)
        {
            transform.position = bestRespawn.position;
            Debug.Log($"{GetPlayerName()} respawneado en: {bestRespawn.name}");
        }
        else
        {
            transform.position = lastSafePosition;
        }

        // Resetear física
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    Transform FindAppropriateRespawn()
    {
        if (respawnPoints.Count == 0) return null;

        Transform bestRespawn = null;
        float bestScore = Mathf.Infinity;

        foreach (Transform respawn in respawnPoints)
        {
            // Solo considerar respawns que estén "atrás" según la dirección
            if (IsRespawnBehind(respawn))
            {
                float score = CalculateRespawnScore(respawn);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestRespawn = respawn;
                }
            }
        }

        // Si no hay respawns atrás, usar el más cercano en general
        if (bestRespawn == null)
        {
            Debug.Log("No hay respawns atrás, usando el más cercano");
            bestRespawn = FindClosestRespawnInGeneral();
        }

        return bestRespawn;
    }

    bool IsRespawnBehind(Transform respawn)
    {
        switch (levelDirection)
        {
            case LevelDirection.VerticalUp:
                // Respawn atrás = está ABAJO del jugador
                return respawn.position.y <= lastSafePosition.y;

            case LevelDirection.VerticalDown:
                // Respawn atrás = está ARRIBA del jugador
                return respawn.position.y >= lastSafePosition.y;

            case LevelDirection.HorizontalRight:
                // Respawn atrás = está a la IZQUIERDA del jugador
                return respawn.position.x <= lastSafePosition.x;

            case LevelDirection.HorizontalLeft:
                // Respawn atrás = está a la DERECHA del jugador
                return respawn.position.x >= lastSafePosition.x;

            default:
                return true;
        }
    }

    float CalculateRespawnScore(Transform respawn)
    {
        float baseDistance = Vector3.Distance(lastSafePosition, respawn.position);

        // Ajustar score según dirección
        switch (levelDirection)
        {
            case LevelDirection.VerticalUp:
                // Priorizar respawns que no estén muy abajo
                float verticalBonus = Mathf.Abs(lastSafePosition.y - respawn.position.y) * 0.7f;
                float horizontalBonus = Mathf.Abs(lastSafePosition.x - respawn.position.x) * 0.3f;
                return verticalBonus + horizontalBonus;

            case LevelDirection.VerticalDown:
                // Priorizar respawns que no estén muy arriba
                verticalBonus = Mathf.Abs(lastSafePosition.y - respawn.position.y) * 0.7f;
                horizontalBonus = Mathf.Abs(lastSafePosition.x - respawn.position.x) * 0.3f;
                return verticalBonus + horizontalBonus;

            case LevelDirection.HorizontalRight:
                // Priorizar respawns que no estén muy a la izquierda
                float horizontalPenalty = Mathf.Abs(lastSafePosition.x - respawn.position.x) * 0.7f;
                float verticalPenalty = Mathf.Abs(lastSafePosition.y - respawn.position.y) * 0.3f;
                return horizontalPenalty + verticalPenalty;

            case LevelDirection.HorizontalLeft:
                // Priorizar respawns que no estén muy a la derecha
                horizontalPenalty = Mathf.Abs(lastSafePosition.x - respawn.position.x) * 0.7f;
                verticalPenalty = Mathf.Abs(lastSafePosition.y - respawn.position.y) * 0.3f;
                return horizontalPenalty + verticalPenalty;

            default:
                return baseDistance;
        }
    }

    Transform FindClosestRespawnInGeneral()
    {
        Transform closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform respawn in respawnPoints)
        {
            float distance = Vector3.Distance(lastSafePosition, respawn.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = respawn;
            }
        }

        return closest;
    }

    // Método para cambiar dirección dinámicamente (desde triggers)
    public void SetLevelDirection(LevelDirection newDirection)
    {
        levelDirection = newDirection;
        SortRespawnsByDirection();
        Debug.Log($"{GetPlayerName()} cambió dirección a: {levelDirection}");
    }

    string GetPlayerName()
    {
        return isPlayer1 ? "Player 1" : "Player 2";
    }

    // Gizmos para debug
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Dibujar límites según dirección
        if (playerCamera != null)
        {
            Gizmos.color = GetDirectionColor();

            Vector3[] bounds = GetDirectionBounds();
            if (bounds.Length >= 2)
            {
                Gizmos.DrawLine(bounds[0], bounds[1]);
                Gizmos.DrawLine(bounds[1], bounds[2]);
                Gizmos.DrawLine(bounds[2], bounds[3]);
                Gizmos.DrawLine(bounds[3], bounds[0]);
            }
        }

        // Dibujar respawns
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        foreach (Transform respawn in respawnPoints)
        {
            Gizmos.DrawWireSphere(respawn.position, 0.3f);

            // Mostrar si está "atrás" o "adelante"
            Gizmos.color = IsRespawnBehind(respawn) ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, respawn.position);
            Gizmos.color = new Color(0, 1, 0, 0.3f);
        }

        // Mostrar dirección actual
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2,
            $"{GetPlayerName()}\nDir: {levelDirection}", style);
#endif
    }

    Color GetDirectionColor()
    {
        switch (levelDirection)
        {
            case LevelDirection.VerticalUp: return Color.green;
            case LevelDirection.VerticalDown: return Color.blue;
            case LevelDirection.HorizontalRight: return Color.yellow;
            case LevelDirection.HorizontalLeft: return Color.magenta;
            default: return Color.white;
        }
    }

    Vector3[] GetDirectionBounds()
    {
        if (playerCamera == null) return new Vector3[0];

        Vector3[] bounds = new Vector3[4];

        switch (levelDirection)
        {
            case LevelDirection.VerticalUp:
                bounds[0] = playerCamera.ViewportToWorldPoint(new Vector3(-margin, -margin, playerCamera.nearClipPlane));
                bounds[1] = playerCamera.ViewportToWorldPoint(new Vector3(1 + margin, -margin, playerCamera.nearClipPlane));
                bounds[2] = playerCamera.ViewportToWorldPoint(new Vector3(1 + margin, 1, playerCamera.nearClipPlane));
                bounds[3] = playerCamera.ViewportToWorldPoint(new Vector3(-margin, 1, playerCamera.nearClipPlane));
                break;

            case LevelDirection.VerticalDown:
                bounds[0] = playerCamera.ViewportToWorldPoint(new Vector3(-margin, 0, playerCamera.nearClipPlane));
                bounds[1] = playerCamera.ViewportToWorldPoint(new Vector3(1 + margin, 0, playerCamera.nearClipPlane));
                bounds[2] = playerCamera.ViewportToWorldPoint(new Vector3(1 + margin, 1 + margin, playerCamera.nearClipPlane));
                bounds[3] = playerCamera.ViewportToWorldPoint(new Vector3(-margin, 1 + margin, playerCamera.nearClipPlane));
                break;

            case LevelDirection.HorizontalRight:
                bounds[0] = playerCamera.ViewportToWorldPoint(new Vector3(-margin, -margin, playerCamera.nearClipPlane));
                bounds[1] = playerCamera.ViewportToWorldPoint(new Vector3(1, -margin, playerCamera.nearClipPlane));
                bounds[2] = playerCamera.ViewportToWorldPoint(new Vector3(1, 1 + margin, playerCamera.nearClipPlane));
                bounds[3] = playerCamera.ViewportToWorldPoint(new Vector3(-margin, 1 + margin, playerCamera.nearClipPlane));
                break;

            case LevelDirection.HorizontalLeft:
                bounds[0] = playerCamera.ViewportToWorldPoint(new Vector3(0, -margin, playerCamera.nearClipPlane));
                bounds[1] = playerCamera.ViewportToWorldPoint(new Vector3(1 + margin, -margin, playerCamera.nearClipPlane));
                bounds[2] = playerCamera.ViewportToWorldPoint(new Vector3(1 + margin, 1 + margin, playerCamera.nearClipPlane));
                bounds[3] = playerCamera.ViewportToWorldPoint(new Vector3(0, 1 + margin, playerCamera.nearClipPlane));
                break;
        }

        return bounds;
    }
}