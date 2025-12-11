using UnityEngine;

public class Bala : MonoBehaviour
{
    [Header("Configuración de Bala")]
    public float velocidad = 10f, tiempoVida = 3f;

    [Header("Debug")]
    public bool mostrarDebug = false;

    private Vector2 direccion;
    private int damage;
    private float radioDano;
    private bool esCortaDistancia, configurada = false;

    void Start()
    {
        gameObject.tag = "Bullet";
        Destroy(gameObject, tiempoVida);
        if (mostrarDebug) Debug.Log($"Bala creada - Velocidad: {velocidad}, Dirección: {direccion}");
    }

    public void SetConfiguracion(Vector2 dir, float dmg, float radio, bool cortaDistancia)
    {
        direccion = dir.normalized; // Asegurar que esté normalizada
        damage = (int)dmg;
        radioDano = radio;
        esCortaDistancia = cortaDistancia;
        configurada = true;

        if (mostrarDebug) Debug.Log($"Bala configurada - Dirección: {direccion}, Daño: {damage}, Velocidad: {velocidad}");

        if (direccion != Vector2.zero) // Rotar la bala hacia la dirección de movimiento
        {
            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);
        }
    }

    void Update()
    {
        if (configurada && direccion != Vector2.zero) // Solo moverse si está configurada
        {
            Vector3 movimiento = direccion * velocidad * Time.deltaTime; // Usar velocidad constante independiente del framerate
            transform.position += movimiento;
            if (mostrarDebug) Debug.DrawRay(transform.position, direccion * 0.5f, Color.red, 0.1f);
        }
        else if (!configurada) transform.position += Vector3.right * velocidad * Time.deltaTime; // Si no está configurada, moverse hacia la derecha por defecto
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (mostrarDebug) Debug.Log($"Bala colisionó con: {other.gameObject.name} (Tag: {other.tag})");

        if (other.CompareTag("Enemy")) // Colisión con enemigo
        {
            Enemy3 enemy = other.GetComponent<Enemy3>();
            if (enemy != null)
            {
                enemy.RecibirDanio(damage);
                if (mostrarDebug) Debug.Log($"Bala hizo {damage} de daño a {other.name}");
            }

            if (radioDano > 0) AplicarDanoArea(); // Aplicar daño en área si corresponde
            Destroy(gameObject);
        }
    }

    private void AplicarDanoArea()
    {
        Collider2D[] enemigos = Physics2D.OverlapCircleAll(transform.position, radioDano);
        if (mostrarDebug) Debug.Log($"Aplicando daño en área - Radio: {radioDano}, Enemigos encontrados: {enemigos.Length}");

        foreach (Collider2D enemigo in enemigos)
        {
            if (enemigo.CompareTag("Enemy"))
            {
                Enemy3 enemy = enemigo.GetComponent<Enemy3>();
                if (enemy != null)
                {
                    enemy.RecibirDanio(damage);
                    if (mostrarDebug) Debug.Log($"Daño en área aplicado a {enemigo.name}");
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (radioDano > 0) // Mostrar radio de daño
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radioDano);
        }

        if (configurada && direccion != Vector2.zero) // Mostrar dirección de movimiento
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, direccion * 1f);
        }
    }

    public void SetDireccionSimple(Vector2 dir) // Método para configuración rápida sin parámetros extra
    {
        direccion = dir.normalized;
        damage = 10; // Valor por defecto
        radioDano = 0;
        esCortaDistancia = false;
        configurada = true;
    }

    public Vector2 GetDireccion() => direccion; // Getters para debugging
    public int GetDamage() => damage;
    public bool EstaConfigurada() => configurada;
}