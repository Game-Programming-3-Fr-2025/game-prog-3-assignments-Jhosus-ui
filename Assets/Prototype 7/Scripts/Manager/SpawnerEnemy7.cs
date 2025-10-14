using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpawnerEnemy7 : MonoBehaviour
{
    [Header("Enemy Spawn")]
    public GameObject enemyPrefab;
    public float spawnInterval = 3f;
    public int minEnemies = 3;
    public int maxEnemies = 50;
    public int spawnIncreaseInterval = 3;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    public float minDistanceFromPlayer = 10f;

    [Header("Boss Spawn")]
    public GameObject bossPrefab;
    public Transform[] bossSpawnPoints;

    private float spawnTimer;
    private GameManager7 gameManager;
    private Transform player;
    private int currentWave = 0;
    private int currentSpawnCount;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager7>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spawnTimer = spawnInterval;
        currentSpawnCount = minEnemies;
    }

    void Update()
    {
        if (player == null || !gameManager.CanSpawnEnemies()) return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0)
        {
            SpawnEnemies();
            spawnTimer = spawnInterval;
            currentWave++;

            if (currentWave % spawnIncreaseInterval == 0 && currentSpawnCount < maxEnemies)
            {
                currentSpawnCount++;
                Debug.Log($"Spawn increased to {currentSpawnCount} enemies/wave");
            }
        }
    }

    void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab not assigned!");
            return;
        }

        var availablePoints = GetAvailableSpawnPoints();
        if (availablePoints.Count == 0)
        {
            Debug.LogWarning("No spawn points available (player too close)");
            return;
        }

        int count = Mathf.Min(currentSpawnCount, availablePoints.Count);
        for (int i = 0; i < count; i++)
        {
            var spawnPoint = availablePoints[Random.Range(0, availablePoints.Count)];
            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        }

        Debug.Log($"Wave {currentWave}: Spawned {count} enemies");
    }

    List<Transform> GetAvailableSpawnPoints()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return new List<Transform>();
        }

        return spawnPoints
            .Where(sp => sp != null && Vector2.Distance(player.position, sp.position) >= minDistanceFromPlayer)
            .ToList();
    }

    public void SpawnBossWave(int bossCount)
    {
        if (bossPrefab == null)
        {
            Debug.LogError("Boss Prefab not assigned!");
            return;
        }

        if (bossSpawnPoints == null || bossSpawnPoints.Length == 0)
        {
            Debug.LogError("Boss Spawn Points not assigned!");
            return;
        }

        for (int i = 0; i < bossCount; i++)
        {
            var spawnPoint = bossSpawnPoints[Random.Range(0, bossSpawnPoints.Length)];
            Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);
        }
        Debug.Log($"Boss wave spawned: {bossCount} bosses");
    }

    public bool AreAllBossesDefeated()
    {
        return GameObject.FindGameObjectsWithTag("Boss").Length == 0;
    }

    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;

        foreach (var point in spawnPoints.Where(p => p != null))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(point.position, 0.5f);

            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawSphere(point.position, minDistanceFromPlayer);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        foreach (var point in spawnPoints.Where(p => p != null))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(point.position, 0.3f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(point.position, minDistanceFromPlayer);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(point.position + Vector3.up, $"Dist: {minDistanceFromPlayer}m");
#endif
        }
    }
}