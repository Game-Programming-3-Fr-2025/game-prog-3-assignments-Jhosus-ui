using UnityEngine;
using System;

public class Enemy7 : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 1f;
    public bool isBoss = false;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Boss Settings")]
    public float bossHealthMultiplier = 5f;
    public float bossSizeMultiplier = 1.5f;
    public float bossSpeedMultiplier = 0.7f;

    [Header("EXP Drop")]
    public GameObject expPrefab;
    public int expDropCount = 3;
    public float expDropRadius = 2f;

    // Eventos para el pooling
    public event Action OnEnemyDeath;

    private Transform player;
    private UnityEngine.AI.NavMeshAgent navMeshAgent;
    private SpawnerEnemy7 spawner;
    private bool isInitialized = false;

    // Método Initialize que falta
    public void Initialize(SpawnerEnemy7 spawnerRef, bool isBossEnemy)
    {
        spawner = spawnerRef;
        isBoss = isBossEnemy;
        InitializeEnemy();
    }

    void Start()
    {
        if (!isInitialized)
        {
            InitializeEnemy();
        }
    }

    void InitializeEnemy()
    {
        if (isBoss)
        {
            maxHealth *= bossHealthMultiplier;
            moveSpeed *= bossSpeedMultiplier;
            transform.localScale *= bossSizeMultiplier;
        }

        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed;
            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;
        }

        isInitialized = true;
    }

    void OnEnable()
    {
        if (isInitialized)
        {
            currentHealth = maxHealth;
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = false;
            }
        }
    }

    void Update()
    {
        if (player != null && navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.SetDestination(player.position);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        SpawnEXP();
        OnEnemyDeath?.Invoke();

        if (spawner != null)
        {
            if (isBoss)
            {
                spawner.ReturnBossToPool(gameObject);
            }
            else
            {
                spawner.ReturnEnemyToPool(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SpawnEXP()
    {
        if (expPrefab == null) return;

        for (int i = 0; i < expDropCount; i++)
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * expDropRadius;
            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            GameObject exp = Instantiate(expPrefab, spawnPosition, Quaternion.identity);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            TakeDamage(10f);
        }
    }

    //void OnDisable()
    //{
    //    if (navMeshAgent != null)
    //    {
    //        navMeshAgent.isStopped = true;
    //    }
    //}

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isBoss ? Color.red : Color.blue;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, expDropRadius);
    }
}