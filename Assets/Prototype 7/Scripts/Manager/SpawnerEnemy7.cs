using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpawnerEnemy7 : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Wave Settings")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int minEnemies = 3;
    [SerializeField] private int maxEnemies = 50;
    [SerializeField] private int waveIncrement = 3;
    [SerializeField] private float minDistanceFromPlayer = 10f;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 100;

    private Transform player;
    private GameManager7 gameManager;
    private float spawnTimer;
    private int currentWave;
    private int currentSpawnCount;

    private Queue<GameObject> enemyPool = new Queue<GameObject>();
    private Queue<GameObject> bossPool = new Queue<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();
    private List<GameObject> activeBosses = new List<GameObject>();

    void Start()
    {
        gameManager = FindObjectOfType<GameManager7>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        currentSpawnCount = minEnemies;

        InitializePools();
    }

    void Update()
    {
        if (!player || !gameManager.CanSpawnEnemies()) return;

        spawnTimer -= Time.deltaTime;
        
        if (spawnTimer <= 0)
        {
            SpawnWave();
            spawnTimer = spawnInterval;
            
            if (++currentWave % waveIncrement == 0 && currentSpawnCount < maxEnemies)
                currentSpawnCount++;
        }
    }

    void InitializePools() // Inicializa las colas de objetos
    {
        for (int i = 0; i < initialPoolSize; i++)
            CreatePooledObject(enemyPrefab, enemyPool, false);
        
        for (int i = 0; i < 10; i++)
            CreatePooledObject(bossPrefab ?? enemyPrefab, bossPool, true);
    }

    void SpawnWave()
    {
        var validPoints = GetValidSpawnPoints();
        int spawnCount = Mathf.Min(currentSpawnCount, validPoints.Count);

        for (int i = 0; i < spawnCount; i++)
            SpawnFromPool(enemyPool, activeEnemies, validPoints[Random.Range(0, validPoints.Count)].position, false);
    }

    public void SpawnBossWave(int count) 
    {
        var validPoints = GetValidSpawnPoints();
        Vector3 spawnPos = validPoints.FirstOrDefault()?.position ?? transform.position;

        for (int i = 0; i < count; i++)
            SpawnFromPool(bossPool, activeBosses, spawnPos, true);
    }

    void CreatePooledObject(GameObject prefab, Queue<GameObject> pool, bool isBoss) // Agregar parametro 
    {
        if (!prefab) return;

        var obj = Instantiate(prefab);
        obj.SetActive(false);

        var enemy = obj.GetComponent<Enemy7>();
        if (enemy)
        {
            enemy.Initialize(this, isBoss);
            enemy.OnEnemyDeath += () => ReturnToPool(obj, isBoss ? activeBosses : activeEnemies, pool);
        }

        pool.Enqueue(obj);
    }

    void SpawnFromPool(Queue<GameObject> pool, List<GameObject> activeList, Vector3 position, bool isBoss) //Ver si es un boss
    {
        if (pool.Count == 0 && activeList.Count + pool.Count < maxPoolSize)
            CreatePooledObject(isBoss ? (bossPrefab ?? enemyPrefab) : enemyPrefab, pool, isBoss);

        if (pool.Count > 0)
        {
            var obj = pool.Dequeue();
            obj.transform.position = position;
            obj.SetActive(true);
            activeList.Add(obj);
        }
    }

    void ReturnToPool(GameObject obj, List<GameObject> activeList, Queue<GameObject> pool) // Agregar parametro para saber si es boss
    {
        if (!obj) return;
        
        obj.SetActive(false);
        activeList.Remove(obj);
        pool.Enqueue(obj);
    }

    public void ReturnEnemyToPool(GameObject enemy) => ReturnToPool(enemy, activeEnemies, enemyPool);
    
    public void ReturnBossToPool(GameObject boss) => ReturnToPool(boss, activeBosses, bossPool);
    
    public bool AreAllBossesDefeated() => activeBosses.Count == 0;

    List<Transform> GetValidSpawnPoints() =>
        spawnPoints?.Where(sp => sp && Vector2.Distance(player.position, sp.position) >= minDistanceFromPlayer).ToList()
        ?? new List<Transform>();
}