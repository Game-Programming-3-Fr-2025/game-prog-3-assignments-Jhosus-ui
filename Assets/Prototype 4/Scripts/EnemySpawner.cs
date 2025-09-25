using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject bossPrefab;
    public float spawnInterval = 3f;
    public int minEnemies = 1;
    public int maxEnemies = 5;

    private int activeBosses = 0;
    private List<GameObject> enemigosActivos = new List<GameObject>();
    private Coroutine spawningCoroutine;

    void Start()
    {
        // Registrar eventos
        MoneyManager.Instance.onResetToLobby += ReiniciarSpawner;

        // NUEVO: Registrar evento para cuando el juego comienza
        IniciarSpawning();
    }

    private void OnDestroy()
    {
        // Desregistrar eventos para evitar memory leaks
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.onResetToLobby -= ReiniciarSpawner;
        }
    }

    void Update()
    {
        // NUEVO: Verificar si el juego está activo y el spawner no está corriendo
        if (MoneyManager.Instance.isPlaying && spawningCoroutine == null)
        {
            Debug.Log("Juego activado - Reiniciando spawning de enemigos");
            IniciarSpawning();
        }
        else if (!MoneyManager.Instance.isPlaying && spawningCoroutine != null)
        {
            // Detener spawning si el juego se pausa
            DetenerSpawning();
        }
    }

    private void IniciarSpawning()
    {
        if (spawningCoroutine != null)
        {
            StopCoroutine(spawningCoroutine);
        }
        spawningCoroutine = StartCoroutine(SpawnEnemies());
        Debug.Log("Spawning de enemigos INICIADO");
    }

    private void DetenerSpawning()
    {
        if (spawningCoroutine != null)
        {
            StopCoroutine(spawningCoroutine);
            spawningCoroutine = null;
            Debug.Log("Spawning de enemigos DETENIDO");
        }
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            // IMPORTANTE: Verificar isPlaying en cada iteración
            if (MoneyManager.Instance.isPlaying && !MoneyManager.Instance.isPausedForBoss)
            {
                int num = Random.Range(minEnemies, maxEnemies + 1);
                for (int i = 0; i < num; i++)
                {
                    GameObject nuevoEnemigo = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
                    enemigosActivos.Add(nuevoEnemigo);

                    // Configurar referencia al spawner en el enemigo
                    Enemy3 enemyScript = nuevoEnemigo.GetComponent<Enemy3>();
                    if (enemyScript != null)
                    {
                        enemyScript.SetSpawner(this);
                    }
                }
                Debug.Log($"Spawned {num} enemigos");
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void SpawnBosses(int count)
    {
        if (!MoneyManager.Instance.isPlaying) return;

        activeBosses += count;
        for (int i = 0; i < count; i++)
        {
            GameObject boss = Instantiate(bossPrefab, transform.position, Quaternion.identity);
            enemigosActivos.Add(boss);

            // Configurar referencia al spawner en el boss
            Enemy3 bossScript = boss.GetComponent<Enemy3>();
            if (bossScript != null)
            {
                bossScript.SetSpawner(this);
                bossScript.onDeath += BossDefeated;
            }
        }
        Debug.Log($"Spawned {count} bosses");
    }

    private void BossDefeated()
    {
        activeBosses--;
        if (activeBosses <= 0)
        {
            MoneyManager.Instance.BossesDefeated();
            Debug.Log("Todos los bosses derrotados - Progreso reanudado");
        }
    }

    public void ReiniciarSpawner()
    {
        Debug.Log("Reiniciando spawner de enemigos...");

        // 1. Detener la corrutina de spawning
        DetenerSpawning();

        // 2. Eliminar todos los enemigos activos
        LimpiarEnemigos();

        // 3. Reiniciar contadores
        activeBosses = 0;

        Debug.Log("Spawner reiniciado completamente");
    }

    private void LimpiarEnemigos()
    {
        int enemigosEliminados = 0;

        for (int i = enemigosActivos.Count - 1; i >= 0; i--)
        {
            if (enemigosActivos[i] != null)
            {
                Destroy(enemigosActivos[i]);
                enemigosEliminados++;
            }
        }

        enemigosActivos.Clear();
        Debug.Log($"Enemigos eliminados: {enemigosEliminados}");
    }

    public void RemoverEnemigo(GameObject enemigo)
    {
        if (enemigosActivos.Contains(enemigo))
        {
            enemigosActivos.Remove(enemigo);
        }
    }
}