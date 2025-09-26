using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI moneyText, energyText, timerText, progressText, upgradeEnergyCostText;
    public Button startButton, upgradeEnergyButton;

    [Header("Dinero")]
    public int money = 0, moneyRewardPerEnemy = 10;

    [Header("Energy")]
    public int energy = 0, energyUpgradeLevel = 0, energyMaxLevel = 4;
    public float energyTimerBase = 10f, energyTimerReductionPerLevel = 0.4f;
    public int energyUpgradeBaseCost = 10, energyUpgradeIncrement = 3;
    private float currentTimer, energyTimerCurrent = 10f;

    [Header("Progreso")]
    public float progress = 0f, progressSpeed = 1f, damageSlowdownFactor = 0.8f, slowdownDuration = 5f;
    private float currentSpeed;

    [Header("Estado")]
    public bool isPlaying = false, isPausedForBoss = false;
    public delegate void OnResetToLobby();
    public event OnResetToLobby onResetToLobby;

    private bool[] bossesSpawned = new bool[3]; // Para 25%, 75%, 98%

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
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
            if (progress < 100f) // Progreso continuo - solo se pausa por daño
            {
                progress += currentSpeed * Time.deltaTime;
                if (progress >= 100f) progress = 100f;
                CheckProgressMilestones();
            }

            currentTimer -= Time.deltaTime; // Timer energy
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

    private void CheckProgressMilestones() // Sistema de spawn de bosses que no pausa el progreso
    {
        if (progress >= 25f && !bossesSpawned[0])
        {
            SpawnBosses(1);
            bossesSpawned[0] = true;
            Debug.Log("25% alcanzado - Spawning boss 1");
        }
        else if (progress >= 75f && !bossesSpawned[1])
        {
            SpawnBosses(2);
            bossesSpawned[1] = true;
            Debug.Log("75% alcanzado - Spawning boss 2");
        }
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
    }

    public void BossesDefeated() => Debug.Log("Bosses derrotados - El progreso continúa normal");

    private void StartGame()
    {
        Debug.Log("=== INICIANDO JUEGO ===");
        isPlaying = true;
        startButton.gameObject.SetActive(false);
        progress = 0f; // Reiniciar TODOS los valores al comenzar juego
        currentSpeed = progressSpeed;
        energy = 0;
        currentTimer = energyTimerCurrent;
        isPausedForBoss = false;
        ResetBossSpawns();
        Debug.Log($"Juego iniciado - Progress: {progress}, Speed: {currentSpeed}, isPausedForBoss: {isPausedForBoss}");
    }

    private void ResetBossSpawns()
    {
        for (int i = 0; i < bossesSpawned.Length; i++) bossesSpawned[i] = false;
    }

    public void ResetToLobby()
    {
        Debug.Log("=== REINICIANDO AL LOBBY ===");
        isPlaying = false; // Cambiar estado inmediatamente
        isPausedForBoss = false;
        progress = 0f; // Reiniciar valores de juego
        currentSpeed = progressSpeed;
        energy = 0;
        currentTimer = energyTimerCurrent;
        ResetBossSpawns();
        startButton.gameObject.SetActive(true);
        Debug.Log($"Estado después del reset - isPlaying: {isPlaying}, progress: {progress}, isPausedForBoss: {isPausedForBoss}");
        RestablecerVidaDeTodosLosObjetos(); // Restablecer la vida de todos los objetos
        Debug.Log("Invocando evento onResetToLobby...");
        onResetToLobby?.Invoke(); // Disparar evento para otros sistemas
        UpdateUI(); // Forzar actualización de UI
        Debug.Log("=== REINICIO COMPLETADO ===");
    }

    private void RestablecerVidaDeTodosLosObjetos()
    {
        HealthP[] todosLosHealthP = FindObjectsOfType<HealthP>();
        foreach (HealthP health in todosLosHealthP) health.RestablecerVida();
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

    private int GetEnergyUpgradeCost() => energyUpgradeBaseCost + (energyUpgradeLevel * energyUpgradeIncrement);

    private void UpdateUI()
    {
        if (moneyText) moneyText.text = money.ToString();
        if (energyText) energyText.text = energy.ToString();
        if (timerText) timerText.text = Mathf.Ceil(currentTimer).ToString();
        if (progressText) progressText.text = Mathf.Round(progress).ToString();
    }
}