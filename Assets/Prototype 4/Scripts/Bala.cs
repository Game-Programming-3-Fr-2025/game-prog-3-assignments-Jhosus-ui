using UnityEngine;

public class Bala : MonoBehaviour
{
    [Header("Configuraci�n de Bala")]
    public float velocidad = 10f;
    public float tiempoVida = 3f;

    [Header("Debug")]
    public bool mostrarDebug = false;

    private Vector2 direccion;
    private int damage;
    private float radioDano;
    private bool esCortaDistancia;
    private bool configurada = false;

    void Start()
    {
        gameObject.tag = "Bullet";
        Destroy(gameObject, tiempoVida);

        if (mostrarDebug)
        {
            Debug.Log($"Bala creada - Velocidad: {velocidad}, Direcci�n: {direccion}");
        }
    }

    public void SetConfiguracion(Vector2 dir, float dmg, float radio, bool cortaDistancia)
    {
        direccion = dir.normalized; // Asegurar que est� normalizada
        damage = (int)dmg;
        radioDano = radio;
        esCortaDistancia = cortaDistancia;
        configurada = true;

        if (mostrarDebug)
        {
            Debug.Log($"Bala configurada - Direcci�n: {direccion}, Da�o: {damage}, Velocidad: {velocidad}");
        }

        // Rotar la bala hacia la direcci�n de movimiento
        if (direccion != Vector2.zero)
        {
            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);
        }
    }

    void Update()
    {
        // Solo moverse si est� configurada
        if (configurada && direccion != Vector2.zero)
        {
            // Usar velocidad constante independiente del framerate
            Vector3 movimiento = direccion * velocidad * Time.deltaTime;
            transform.position += movimiento;

            if (mostrarDebug)
            {
                Debug.DrawRay(transform.position, direccion * 0.5f, Color.red, 0.1f);
            }
        }
        else if (!configurada)
        {
            // Si no est� configurada, moverse hacia la derecha por defecto
            transform.position += Vector3.right * velocidad * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (mostrarDebug)
        {
            Debug.Log($"Bala colision� con: {other.gameObject.name} (Tag: {other.tag})");
        }

        // Colisi�n con enemigo
        if (other.CompareTag("Enemy"))
        {
            Enemy3 enemy = other.GetComponent<Enemy3>();
            if (enemy != null)
            {
                enemy.RecibirDanio(damage);

                if (mostrarDebug)
                {
                    Debug.Log($"Bala hizo {damage} de da�o a {other.name}");
                }
            }

            // Aplicar da�o en �rea si corresponde
            if (radioDano > 0)
            {
                AplicarDanoArea();
            }

            Destroy(gameObject);
        }
    }

    private void AplicarDanoArea()
    {
        Collider2D[] enemigos = Physics2D.OverlapCircleAll(transform.position, radioDano);

        if (mostrarDebug)
        {
            Debug.Log($"Aplicando da�o en �rea - Radio: {radioDano}, Enemigos encontrados: {enemigos.Length}");
        }

        foreach (Collider2D enemigo in enemigos)
        {
            if (enemigo.CompareTag("Enemy"))
            {
                Enemy3 enemy = enemigo.GetComponent<Enemy3>();
                if (enemy != null)
                {
                    enemy.RecibirDanio(damage);

                    if (mostrarDebug)
                    {
                        Debug.Log($"Da�o en �rea aplicado a {enemigo.name}");
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Mostrar radio de da�o
        if (radioDano > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radioDano);
        }

        // Mostrar direcci�n de movimiento
        if (configurada && direccion != Vector2.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, direccion * 1f);
        }
    }

    // M�todo para configuraci�n r�pida sin par�metros extra
    public void SetDireccionSimple(Vector2 dir)
    {
        direccion = dir.normalized;
        damage = 10; // Valor por defecto
        radioDano = 0;
        esCortaDistancia = false;
        configurada = true;
    }

    // Getters para debugging
    public Vector2 GetDireccion() { return direccion; }
    public int GetDamage() { return damage; }
    public bool EstaConfigurada() { return configurada; }
}