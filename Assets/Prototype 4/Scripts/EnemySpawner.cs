using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab, bossPrefab;
    public float spawnInterval = 3f;
    public int minEnemies = 1, maxEnemies = 5;

    private int activeBosses = 0;
    private List<GameObject> enemigosActivos = new List<GameObject>();
    private Coroutine spawningCoroutine;

    void Start()
    {
        if (MoneyManager.Instance != null) MoneyManager.Instance.onResetToLobby += ReiniciarSpawner; // Registrar eventos
        IniciarSpawning(); // Registrar evento para cuando el juego comienza
    }

    void Update()
    {
        if (MoneyManager.Instance.isPlaying && spawningCoroutine == null) // Verificar si el juego está activo y el spawner no está corriendo
        {
            Debug.Log("Juego activado - Reiniciando spawning de enemigos");
            IniciarSpawning();
        }
        else if (!MoneyManager.Instance.isPlaying && spawningCoroutine != null) DetenerSpawning(); // Detener spawning si el juego se pausa
    }

    private void OnDestroy() // Desregistrar eventos para evitar memory leaks
    {
        if (MoneyManager.Instance != null) MoneyManager.Instance.onResetToLobby -= ReiniciarSpawner;
    }

    private void IniciarSpawning()
    {
        if (spawningCoroutine != null) StopCoroutine(spawningCoroutine);
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
            if (MoneyManager.Instance.isPlaying && !MoneyManager.Instance.isPausedForBoss) // Verificar isPlaying en cada iteración
            {
                int num = Random.Range(minEnemies, maxEnemies + 1);
                for (int i = 0; i < num; i++)
                {
                    GameObject nuevoEnemigo = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
                    enemigosActivos.Add(nuevoEnemigo);
                    Enemy3 enemyScript = nuevoEnemigo.GetComponent<Enemy3>(); // Configurar referencia al spawner en el enemigo
                    if (enemyScript != null) enemyScript.SetSpawner(this);
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
            Enemy3 bossScript = boss.GetComponent<Enemy3>(); // Configurar referencia al spawner en el boss
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
        DetenerSpawning(); // Detener la corrutina de spawning
        LimpiarEnemigos(); // Eliminar todos los enemigos activos
        activeBosses = 0; // Reiniciar contadores
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
        if (enemigosActivos.Contains(enemigo)) enemigosActivos.Remove(enemigo);
    }
}