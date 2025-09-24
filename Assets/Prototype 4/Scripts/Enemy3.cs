using UnityEngine;

public class Enemy3 : MonoBehaviour
{
    [Header("Configuración del Enemigo")]
    public int vidaMinima = 30;
    public int vidaMaxima = 50;
    public float velocidad = 2f;
    public int damageAlContacto = 10;

    [Header("Referencias")]
    public string playerTag = "Player";

    [Header("Debug")]
    public bool mostrarGizmos = true;

    private int vidaActual;
    private EnemySpawner spawner;
    private GameObject player;
    private bool estaMuerto = false;

    void Start()
    {
        vidaActual = Random.Range(vidaMinima, vidaMaxima + 1);
        BuscarJugador();
        Debug.Log($"Enemigo generado con {vidaActual} HP");
    }

    private void BuscarJugador()
    {
        // Buscar jugador cada vez que se ejecute
        player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            Debug.LogWarning($"No se encontró jugador con tag '{playerTag}'");
            // Intentar buscar por nombre común
            player = GameObject.Find("Player");
        }

        if (player != null)
        {
            Debug.Log($"Jugador encontrado: {player.name}");
        }
    }

    void Update()
    {
        if (estaMuerto) return;

        // Re-buscar jugador si se perdió la referencia
        if (player == null)
        {
            BuscarJugador();
        }

        Movimiento();
    }

    private void Movimiento()
    {
        Vector3 direccion = Vector3.left; // Por defecto izquierda

        if (player != null)
        {
            // Calcular dirección hacia el jugador
            Vector3 playerPos = player.transform.position;
            Vector3 myPos = transform.position;

            direccion = (playerPos - myPos).normalized;

            // Debug para ver si está calculando bien
            Debug.DrawLine(myPos, playerPos, Color.red, 0.1f);
            Debug.Log($"Enemigo moviéndose hacia jugador. Dirección: {direccion}");
        }
        else
        {
            Debug.Log("Jugador no encontrado, moviéndose a la izquierda");
        }

        // Aplicar movimiento - Verificar si tiene Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Usar Rigidbody2D para el movimiento
            rb.linearVelocity = direccion * velocidad;
        }
        else
        {
            // Usar Transform para el movimiento
            transform.position += direccion * velocidad * Time.deltaTime;
        }

        // Voltear sprite según dirección
        Vector3 scale = transform.localScale;
        if (direccion.x > 0)
        {
            scale.x = Mathf.Abs(scale.x) * -1; // Mirando derecha
        }
        else
        {
            scale.x = Mathf.Abs(scale.x); // Mirando izquierda
        }
        transform.localScale = scale;
    }

    public void RecibirDanio(int cantidad)
    {
        if (estaMuerto) return;

        vidaActual -= cantidad;
        Debug.Log($"Enemigo recibió {cantidad} de daño. Vida restante: {vidaActual}");

        StartCoroutine(FlashDanio());

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private System.Collections.IEnumerator FlashDanio()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color original = sprite.color;
            sprite.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sprite.color = original;
        }
    }

    private void Morir()
    {
        estaMuerto = true;

        if (spawner != null)
        {
            spawner.OnEnemyDestroyed();
        }

        Debug.Log("Enemigo derrotado!");
        Destroy(gameObject, 0.1f); // Pequeño delay para evitar errores
    }

    public void SetSpawner(EnemySpawner enemySpawner)
    {
        spawner = enemySpawner;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (estaMuerto) return;

        if (other.CompareTag(playerTag))
        {
            Debug.Log("Enemigo tocó al jugador");

            // Buscar script de salud del jugador
            var playerHealth = other.GetComponent<MonoBehaviour>();
            if (playerHealth != null)
            {
                // Intentar llamar método de daño si existe
                playerHealth.SendMessage("TakeDamage", damageAlContacto, SendMessageOptions.DontRequireReceiver);
            }

            Morir();
        }
    }

    void OnDrawGizmos()
    {
        if (!mostrarGizmos) return;

        // Dibujar línea hacia el jugador
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.transform.position);
        }

        // Dibujar círculo de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    [ContextMenu("Buscar Jugador Manualmente")]
    public void DebugFindPlayer()
    {
        BuscarJugador();
    }

    [ContextMenu("Matar Enemigo")]
    public void DebugKill()
    {
        Morir();
    }

    [ContextMenu("Recibir 10 de Daño")]
    public void DebugDamage()
    {
        RecibirDanio(10);
    }
}