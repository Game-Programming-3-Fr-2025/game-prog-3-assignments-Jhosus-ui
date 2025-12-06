using UnityEngine;

public class ObjectD : MonoBehaviour
{
    [Header("Configuración de Daño")]
    public int damage = 1;

    [Header("Opciones de Respawn")]
    public bool enableRespawn = true;

    [Header("Puntos de Respawn")]
    public Transform respawnContainer;

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyDamage(other.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ApplyDamage(collision.gameObject);
        }
    }

    void ApplyDamage(GameObject player)
    {
        LifeSystem playerLife = player.GetComponent<LifeSystem>();
        if (playerLife != null && !playerLife.IsInvincible)
        {
            // Solo notificar el daño, P2Life maneja el knockback
            playerLife.TakeDamage(damage, transform.position);
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