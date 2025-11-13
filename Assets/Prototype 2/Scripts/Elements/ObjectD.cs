using UnityEngine;

public class ObjectD : MonoBehaviour
{
    [Header("Configuración de Daño")]
    public int damage = 1;

    [Header("Configuración de Knockback")]
    public float knockbackForce = 10f;
    public Vector2 knockbackDirection = new Vector2(1, 1);

    [Header("Opciones de Respawn")]
    public bool enableRespawn = true;

    [Header("Puntos de Respawn")]
    public Transform respawnContainer;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyDamageAndKnockback(other.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ApplyDamageAndKnockback(collision.gameObject);
        }
    }

    void ApplyDamageAndKnockback(GameObject player)
    {
        P2Life playerLife = player.GetComponent<P2Life>();
        if (playerLife != null && !playerLife.IsInvincible)
        {
            // Calcular dirección automática simple
            Vector2 knockbackDir = Vector2.zero;

            // Determinar dirección basada en qué lado del objeto está el jugador
            if (player.transform.position.x > transform.position.x)
            {
                // Jugador está a la derecha -> empujar hacia derecha
                knockbackDir = new Vector2(1f, 1f);
            }
            else
            {
                // Jugador está a la izquierda -> empujar hacia izquierda
                knockbackDir = new Vector2(-1f, 1f);
            }

            playerLife.TakeDamage(damage, knockbackDir.normalized, knockbackForce);
        }

        if (enableRespawn)
        {
            RespawnPlayer(player);
        }
    }

    void RespawnPlayer(GameObject player)
    {
        Transform closestRespawn = FindClosestRespawn(player.transform.position);

        if (closestRespawn != null)
        {
            player.transform.position = closestRespawn.position;
        }
    }

    Transform FindClosestRespawn(Vector3 playerPosition)
    {
        if (respawnContainer == null || respawnContainer.childCount == 0)
            return null;

        Transform closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform respawn in respawnContainer)
        {
            float distance = Vector3.Distance(playerPosition, respawn.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = respawn;
            }
        }

        return closest;
    }
}