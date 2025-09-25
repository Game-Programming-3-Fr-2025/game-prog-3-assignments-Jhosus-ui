using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; } // Singleton

    [Header("UI References")]
    public TextMeshProUGUI moneyText; // "Money: X"
    public TextMeshProUGUI energyText; // "Energy: X"
    public TextMeshProUGUI timerText; // "Next Energy: Xs"
    public TextMeshProUGUI progressText; // "Progress: X%"
    public Button startButton; // Botón Start UI
    public Button upgradeEnergyButton; // "+ Upgrade Energy"
    public TextMeshProUGUI upgradeEnergyCostText; // Costo de upgrade energy

    [Header("Dinero")]
    public int money = 0;
    public int moneyRewardPerEnemy = 10; // Ajusta

    [Header("Energy")]
    public int energy = 0;
    public float energyTimerBase = 10f; // Segundos base
    private float currentTimer;
    private float energyTimerCurrent = 10f; // Se reduce con upgrades
    public int energyUpgradeLevel = 0;
    public int energyMaxLevel = 4;
    public float energyTimerReductionPerLevel = 0.4f; // Ej. 10 -> 9.6
    public int energyUpgradeBaseCost = 10;
    public int energyUpgradeIncrement = 3;

    [Header("Progreso")]
    public float progress = 0f;
    public float progressSpeed = 1f; // Unidades por segundo
    private float currentSpeed;
    public float damageSlowdownFactor = 0.8f; // Reduce speed a 80%
    public float slowdownDuration = 5f; // Segundos

    [Header("Estado")]
    public bool isPlaying = false;
    public bool isPausedForBoss = false; // Para pausar progreso
    public delegate void OnResetToLobby();
    public event OnResetToLobby onResetToLobby;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentTimer = energyTimerCurrent;
        currentSpeed = progressSpeed;
        UpdateUI();

        startButton.onClick.AddListener(StartGame);
        upgradeEnergyButton.onClick.AddListener(UpgradeEnergy);
        upgradeEnergyButton.gameObject.SetActive(!isPlaying && energyUpgradeLevel < energyMaxLevel);
    }

    void Update()
    {
        if (isPlaying)
        {
            // Progreso
            if (!isPausedForBoss && progress < 100f)
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

        // Ocultar/mostrar upgrades
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

    private void CheckProgressMilestones()
    {
        if (progress >= 25f && progress < 26f) SpawnBosses(1);
        else if (progress >= 75f && progress < 76f) SpawnBosses(2);
        else if (progress >= 98f && progress < 99f) SpawnBosses(3);
    }

    private void SpawnBosses(int count)
    {
        // Llama a EnemySpawner (asumiendo referencia o evento)
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner) spawner.SpawnBosses(count);
        isPausedForBoss = true; // Pausa progreso
    }

    public void BossesDefeated()
    {
        isPausedForBoss = false;
    }

    private void StartGame()
    {
        isPlaying = true;
        startButton.gameObject.SetActive(false);
        // Ocultar todos los upgrades en otros scripts (ellos checkean isPlaying)
    }

    public void ResetToLobby()
    {
        isPlaying = false;
        progress = 0f;
        startButton.gameObject.SetActive(true);

        // NUEVO: Restablecer la vida de todos los objetos con HealthP
        RestablecerVidaDeTodosLosObjetos();
        onResetToLobby?.Invoke();
    }

    // NUEVO MÉTODO: Restablece la vida de chasis y módulos
    private void RestablecerVidaDeTodosLosObjetos()
    {
        HealthP[] todosLosHealthP = FindObjectsOfType<HealthP>();
        foreach (HealthP health in todosLosHealthP)
        {
            health.RestablecerVida();
        }
        Debug.Log("Vida restablecida para todos los objetos");
    }


    private void UpgradeEnergy()
    {
        int cost = GetEnergyUpgradeCost();
        if (SpendMoney(cost) && energyUpgradeLevel < energyMaxLevel)
        {
            energyUpgradeLevel++;
            energyTimerCurrent -= energyTimerReductionPerLevel;
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