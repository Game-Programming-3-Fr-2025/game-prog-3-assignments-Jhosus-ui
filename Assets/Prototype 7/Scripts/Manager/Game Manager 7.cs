using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager7 : MonoBehaviour
{
    [Header("Settings")]
    public float maxGameTime = 600f;
    public float startDelay = 3f;
    public int bossSpawnCount = 3;
    public TMP_Text timerText;

    [Header("EXP System")]
    public TMP_Text expText; // UI para mostrar EXP
    private int currentEXP = 0;

    public bool gameStarted { get; private set; }
    public bool gameEnded { get; private set; }

    private float currentTime;
    private bool playerHasMoved;
    private bool waitingForBossClear;
    private bool finalBossSpawned;
    private Vector3 lastPlayerPosition;
    private SpawnerEnemy7 spawner;
    private Transform player;
    private readonly float[] bossTimes = { 450f, 300f, 150f };
    private bool[] bossWaveSpawned = new bool[3];

    void Start()
    {
        currentTime = maxGameTime;
        spawner = FindObjectOfType<SpawnerEnemy7>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player)
            lastPlayerPosition = player.position;

        StartCoroutine(StartGameAfterDelay());
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (!gameStarted || gameEnded) return;

        CheckPlayerMovement();

        if (waitingForBossClear)
        {
            CheckBossClear();
            return;
        }

        if (playerHasMoved && currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();
            CheckBossSpawn();

            if (currentTime <= 0 && !finalBossSpawned)
                TriggerFinalBosses();
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

    public void AddEXP(int amount)
    {
        currentEXP += amount;
        UpdateEXPDisplay();
        Debug.Log($"EXP total: {currentEXP}");
    }

    void UpdateEXPDisplay()
    {
        if (expText != null)
        {
            expText.text = $"EXP: {currentEXP}";
        }
    }

    void CheckBossSpawn()
    {
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
        if (spawner)
            spawner.SpawnBossWave(bossSpawnCount);
    }

    void TriggerFinalBosses()
    {
        finalBossSpawned = true;
        waitingForBossClear = true;
        TriggerBossWave();
    }

    void CheckBossClear()
    {
        if (spawner && spawner.AreAllBossesDefeated())
            EndGame();
    }

    void EndGame()
    {
        gameEnded = true;
    }

    public bool CanSpawnEnemies() =>
        gameStarted && playerHasMoved && !gameEnded && !waitingForBossClear;
}