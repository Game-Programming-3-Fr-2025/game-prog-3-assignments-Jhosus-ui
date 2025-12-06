using UnityEngine;

public class DamageSource : MonoBehaviour
{
    [Header("Configuración de Daño")]
    public int damageAmount = 1;
    public float damageCooldown = 0.5f; // Tiempo entre daños
    public bool continuousDamage = false; // Si hace daño continuo

    [Header("Área de Daño Rectangular")]
    public Color damageAreaColor = new Color(1f, 0f, 0f, 0.3f);
    public Vector2 damageAreaSize = new Vector2(1f, 1f); // Ancho (X) y Alto (Y)

    private float lastDamageTime = 0f;

    void Start()
    {
        // Configurar layer si no está en "Dangers"
        if (gameObject.layer != LayerMask.NameToLayer("Dangers"))
        {
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

    // Método para verificar si un punto está dentro del área de daño
    public bool IsPointInDamageArea(Vector2 point)
    {
        Vector2 halfSize = damageAreaSize * 0.5f;
        Rect damageRect = new Rect(
            transform.position.x - halfSize.x,
            transform.position.y - halfSize.y,
            damageAreaSize.x,
            damageAreaSize.y
        );

        return damageRect.Contains(point);
    }

    // Dibujar gizmo para visualizar el área de daño rectangular
    private void OnDrawGizmos()
    {
        // Área de daño (siempre visible)
        Gizmos.color = damageAreaColor;

        // Dibujar rectángulo
        Vector3 size = new Vector3(damageAreaSize.x, damageAreaSize.y, 0.1f);
        Gizmos.DrawWireCube(transform.position, size);

        // Pequeño cubo central para referencia
        Gizmos.DrawCube(transform.position, Vector3.one * 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        // Resaltar cuando está seleccionado
        Gizmos.color = Color.yellow;

        Vector3 size = new Vector3(damageAreaSize.x * 1.2f, damageAreaSize.y * 1.2f, 0.1f);
        Gizmos.DrawWireCube(transform.position, size);
    }
}