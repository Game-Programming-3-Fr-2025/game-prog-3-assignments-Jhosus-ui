using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager7 : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float maxGameTime = 600f;
    [SerializeField] private float startDelay = 3f;
    [SerializeField] private TMP_Text timerText;

    [Header("EXP System")]
    [SerializeField] private TMP_Text expText;
    private int currentEXP = 1;

    [Header("Boss Wave")]
    [SerializeField] private float bossSpawnInterval = 150f;
    [SerializeField] private int bossSpawnCount = 3;

    public bool gameStarted { get; private set; }
    public bool gameEnded { get; private set; }

    private float currentTime;
    private float nextBossSpawnTime;
    private bool playerHasMoved;
    private bool restartScheduled;
    private Vector3 lastPlayerPosition;
    private Transform player;
    private SpawnerEnemy7 spawner;

    void Start()
    {
        currentTime = maxGameTime;
        nextBossSpawnTime = maxGameTime - bossSpawnInterval;

        spawner = FindObjectOfType<SpawnerEnemy7>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player)
            lastPlayerPosition = player.position;

        StartCoroutine(DelayedGameStart());
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

            if (currentTime <= 0 && !restartScheduled)
            {
                restartScheduled = true;
                StartCoroutine(RestartScene());
            }
        }
    }

    IEnumerator DelayedGameStart()
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
        if (currentTime <= nextBossSpawnTime)
        {
            spawner?.SpawnBossWave(bossSpawnCount);
            nextBossSpawnTime -= bossSpawnInterval;
        }
    }

    IEnumerator RestartScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void AddEXP(int amount)
    {
        currentEXP += amount;
        if (expText)
            expText.text = $"EXP: {currentEXP}";
    }

    public int GetCurrentEXP() => currentEXP;

    public bool CanSpawnEnemies() => gameStarted && playerHasMoved && !gameEnded;
}