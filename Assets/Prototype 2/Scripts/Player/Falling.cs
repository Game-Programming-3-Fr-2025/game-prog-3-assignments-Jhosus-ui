using UnityEngine;

public class Falling : MonoBehaviour
{
    [Header("Configuraci�n de Ca�da")]
    public float fallTimeThreshold = 3f; // Tiempo en segundos antes de respawn

    [Header("Puntos de Respawn")]
    public Transform respawnContainer; // Objeto padre con todos los puntos de respawn

    private float fallTimer = 0f;
    private bool isFalling = false;
    private Vector3 lastGroundedPos;
    private P2Life playerLife;
    private int fallCount = 0; // Contador de ca�das

    void Start()
    {
        playerLife = GetComponent<P2Life>();
        lastGroundedPos = transform.position;
    }

    void Update()
    {
        // Detectar si est� cayendo (velocidad Y negativa)
        if (GetComponent<Rigidbody2D>().linearVelocity.y < -0.1f)
        {
            if (!isFalling)
            {
                isFalling = true;
                fallTimer = 0f;
            }

            fallTimer += Time.deltaTime;

            // Si cae por demasiado tiempo
            if (fallTimer >= fallTimeThreshold)
            {
                RespawnFromFall();
            }
        }
        else
        {
            // Si est� en el suelo, guardar posici�n
            if (isFalling)
            {
                lastGroundedPos = transform.position;
            }
            isFalling = false;
            fallTimer = 0f;
        }
    }

    void RespawnFromFall()
    {
        // Incrementar contador de ca�das
        fallCount++;

        // Cada 2 ca�das, quitar vida
        if (fallCount >= 2)
        {
            if (playerLife != null)
            {
                playerLife.TakeDamage(1);
            }
            fallCount = 0; // Reiniciar contador
        }

        // Encontrar el respawn m�s cercano
        Transform closestRespawn = FindClosestRespawn();

        // Teletransportar
        if (closestRespawn != null)
        {
            transform.position = closestRespawn.position;
        }
        else
        {
            transform.position = lastGroundedPos;
        }

        // Resetear variables
        isFalling = false;
        fallTimer = 0f;
    }

    Transform FindClosestRespawn()
    {
        if (respawnContainer == null) return null;

        Transform closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform respawn in respawnContainer)
        {
            float distance = Vector3.Distance(lastGroundedPos, respawn.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = respawn;
            }
        }

        return closest;
    }
}