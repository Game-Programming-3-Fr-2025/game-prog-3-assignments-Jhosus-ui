using UnityEngine;

public class EXP : MonoBehaviour
{
    [Header("EXP Settings")]
    [SerializeField] private float attractRadius = 3f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int expValue = 1;

    private Transform player;
    private GameManager7 gameManager;
    private bool isAttracted;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        gameManager = FindObjectOfType<GameManager7>();
    }

    void Update()
    {
        if (!player) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (!isAttracted && distance <= attractRadius)
            isAttracted = true;

        if (isAttracted)
        {
            transform.Translate((player.position - transform.position).normalized * moveSpeed * Time.deltaTime);

            if (distance <= 0.3f)
                Collect();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            Collect();
    }

    private void Collect()
    {
        gameManager?.AddEXP(expValue);
        Destroy(gameObject);
    }
}