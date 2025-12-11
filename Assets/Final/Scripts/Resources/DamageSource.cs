using UnityEngine;

public class DamageSource : MonoBehaviour
{
    [Header("Configuración de Daño")]
    public int damageAmount = 1;
    public float damageCooldown = 0.5f; 
    public bool continuousDamage = false; 

    [Header("Área de Daño Rectangular")]
    public Color damageAreaColor = new Color(1f, 0f, 0f, 0.3f);
    public Vector2 damageAreaSize = new Vector2(1f, 1f); // Ancho (X) y Alto (Y)

    private float lastDamageTime = 0f;

    void Start()
    {
        
        if (gameObject.layer != LayerMask.NameToLayer("Dangers")) // Configurar layer manual aunque podria servir a futuro a otros casos
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
           
            return Time.time >= lastDamageTime + damageCooldown;  // Para daño continuo, verificar cooldown aunque opciona en algunas areas
        }
        return true;
    }

    public void OnDamageDealt()
    {
        lastDamageTime = Time.time;
    }

    
    public bool IsPointInDamageArea(Vector2 point) // Método para verificar si un punto está dentro del área de daño
    {
        Vector2 halfSize = damageAreaSize * 0.5f;
        Rect damageRect = new Rect(
            transform.position.x - halfSize.x,
            transform.position.y - halfSize.y,
            damageAreaSize.x,
            damageAreaSize.y
        );

        return damageRect.Contains(point); // Verificar si el punto está dentro del rectángulo
    }

    
    private void OnDrawGizmos()
    {
        Gizmos.color = damageAreaColor;
        Vector3 size = new Vector3(damageAreaSize.x, damageAreaSize.y, 0.1f);
        Gizmos.DrawWireCube(transform.position, size);
        Gizmos.DrawCube(transform.position, Vector3.one * 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 size = new Vector3(damageAreaSize.x * 1.2f, damageAreaSize.y * 1.2f, 0.1f);
        Gizmos.DrawWireCube(transform.position, size);
    }
}