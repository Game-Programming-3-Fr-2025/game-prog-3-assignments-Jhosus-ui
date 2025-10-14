using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpawnerEnemy7 : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject enemyPrefab;
    public GameObject bossPrefab;
    public Transform[] spawnPoints;

    [Header("Spawn")]
    public float spawnInterval = 3f;
    public int minEnemies = 3;
    public int maxEnemies = 50;
    public int waveIncrement = 3;
    public float minDistanceFromPlayer = 10f;

    [Header("Pooling")]
    public int initialPoolSize = 20;
    public int maxPoolSize = 100;

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

        for (int i = 0; i < initialPoolSize; i++) CreatePooledObject(enemyPrefab, enemyPool, false);
        for (int i = 0; i < 10; i++) CreatePooledObject(bossPrefab ?? enemyPrefab, bossPool, true);
    }

    void Update()
    {
        if (!player || !gameManager.CanSpawnEnemies()) return;

        if ((spawnTimer -= Time.deltaTime) <= 0)
        {
            SpawnWave();
            spawnTimer = spawnInterval;
            if (++currentWave % waveIncrement == 0 && currentSpawnCount < maxEnemies) currentSpawnCount++;
        }
    }

    void SpawnWave()
    {
        var points = GetValidSpawnPoints();
        int count = Mathf.Min(currentSpawnCount, points.Count);

        for (int i = 0; i < count; i++)
            SpawnFromPool(enemyPool, activeEnemies, points[Random.Range(0, points.Count)].position, false);
    }

    public void SpawnBossWave(int count)
    {
        for (int i = 0; i < count; i++)
            SpawnFromPool(bossPool, activeBosses, GetValidSpawnPoints().FirstOrDefault()?.position ?? transform.position, true);
    }

    void CreatePooledObject(GameObject prefab, Queue<GameObject> pool, bool isBoss)
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

    void SpawnFromPool(Queue<GameObject> pool, List<GameObject> activeList, Vector3 position, bool isBoss)
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

    void ReturnToPool(GameObject obj, List<GameObject> activeList, Queue<GameObject> pool)
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

    // GIZMOS PARA VISUALIZAR LA DISTANCIA MÍNIMA DEL PLAYER
    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;

        // Dibujar área de exclusión alrededor de cada spawn point
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Naranja semitransparente

        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, minDistanceFromPlayer);
            }
        }
    }
}