using UnityEngine;

public class Bala : MonoBehaviour
{
    [Header("Configuración de Bala")]
    public float velocidad = 10f;
    public float tiempoVida = 3f;

    private Vector2 direccion;
    private float damage;
    private float radioDano;
    private bool esCortaDistancia;

    public void SetConfiguracion(Vector2 dir, float dmg, float radio, bool cortaDistancia)
    {
        direccion = dir;
        damage = dmg;
        radioDano = radio;
        esCortaDistancia = cortaDistancia;

        // Auto-destrucción después del tiempo de vida
        Destroy(gameObject, tiempoVida);
    }

    void Update()
    {
        // Movimiento de la bala
        transform.Translate(direccion * velocidad * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            AplicarDanoArea();
            Destroy(gameObject);
        }
    }

    private void AplicarDanoArea()
    {
        Collider2D[] enemigos = Physics2D.OverlapCircleAll(transform.position, radioDano);

        foreach (Collider2D enemigo in enemigos)
        {
            if (enemigo.CompareTag("Enemy"))
            {
                // Aquí aplicas el daño al enemigo
                Debug.Log($"Daño aplicado: {damage} a {enemigo.name}");

                // Ejemplo: enemigo.GetComponent<EnemyHealth>().TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDano);
    }
}