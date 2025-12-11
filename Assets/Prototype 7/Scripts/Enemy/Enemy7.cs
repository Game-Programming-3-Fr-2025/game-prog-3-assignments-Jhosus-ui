using UnityEngine;
using UnityEngine.AI;
using System;

public class Enemy7 : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 1f;
    [SerializeField] private float maxHealth = 100f;

    [Header("Boss Multipliers")]
    [SerializeField] private float bossHealthMultiplier = 5f;
    [SerializeField] private float bossSizeMultiplier = 1.5f;
    [SerializeField] private float bossSpeedMultiplier = 0.7f;

    [Header("EXP Drop")]
    [SerializeField] private GameObject expPrefab;
    [SerializeField] private int expDropCount = 3;
    [SerializeField] private float expDropRadius = 2f;

    public event Action OnEnemyDeath;

    private Transform player;
    private NavMeshAgent navMeshAgent;
    private SpawnerEnemy7 spawner;
    private float currentHealth;
    private bool isBoss;
    private bool isInitialized;

    public void Initialize(SpawnerEnemy7 spawnerRef, bool isBossEnemy)
    {
        spawner = spawnerRef;
        isBoss = isBossEnemy;
        Setup();
    }

    void Start()
    {
        if (!isInitialized)
            Setup();
    }

    void Setup()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (isBoss)
        {
            maxHealth *= bossHealthMultiplier;
            moveSpeed *= bossSpeedMultiplier;
            transform.localScale *= bossSizeMultiplier;
        }

        currentHealth = maxHealth;

        if (navMeshAgent)
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
            if (navMeshAgent)
                navMeshAgent.isStopped = false;
        }
    }

    void Update()
    {
        if (player && navMeshAgent && navMeshAgent.isActiveAndEnabled)
            navMeshAgent.SetDestination(player.position);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        DropEXP();
        OnEnemyDeath?.Invoke();

        if (spawner)
        {
            if (isBoss)
                spawner.ReturnBossToPool(gameObject);
            else
                spawner.ReturnEnemyToPool(gameObject);
        }
        else
            Destroy(gameObject);
    }

    void DropEXP()
    {
        if (!expPrefab) return;

        for (int i = 0; i < expDropCount; i++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * expDropRadius;
            Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0);
            Instantiate(expPrefab, spawnPos, Quaternion.identity);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
            TakeDamage(10f);
    }
}