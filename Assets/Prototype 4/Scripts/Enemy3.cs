using UnityEngine;

public class Enemy3 : MonoBehaviour
{
    public float velocidad = 2f;
    public int vida = 30;
    public int danioPorAtaque = 10;
    public float intervaloAtaque = 1f;
    public float fuerzaSalto = 5f;

    private bool atacando = false;
    private HealthP objetivoActual;
    private float siguienteAtaqueTime = 0f;
    private Rigidbody2D rb;
    private Vector3 posicionOriginal; // Para mantener la posición después de detenerse

    void Start()
    {
        gameObject.tag = "Enemy";
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!atacando)
        {
            // Movimiento normal hacia la izquierda
            transform.Translate(Vector3.left * velocidad * Time.deltaTime);
        }
        else
        {
            // Mantener la posición donde se detuvo
            transform.position = new Vector3(posicionOriginal.x, transform.position.y, transform.position.z);

            // Realizar salto y ataque cada intervalo
            if (Time.time >= siguienteAtaqueTime && objetivoActual != null)
            {
                SaltarYAtacar();
                siguienteAtaqueTime = Time.time + intervaloAtaque;
            }
        }
    }

    void FixedUpdate()
    {
        // Si está atacando, aplicar fuerza de salto periódicamente
        if (atacando && Time.time % 0.5f < 0.1f) // Salto cada ~0.5 segundos
        {
            AplicarFuerzaSalto();
        }
    }

    public void RecibirDanio(int cantidad)
    {
        vida -= cantidad;
        if (vida <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("Chasis") || other.CompareTag("Modulo")) && !atacando)
        {
            // Detener movimiento y comenzar ataque
            atacando = true;
            posicionOriginal = transform.position; // Guardar posición donde se detuvo
            objetivoActual = other.GetComponent<HealthP>();

            if (objetivoActual != null)
            {
                Debug.Log($"{gameObject.name} comenzó a atacar a {other.gameObject.name}");
                // Primer ataque inmediato
                SaltarYAtacar();
                siguienteAtaqueTime = Time.time + intervaloAtaque;
            }
        }

        if (other.CompareTag("Bullet"))
        {
            Bala bala = other.GetComponent<Bala>();
            if (bala != null)
            {
                RecibirDanio(10);
                Destroy(other.gameObject);
            }
        }
    }

    private void SaltarYAtacar()
    {
        if (objetivoActual == null)
        {
            atacando = false;
            return;
        }

        // Aplicar daño
        objetivoActual.RecibirDanio(danioPorAtaque);

        Debug.Log($"{gameObject.name} atacó a {objetivoActual.gameObject.name} (Salto)");
    }

    private void AplicarFuerzaSalto()
    {
        if (rb != null)
        {
            // Fuerza de salto hacia arriba
            rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
        }
    }

    // Si el objetivo se destruye mientras atacamos
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Chasis") || other.CompareTag("Modulo"))
        {
            if (other.GetComponent<HealthP>() == objetivoActual)
            {
                atacando = false;
                objetivoActual = null;
            }
        }
    }

    // Para evitar que se mueva horizontalmente mientras ataca
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (atacando && (collision.gameObject.CompareTag("Chasis") || collision.gameObject.CompareTag("Modulo")))
        {
            // Mantener posición X fija
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }
}