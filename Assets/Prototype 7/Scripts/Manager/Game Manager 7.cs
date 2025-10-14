using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager7 : MonoBehaviour
{
    [Header("Timer Settings")]
    public float maxGameTime = 600f; // 10 minutos en segundos
    public float startDelay = 3f;
    public TMP_Text timerText;

    [Header("Boss Settings")]
    public int bossSpawnCount = 3;
    private bool[] bossWaveSpawned = new bool[3]; // Para 7.5, 5 y 2.5 minutos
    private bool finalBossSpawned = false;
    private bool waitingForBossClear = false;

    [Header("Game State")]
    public bool gameStarted = false;
    public bool gameEnded = false;

    private float currentTime;
    private bool playerHasMoved = false;
    private Vector3 lastPlayerPosition;
    private SpawnerEnemy7 spawner;

    void Start()
    {
        currentTime = maxGameTime;
        spawner = FindObjectOfType<SpawnerEnemy7>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            lastPlayerPosition = player.transform.position;
        }

        StartCoroutine(StartGameAfterDelay());
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (!gameStarted || gameEnded) return;

        CheckPlayerMovement();

        // Si estamos esperando que eliminen los bosses, no reducimos el timer
        if (waitingForBossClear)
        {
            CheckBossClear();
            return;
        }

        // Reducir timer solo si el jugador se ha movido y no estamos en pausa de bosses
        if (playerHasMoved && currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();
            CheckBossSpawn();

            if (currentTime <= 0 && !finalBossSpawned)
            {
                TriggerFinalBosses();
            }
        }
    }

    IEnumerator StartGameAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        gameStarted = true;
    }

    void CheckPlayerMovement()
    {
        if (playerHasMoved) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && Vector3.Distance(lastPlayerPosition, player.transform.position) > 0.1f)
        {
            playerHasMoved = true;
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Cambiar color cuando queden 10 segundos
            if (currentTime <= 10f)
            {
                timerText.color = Color.red;
            }
        }
    }

    void CheckBossSpawn()
    {
        // Spawn bosses cada 2.5 minutos (150 segundos)
        float[] bossTimes = { 450f, 300f, 150f }; // 7.5min, 5min, 2.5min

        for (int i = 0; i < bossTimes.Length; i++)
        {
            if (currentTime <= bossTimes[i] && currentTime > bossTimes[i] - 1f && !bossWaveSpawned[i])
            {
                bossWaveSpawned[i] = true;
                TriggerBossWave();
                break;
            }
        }
    }

    void TriggerBossWave()
    {
        if (spawner != null)
        {
            spawner.SpawnBossWave(bossSpawnCount);
            Debug.Log($"GameManager: Solicitando oleada de {bossSpawnCount} bosses");
        }
    }

    void TriggerFinalBosses()
    {
        finalBossSpawned = true;
        waitingForBossClear = true;
        TriggerBossWave();
        Debug.Log("GameManager: ¡Jefes finales! Elimínalos todos para terminar el juego.");
    }

    void CheckBossClear()
    {
        if (spawner != null && spawner.AreAllBossesDefeated())
        {
            EndGame();
        }
    }

    void EndGame()
    {
        gameEnded = true;
        Debug.Log("GameManager: ¡Juego terminado! Todos los bosses eliminados.");
    }

    public bool CanSpawnEnemies()
    {
        return gameStarted && playerHasMoved && !gameEnded && !waitingForBossClear;
    }
}