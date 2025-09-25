using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI progressText;
    public Button startButton;
    public Button upgradeEnergyButton;
    public TextMeshProUGUI upgradeEnergyCostText;

    [Header("Dinero")]
    public int money = 0;
    public int moneyRewardPerEnemy = 10;

    [Header("Energy")]
    public int energy = 0;
    public float energyTimerBase = 10f;
    private float currentTimer;
    private float energyTimerCurrent = 10f;
    public int energyUpgradeLevel = 0;
    public int energyMaxLevel = 4;
    public float energyTimerReductionPerLevel = 0.4f;
    public int energyUpgradeBaseCost = 10;
    public int energyUpgradeIncrement = 3;

    [Header("Progreso")]
    public float progress = 0f;
    public float progressSpeed = 1f;
    private float currentSpeed;
    public float damageSlowdownFactor = 0.8f;
    public float slowdownDuration = 5f;

    [Header("Estado")]
    public bool isPlaying = false;
    public bool isPausedForBoss = false;
    public delegate void OnResetToLobby();
    public event OnResetToLobby onResetToLobby;

    // NUEVO: Variables para spawn de bosses mejorado
    private bool[] bossesSpawned = new bool[3]; // Para 25%, 75%, 98%

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Inicialización correcta
        energyTimerCurrent = energyTimerBase;
        currentTimer = energyTimerCurrent;
        currentSpeed = progressSpeed;
        progress = 0f;
        ResetBossSpawns();

        UpdateUI();

        startButton.onClick.AddListener(StartGame);
        upgradeEnergyButton.onClick.AddListener(UpgradeEnergy);
        upgradeEnergyButton.gameObject.SetActive(!isPlaying && energyUpgradeLevel < energyMaxLevel);
    }

    void Update()
    {
        if (isPlaying)
        {
            // PROGRESO CONTINUO - Solo se pausa por daño, NO por bosses
            if (progress < 100f)
            {
                progress += currentSpeed * Time.deltaTime;
                if (progress >= 100f) progress = 100f;
                CheckProgressMilestones();
            }

            // Timer energy
            currentTimer -= Time.deltaTime;
            if (currentTimer <= 0)
            {
                energy++;
                currentTimer = energyTimerCurrent;
            }
        }

        UpdateUI();

        upgradeEnergyButton.gameObject.SetActive(!isPlaying && energyUpgradeLevel < energyMaxLevel);
        if (upgradeEnergyCostText) upgradeEnergyCostText.text = GetEnergyUpgradeCost().ToString();
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateUI();
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    public bool SpendEnergy(int amount)
    {
        if (energy >= amount)
        {
            energy -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    public void ApplyDamageSlowdown()
    {
        currentSpeed *= damageSlowdownFactor;
        StartCoroutine(ResetSpeedAfterDelay());
    }

    private IEnumerator ResetSpeedAfterDelay()
    {
        yield return new WaitForSeconds(slowdownDuration);
        currentSpeed = progressSpeed;
    }

    // MEJORADO: Sistema de spawn de bosses que no pausa el progreso
    private void CheckProgressMilestones()
    {
        // 25% - Spawn primer boss
        if (progress >= 25f && !bossesSpawned[0])
        {
            SpawnBosses(1);
            bossesSpawned[0] = true;
            Debug.Log("25% alcanzado - Spawning boss 1");
        }
        // 75% - Spawn segundo grupo de bosses
        else if (progress >= 75f && !bossesSpawned[1])
        {
            SpawnBosses(2);
            bossesSpawned[1] = true;
            Debug.Log("75% alcanzado - Spawning boss 2");
        }
        // 98% - Spawn boss final
        else if (progress >= 98f && !bossesSpawned[2])
        {
            SpawnBosses(3);
            bossesSpawned[2] = true;
            Debug.Log("98% alcanzado - Spawning boss final");
        }
    }

    private void SpawnBosses(int count)
    {
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner) spawner.SpawnBosses(count);

        // ELIMINADO: Ya no pausamos el progreso por bosses
        // isPausedForBoss = true;
    }

    public void BossesDefeated()
    {
        // ELIMINADO: Ya no hay pausa que quitar
        // isPausedForBoss = false;
        Debug.Log("Bosses derrotados - El progreso continúa normal");
    }

    private void StartGame()
    {
        Debug.Log("=== INICIANDO JUEGO ===");

        isPlaying = true;
        startButton.gameObject.SetActive(false);

        // IMPORTANTE: Reiniciar TODOS los valores al comenzar juego
        progress = 0f;
        currentSpeed = progressSpeed;
        energy = 0;
        currentTimer = energyTimerCurrent;
        isPausedForBoss = false;
        ResetBossSpawns(); // Resetear spawns de bosses

        Debug.Log($"Juego iniciado - Progress: {progress}, Speed: {currentSpeed}, isPausedForBoss: {isPausedForBoss}");
    }

    private void ResetBossSpawns()
    {
        for (int i = 0; i < bossesSpawned.Length; i++)
        {
            bossesSpawned[i] = false;
        }
    }

    public void ResetToLobby()
    {
        Debug.Log("=== REINICIANDO AL LOBBY ===");

        // PASO 1: Cambiar estado inmediatamente
        isPlaying = false;
        isPausedForBoss = false;

        // PASO 2: Reiniciar valores de juego
        progress = 0f;
        currentSpeed = progressSpeed;
        energy = 0;
        currentTimer = energyTimerCurrent;
        ResetBossSpawns(); // Resetear spawns de bosses

        // PASO 3: Mostrar botón de inicio
        startButton.gameObject.SetActive(true);

        Debug.Log($"Estado después del reset - isPlaying: {isPlaying}, progress: {progress}, isPausedForBoss: {isPausedForBoss}");

        // PASO 4: Restablecer la vida de todos los objetos
        RestablecerVidaDeTodosLosObjetos();

        // PASO 5: Disparar evento para otros sistemas
        Debug.Log("Invocando evento onResetToLobby...");
        onResetToLobby?.Invoke();

        // PASO 6: Forzar actualización de UI
        UpdateUI();

        Debug.Log("=== REINICIO COMPLETADO ===");
    }

    private void RestablecerVidaDeTodosLosObjetos()
    {
        HealthP[] todosLosHealthP = FindObjectsOfType<HealthP>();
        foreach (HealthP health in todosLosHealthP)
        {
            health.RestablecerVida();
        }
        Debug.Log($"Vida restablecida en {todosLosHealthP.Length} objetos");
    }

    private void UpgradeEnergy()
    {
        int cost = GetEnergyUpgradeCost();
        if (SpendMoney(cost) && energyUpgradeLevel < energyMaxLevel)
        {
            energyUpgradeLevel++;
            energyTimerCurrent = energyTimerBase - (energyUpgradeLevel * energyTimerReductionPerLevel);
            currentTimer = energyTimerCurrent;
        }
    }

    private int GetEnergyUpgradeCost()
    {
        return energyUpgradeBaseCost + (energyUpgradeLevel * energyUpgradeIncrement);
    }

    private void UpdateUI()
    {
        if (moneyText) moneyText.text = money.ToString();
        if (energyText) energyText.text = energy.ToString();
        if (timerText) timerText.text = Mathf.Ceil(currentTimer).ToString();
        if (progressText) progressText.text = Mathf.Round(progress).ToString();
    }
}