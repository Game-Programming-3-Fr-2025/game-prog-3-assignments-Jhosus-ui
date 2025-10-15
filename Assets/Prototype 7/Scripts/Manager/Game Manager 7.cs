using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager7 : MonoBehaviour
{
    [Header("Settings")]
    public float maxGameTime = 600f;
    public float startDelay = 3f;
    public int bossSpawnCount = 3;
    public TMP_Text timerText;

    [Header("EXP System")]
    public TMP_Text expText;
    private int currentEXP = 1;

    [Header("Boss Spawn")]
    public float bossSpawnInterval = 150f; // Cada 2.5 minutos

    public bool gameStarted { get; private set; }
    public bool gameEnded { get; private set; }

    private float currentTime;
    private bool playerHasMoved;
    private Vector3 lastPlayerPosition;
    private SpawnerEnemy7 spawner;
    private Transform player;
    private float nextBossSpawnTime;
    private bool reinicioProgramado = false;

    void Start()
    {
        currentTime = maxGameTime;
        spawner = FindObjectOfType<SpawnerEnemy7>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player)
            lastPlayerPosition = player.position;

        nextBossSpawnTime = maxGameTime - bossSpawnInterval; // Primer boss a los 7.5 minutos

        StartCoroutine(StartGameAfterDelay());
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (!gameStarted || gameEnded) return;

        CheckPlayerMovement();

        if (playerHasMoved && currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();
            CheckBossSpawn();

            // Reiniciar cuando el tiempo llegue a 0
            if (currentTime <= 0 && !reinicioProgramado)
            {
                reinicioProgramado = true;
                StartCoroutine(ReiniciarEscena());
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
        if (playerHasMoved || !player) return;

        if (Vector3.Distance(lastPlayerPosition, player.position) > 0.1f)
            playerHasMoved = true;
    }

    void UpdateTimerDisplay()
    {
        if (!timerText) return;

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        if (currentTime <= 10f)
            timerText.color = Color.red;
    }

    void CheckBossSpawn()
    {
        // Spawnear bosses cada X tiempo
        if (currentTime <= nextBossSpawnTime)
        {
            TriggerBossWave();
            nextBossSpawnTime -= bossSpawnInterval; // Siguiente boss
        }
    }

    void TriggerBossWave()
    {
        if (spawner)
        {
            spawner.SpawnBossWave(bossSpawnCount);
            Debug.Log($"¡Oleada de {bossSpawnCount} bosses apareció! Tiempo: {currentTime:F0}s");
        }
    }

    IEnumerator ReiniciarEscena()
    {
        Debug.Log("Tiempo agotado! Reiniciando escena...");

        // Pequeño delay antes del reinicio
        yield return new WaitForSeconds(2f);

        // Reiniciar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void AddEXP(int amount)
    {
        currentEXP += amount;
        UpdateEXPDisplay();
    }

    void UpdateEXPDisplay()
    {
        if (expText != null)
        {
            expText.text = $"EXP: {currentEXP}";
        }
    }

    public int GetCurrentEXP()
    {
        return currentEXP;
    }

    public bool CanSpawnEnemies() =>
        gameStarted && playerHasMoved && !gameEnded;
}