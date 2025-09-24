using UnityEngine;

public class Bala : MonoBehaviour
{
    [Header("Configuración de Bala")]
    public float velocidad = 10f;
    public float tiempoVida = 3f;

    private Vector2 direccion;
    private int damage; // CAMBIAR a int
    private float radioDano;
    private bool esCortaDistancia;

    void Start()
    {
        gameObject.tag = "Bullet";
        Destroy(gameObject, tiempoVida);
    }

    public void SetConfiguracion(Vector2 dir, float dmg, float radio, bool cortaDistancia)
    {
        direccion = dir;
        damage = (int)dmg; // Convertir a int
        radioDano = radio;
        esCortaDistancia = cortaDistancia;
        Destroy(gameObject, tiempoVida);
    }

    void Update()
    {
        transform.Translate(direccion * velocidad * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy3 enemy = other.GetComponent<Enemy3>();
            if (enemy != null)
            {
                enemy.RecibirDanio(damage); // Ahora damage es int
            }
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
                Enemy3 enemy = enemigo.GetComponent<Enemy3>();
                if (enemy != null)
                {
                    enemy.RecibirDanio(damage);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDano);
    }
}