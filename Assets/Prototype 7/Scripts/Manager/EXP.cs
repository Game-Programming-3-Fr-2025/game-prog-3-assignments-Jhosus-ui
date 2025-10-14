using UnityEngine;

public class EXP : MonoBehaviour
{
    [Header("EXP Settings")]
    public float attractRadius = 3f;
    public float moveSpeed = 5f;
    public int expValue = 1;

    private Transform player;
    private bool isAttracted = false;
    private GameManager7 gameManager;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        gameManager = FindObjectOfType<GameManager7>();

        if (gameManager == null)
        {
            Debug.LogError("GameManager7 no encontrado en la escena!");
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attractRadius)
        {
            isAttracted = true;
        }

        if (isAttracted)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime);

            if (distanceToPlayer <= 0.3f) // Reducí la distancia para mejor detección
            {
                CollectEXP();
            }
        }
    }

    void CollectEXP()
    {
        // Llamar al GameManager para sumar EXP
        if (gameManager != null)
        {
            gameManager.AddEXP(expValue);
        }
        else
        {
            Debug.LogWarning("GameManager no encontrado, pero EXP recolectada: " + expValue);
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Método alternativo por si el movimiento no funciona bien
        if (other.CompareTag("Player"))
        {
            CollectEXP();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attractRadius);
    }
}