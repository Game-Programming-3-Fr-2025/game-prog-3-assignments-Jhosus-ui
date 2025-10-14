using UnityEngine;

public class EXP : MonoBehaviour
{
    [Header("EXP Settings")]
    public float attractRadius = 3f; // Radio de atracción hacia el player
    public float moveSpeed = 5f; // Velocidad hacia el player
    public int expValue = 1; // Cuánto EXP suma al ser recolectado

    private Transform player;
    private bool isAttracted = false;
    private GameManager7 gameManager;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        gameManager = FindObjectOfType<GameManager7>();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Si el player está dentro del radio de atracción
        if (distanceToPlayer <= attractRadius)
        {
            isAttracted = true;
        }

        // Moverse hacia el player si está atraído
        if (isAttracted)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime);

            // Si está muy cerca del player, recolectar
            if (distanceToPlayer <= 0.5f)
            {
                CollectEXP();
            }
        }
    }

    void CollectEXP()
    {
        // Aquí puedes sumar el EXP al contador del player
        // Por ahora solo lo destruimos
        Debug.Log($"EXP recolectada: +{expValue}");

        // En el futuro aquí llamarías a: gameManager.AddEXP(expValue);

        Destroy(gameObject);
    }

    // Gizmos para visualizar el radio de atracción
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attractRadius);
    }
}