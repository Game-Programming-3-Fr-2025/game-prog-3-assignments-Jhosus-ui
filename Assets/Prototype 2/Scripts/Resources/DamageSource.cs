using UnityEngine;

public class DamageSource : MonoBehaviour
{
    [Header("Configuración de Daño")]
    public int damageAmount = 1;
    public float damageCooldown = 0.5f; // Tiempo entre daños
    public bool continuousDamage = false; // Si hace daño continuo

    [Header("Visualización")]
    public Color damageAreaColor = new Color(1f, 0f, 0f, 0.3f);
    public float damageAreaRadius = 1f;

    private float lastDamageTime = 0f;

    void Start()
    {
        // Configurar layer si no está en "Dangers"
        if (gameObject.layer != LayerMask.NameToLayer("Dangers"))
        {
            // Intentar asignar layer Dangers
            int dangersLayer = LayerMask.NameToLayer("Dangers");
            if (dangersLayer != -1)
            {
                gameObject.layer = dangersLayer;
            }
            else
            {
                Debug.LogWarning("Layer 'Dangers' no encontrada. Crea la layer en Project Settings");
            }
        }
    }

    public bool CanDamage()
    {
        if (continuousDamage)
        {
            // Para daño continuo, verificar cooldown
            return Time.time >= lastDamageTime + damageCooldown;
        }
        return true;
    }

    public void OnDamageDealt()
    {
        lastDamageTime = Time.time;
    }

    // Dibujar gizmo para visualizar el área de daño
    private void OnDrawGizmos()
    {
        // Área de daño (siempre visible)
        Gizmos.color = damageAreaColor;
        Gizmos.DrawWireSphere(transform.position, damageAreaRadius);
        Gizmos.DrawSphere(transform.position, damageAreaRadius * 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        // Resaltar cuando está seleccionado
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, damageAreaRadius * 1.2f);
    }
}