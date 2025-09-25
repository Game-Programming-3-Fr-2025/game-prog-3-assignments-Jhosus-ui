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
        Debug.Log("EnemySpawner inicializado y evento registrado");
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
        // MEJORADO: Verificar si el juego está activo y manejar el spawning
        if (MoneyManager.Instance.isPlaying && !MoneyManager.Instance.isPausedForBoss && spawningCoroutine == null)
        {
            Debug.Log("Juego activado - Iniciando spawning de enemigos");
            IniciarSpawning();
        }
        else if ((!MoneyManager.Instance.isPlaying || MoneyManager.Instance.isPausedForBoss) && spawningCoroutine != null)
        {
            Debug.Log("Juego desactivado/pausado - Deteniendo spawning");
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
            // CRÍTICO: Verificar TODAS las condiciones necesarias para spawning
            if (MoneyManager.Instance.isPlaying && !MoneyManager.Instance.isPausedForBoss)
            {
                int num = Random.Range(minEnemies, maxEnemies + 1);
                for (int i = 0; i < num; i++)
                {
                    // VERIFICAR nuevamente antes de cada spawn individual
                    if (!MoneyManager.Instance.isPlaying || MoneyManager.Instance.isPausedForBoss)
                    {
                        Debug.Log("Condiciones cambiaron durante spawning - Saliendo del loop");
                        break;
                    }

                    GameObject nuevoEnemigo = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
                    enemigosActivos.Add(nuevoEnemigo);

                    // Configurar referencia al spawner en el enemigo
                    Enemy3 enemyScript = nuevoEnemigo.GetComponent<Enemy3>();
                    if (enemyScript != null)
                    {
                        enemyScript.SetSpawner(this);
                    }
                }
                Debug.Log($"Spawned {num} enemigos. Total activos: {enemigosActivos.Count}");
            }
            else
            {
                Debug.Log("Condiciones no cumplidas para spawning - Esperando...");
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void SpawnBosses(int count)
    {
        if (!MoneyManager.Instance.isPlaying)
        {
            Debug.Log("No se pueden spawnear bosses - juego no activo");
            return;
        }

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
        Debug.Log($"Spawned {count} bosses. Total bosses activos: {activeBosses}");
    }

    private void BossDefeated()
    {
        activeBosses--;
        Debug.Log($"Boss derrotado. Bosses restantes: {activeBosses}");

        if (activeBosses <= 0)
        {
            MoneyManager.Instance.BossesDefeated();
            Debug.Log("Todos los bosses derrotados - Progreso reanudado");
        }
    }

    public void ReiniciarSpawner()
    {
        Debug.Log("=== REINICIANDO SPAWNER ===");

        // 1. Detener INMEDIATAMENTE la corrutina de spawning
        DetenerSpawning();

        // 2. Limpiar todos los enemigos activos
        LimpiarEnemigos();

        // 3. Reiniciar TODOS los contadores y estados
        activeBosses = 0;

        Debug.Log("Spawner reiniciado completamente");
    }

    private void LimpiarEnemigos()
    {
        int enemigosEliminados = 0;

        // IMPORTANTE: Iterar hacia atrás para evitar problemas con índices
        for (int i = enemigosActivos.Count - 1; i >= 0; i--)
        {
            if (enemigosActivos[i] != null)
            {
                // CRÍTICO: Desuscribir eventos antes de destruir
                Enemy3 enemyScript = enemigosActivos[i].GetComponent<Enemy3>();
                if (enemyScript != null)
                {
                    enemyScript.onDeath -= BossDefeated; // Evitar llamadas después de destruir
                }

                Destroy(enemigosActivos[i]);
                enemigosEliminados++;
            }
        }

        enemigosActivos.Clear();
        Debug.Log($"Enemigos eliminados: {enemigosEliminados}. Lista limpiada.");
    }

    public void RemoverEnemigo(GameObject enemigo)
    {
        if (enemigosActivos.Contains(enemigo))
        {
            enemigosActivos.Remove(enemigo);
            Debug.Log($"Enemigo removido de la lista. Enemigos activos: {enemigosActivos.Count}");
        }
    }
}