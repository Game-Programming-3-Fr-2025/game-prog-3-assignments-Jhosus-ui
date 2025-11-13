using UnityEngine;

public class CastigFloor : MonoBehaviour
{
    [Header("Configuración Trampa")]
    public float riseSpeed = 2f;
    public float maxHeight = 3f;

    [Header("Punto de Respawn")]
    public Transform respawnPoint;

    private Vector3 startPos;
    private bool isActive = false;

    void Start()
    {
        startPos = transform.position;
        // Ocultar la trampa al inicio
        GetComponent<SpriteRenderer>().enabled = false;
    }

    void Update()
    {
        if (isActive)
        {
            // Mover hacia arriba
            transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);

            // Detener si llega a la altura máxima
            if (transform.position.y >= startPos.y + maxHeight)
            {
                isActive = false;
            }
        }
    }

    // Trigger para activar la trampa
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActive)
        {
            ActivateTrap();
        }
    }

    // Colisión cuando la trampa toca al jugador
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HurtPlayer(collision.gameObject);
        }
    }

    void ActivateTrap()
    {
        // Mostrar y activar la trampa
        GetComponent<SpriteRenderer>().enabled = true;
        isActive = true;
    }

    void HurtPlayer(GameObject player)
    {
        // Quitar vida
        P2Life life = player.GetComponent<P2Life>();
        if (life != null)
        {
            life.TakeDamage(1);
        }

        // Mover al punto de respawn
        if (respawnPoint != null)
        {
            player.transform.position = respawnPoint.position;
        }

        // Resetear la trampa
        ResetTrap();
    }

    void ResetTrap()
    {
        // Volver a posición inicial y ocultar
        transform.position = startPos;
        GetComponent<SpriteRenderer>().enabled = false;
        isActive = false;
    }

    // Dibujar gizmos en el editor
    void OnDrawGizmosSelected()
    {
        // Posición inicial (si no estamos en play mode, usar posición actual)
        Vector3 currentStartPos = Application.isPlaying ? startPos : transform.position;

        // Dibujar línea desde la posición inicial hasta la altura máxima
        Vector3 maxHeightPos = currentStartPos + Vector3.up * maxHeight;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(currentStartPos, maxHeightPos);

        // Dibujar esfera en la altura máxima
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(maxHeightPos, 0.3f);

        // Dibujar caja en la posición inicial
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(currentStartPos, GetComponent<Collider2D>().bounds.size);
    }
}