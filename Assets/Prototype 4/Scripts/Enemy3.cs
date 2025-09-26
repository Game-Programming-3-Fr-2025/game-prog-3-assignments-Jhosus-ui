using TMPro;
using UnityEditor;
using UnityEngine;

public class Enemy3 : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Velocidad mínima aleatoria")]
    public float velocidadMin = 1.5f;
    [Tooltip("Velocidad máxima aleatoria")]
    public float velocidadMax = 3f;
    private float velocidad; // Velocidad aleatoria asignada

    public int vida = 30, danioPorAtaque = 10;
    public float intervaloAtaque = 1f, fuerzaSalto = 5f;

    private bool atacando = false;
    private HealthP objetivoActual;
    private float siguienteAtaqueTime = 0f, siguienteSaltoTime = 0f, posicionXFija;
    private Rigidbody2D rb;

    public delegate void OnDeath();
    public event OnDeath onDeath;
    private EnemySpawner spawner;

    public void SetSpawner(EnemySpawner enemySpawner) => spawner = enemySpawner;

    void Start()
    {
        gameObject.tag = "Enemy";
        rb = GetComponent<Rigidbody2D>();
        spawner = FindObjectOfType<EnemySpawner>();
        AsignarVelocidadAleatoria(); // Asignar velocidad aleatoria al crear el enemigo
    }

    void Update()
    {
        if (!atacando)
        {
            transform.Translate(Vector3.left * velocidad * Time.deltaTime); // Movimiento normal hacia la izquierda con velocidad aleatoria
        }
        else
        {
            if (objetivoActual == null || objetivoActual.gameObject == null) // Verificar si el objetivo sigue existiendo
            {
                DetenerAtaque();
                return;
            }

            Vector3 pos = transform.position; // Mantener posición X fija durante ataque
            pos.x = posicionXFija;
            transform.position = pos;

            if (Time.time >= siguienteAtaqueTime) // Aplicar daño en intervalos controlados
            {
                RealizarAtaque();
                siguienteAtaqueTime = Time.time + intervaloAtaque;
            }
        }
    }

    void FixedUpdate()
    {
        if (atacando)
        {
            if (Time.time >= siguienteSaltoTime) // Salto controlado por timer
            {
                AplicarSalto();
                siguienteSaltoTime = Time.time + 0.5f; // Salto cada 0.5 segundos
            }

            if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Mantener velocidad horizontal en 0
        }
    }

    private void AsignarVelocidadAleatoria() // Método para asignar velocidad aleatoria
    {
        velocidad = Random.Range(velocidadMin, velocidadMax);
        Debug.Log($"{gameObject.name} velocidad asignada: {velocidad:F2} (rango: {velocidadMin}-{velocidadMax})");
    }

    public void RecibirDanio(int cantidad)
    {
        vida -= cantidad;
        Debug.Log($"{gameObject.name} recibió {cantidad} daño. Vida: {vida}");

        if (vida <= 0)
        {
            MoneyManager.Instance.AddMoney(MoneyManager.Instance.moneyRewardPerEnemy);
            onDeath?.Invoke(); // Para bosses
            spawner?.RemoverEnemigo(gameObject); // Notificar al spawner que este enemigo murió
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("Chasis") || other.CompareTag("Modulo")) && !atacando) // Verificar que no esté ya atacando otro objetivo
        {
            HealthP health = other.GetComponent<HealthP>();
            if (health != null) IniciarAtaque(health);
        }

        if (other.CompareTag("Bullet"))
        {
            Bala bala = other.GetComponent<Bala>();
            int danio = bala?.GetDamage() ?? 10; // Daño por defecto si no hay componente Bala
            RecibirDanio(danio);
            Destroy(other.gameObject);
        }
    }

    private void IniciarAtaque(HealthP objetivo)
    {
        atacando = true;
        objetivoActual = objetivo;
        posicionXFija = transform.position.x;
        siguienteAtaqueTime = Time.time + 0.5f; // Primer ataque en 0.5 segundos
        siguienteSaltoTime = Time.time + 0.1f; // Primer salto casi inmediato
        Debug.Log($"{gameObject.name} inició ataque contra {objetivo.gameObject.name}");
    }

    private void RealizarAtaque()
    {
        if (objetivoActual == null) return;
        objetivoActual.RecibirDanio(danioPorAtaque);
        Debug.Log($"{gameObject.name} atacó por {danioPorAtaque} daño");
    }

    private void DetenerAtaque()
    {
        atacando = false;
        objetivoActual = null;
        Debug.Log($"{gameObject.name} detuvo el ataque - objetivo destruido");
    }

    private void AplicarSalto()
    {
        if (rb != null && Mathf.Abs(rb.linearVelocity.y) < 1f) // Solo saltar si no está ya saltando
            rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if ((other.CompareTag("Chasis") || other.CompareTag("Modulo")) && atacando)
        {
            HealthP health = other.GetComponent<HealthP>();
            if (health == objetivoActual) DetenerAtaque();
        }
    }
}