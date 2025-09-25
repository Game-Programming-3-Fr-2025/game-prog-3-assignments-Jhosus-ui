using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject bossPrefab; // Nuevo: Prefab de enemigo fuerte
    public float spawnInterval = 3f;
    public int minEnemies = 1;
    public int maxEnemies = 5;

    private int activeBosses = 0;
    private List<GameObject> enemigosActivos = new List<GameObject>();
    private Coroutine spawningCoroutine;

    void Start()
    {
        // Registrar evento para saber cuando se reinicia el lobby
        MoneyManager.Instance.onResetToLobby += ReiniciarSpawner;
        IniciarSpawning();
    }

    private void OnDestroy()
    {
        // Desregistrar evento para evitar memory leaks
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.onResetToLobby -= ReiniciarSpawner;
        }
    }

    private void IniciarSpawning()
    {
        if (spawningCoroutine != null)
        {
            StopCoroutine(spawningCoroutine);
        }
        spawningCoroutine = StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            if (MoneyManager.Instance.isPlaying)
            {
                int num = Random.Range(minEnemies, maxEnemies + 1);
                for (int i = 0; i < num; i++)
                {
                    GameObject nuevoEnemigo = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
                    enemigosActivos.Add(nuevoEnemigo);
                }
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void SpawnBosses(int count)
    {
        activeBosses += count;
        for (int i = 0; i < count; i++)
        {
            GameObject boss = Instantiate(bossPrefab, transform.position, Quaternion.identity);
            enemigosActivos.Add(boss);

            // Asume boss tiene script similar a Enemy3, y al morir llama a BossDefeated
            Enemy3 bossScript = boss.GetComponent<Enemy3>();
            if (bossScript) bossScript.onDeath += BossDefeated;
        }
    }

    private void BossDefeated()
    {
        activeBosses--;
        if (activeBosses <= 0)
        {
            MoneyManager.Instance.BossesDefeated();
        }
    }

    // NUEVO MÉTODO: Reinicia completamente el spawner
    public void ReiniciarSpawner()
    {
        Debug.Log("Reiniciando spawner de enemigos...");

        // 1. Detener la corrutina de spawning
        if (spawningCoroutine != null)
        {
            StopCoroutine(spawningCoroutine);
            spawningCoroutine = null;
        }

        // 2. Eliminar todos los enemigos activos
        LimpiarEnemigos();

        // 3. Reiniciar contadores
        activeBosses = 0;

        // 4. El spawning se reiniciará automáticamente cuando isPlaying vuelva a true
    }

    // NUEVO MÉTODO: Limpia todos los enemigos existentes
    private void LimpiarEnemigos()
    {
        int enemigosEliminados = 0;

        foreach (GameObject enemigo in enemigosActivos)
        {
            if (enemigo != null)
            {
                Destroy(enemigo);
                enemigosEliminados++;
            }
        }

        enemigosActivos.Clear();
        Debug.Log($"Enemigos eliminados: {enemigosEliminados}");
    }

    // NUEVO MÉTODO: Para remover enemigos de la lista cuando mueren naturalmente
    public void RemoverEnemigo(GameObject enemigo)
    {
        if (enemigosActivos.Contains(enemigo))
        {
            enemigosActivos.Remove(enemigo);
        }
    }
}