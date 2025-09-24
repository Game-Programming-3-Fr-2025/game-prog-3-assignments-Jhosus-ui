using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración Básica")]
    public GameObject enemyPrefab;
    public float spawnInterval = 3f;
    public int maxEnemies = 10;

    [Header("Spawn Settings")]
    public Vector3 spawnScale = Vector3.one; // Escala del enemigo al spawnearse
    public bool randomizeSpawnPosition = false;
    public float spawnRadius = 1f;

    [Header("Debug")]
    public bool showGizmos = true;

    private int currentEnemies = 0;
    private bool isSpawning = false;

    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPrefab no asignado en EnemySpawner");
            return;
        }

        StartCoroutine(SpawnEnemies());
        Debug.Log("EnemySpawner iniciado");
    }

    private IEnumerator SpawnEnemies()
    {
        isSpawning = true;

        while (isSpawning)
        {
            if (currentEnemies < maxEnemies)
            {
                SpawnSingleEnemy();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnSingleEnemy()
    {
        Vector3 spawnPosition = transform.position;

        // Posición aleatoria si está habilitada
        if (randomizeSpawnPosition)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            spawnPosition += new Vector3(randomOffset.x, randomOffset.y, 0);
        }

        // Crear enemigo
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // APLICAR ESCALA CORRECTA
        newEnemy.transform.localScale = spawnScale;

        // Configurar script del enemigo
        Enemy3 enemyScript = newEnemy.GetComponent<Enemy3>();
        if (enemyScript != null)
        {
            enemyScript.SetSpawner(this);
        }
        else
        {
            Debug.LogWarning("El prefab del enemigo no tiene el script Enemy3");
        }

        currentEnemies++;
        Debug.Log($"Enemigo generado en {spawnPosition}. Total: {currentEnemies}");
    }

    public void OnEnemyDestroyed()
    {
        currentEnemies--;
        currentEnemies = Mathf.Max(0, currentEnemies);
        Debug.Log($"Enemigo destruido. Restantes: {currentEnemies}");
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            StartCoroutine(SpawnEnemies());
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Dibujar punto de spawn
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // Dibujar radio de spawn aleatorio
        if (randomizeSpawnPosition)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }

    [ContextMenu("Spawn Test Enemy")]
    public void SpawnTestEnemy()
    {
        if (currentEnemies < maxEnemies)
        {
            SpawnSingleEnemy();
        }
        else
        {
            Debug.Log("Máximo de enemigos alcanzado");
        }
    }

    [ContextMenu("Clear All Enemies")]
    public void ClearAllEnemies()
    {
        Enemy3[] enemies = FindObjectsOfType<Enemy3>();
        foreach (Enemy3 enemy in enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        currentEnemies = 0;
        Debug.Log("Todos los enemigos eliminados");
    }

    [ContextMenu("Stop Spawning")]
    public void DebugStopSpawning()
    {
        StopSpawning();
    }

    [ContextMenu("Start Spawning")]
    public void DebugStartSpawning()
    {
        StartSpawning();
    }
}