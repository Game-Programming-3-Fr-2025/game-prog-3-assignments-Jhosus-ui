using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SimpleVehicle : MonoBehaviour
{
    [Header("Vehicle Setup")]
    public GameObject chassisBase; // El chasis principal del jugador
    public Transform wheelsContainer; // Contenedor de las ruedas (se mantiene fijo)
    public Transform moduleSpawnPoint; // Donde aparecen los módulos (encima del chasis)

    [Header("Modules")]
    public GameObject[] modulePrefabs; // 5 tipos de módulos (débil a fuerte)
    public float moduleHeight = 1f; // Altura de cada módulo

    [Header("Game State")]
    public bool gameStarted = false;
    public bool gameOver = false;
    public int currentMoney = 0;
    public float currentEnergy = 0f;
    public float maxEnergy = 100f;
    public float energyRegenRate = 20f; // Por segundo

    [Header("UI References")]
    public Text moneyText;
    public Text energyText;
    public Slider energyBar;
    public Button startButton;
    public Slider progressBar; // Barra de progreso del recorrido

    [Header("Upgrade Buttons")]
    public GameObject upgradePanel; // Panel que contiene todos los botones de mejora
    public Button addModuleButtonPrefab; // Prefab del botón para añadir módulos
    public Button upgradeModuleButtonPrefab; // Prefab del botón para mejorar módulos
    public Button weaponButtonPrefab; // Prefab del botón para armas
    public Button energyUpgradeButton; // Botón para mejorar regeneración de energía

    [Header("Progress System")]
    public float totalDistance = 1000f; // Distancia total del nivel
    public float currentDistance = 0f;

    // Variables internas
    private List<ModuleData> vehicleModules = new List<ModuleData>();
    private float energyRegenMultiplier = 1f;
    private List<GameObject> currentUpgradeButtons = new List<GameObject>();

    // Costos base
    private int baseModuleCost = 50;
    private int baseUpgradeCost = 100;
    private int baseEnergyUpgradeCost = 200;

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (gameStarted && !gameOver)
        {
            RegenerateEnergy();
            UpdateProgress();
        }

        UpdateUI();
    }

    void InitializeGame()
    {
        // Configurar el estado inicial
        gameOver = false;
        gameStarted = false;

        // Configurar UI inicial
        upgradePanel.SetActive(false);
        startButton.gameObject.SetActive(true);

        // Configurar el botón de inicio
        startButton.onClick.AddListener(StartGame);

        // Configurar botón de mejora de energía
        energyUpgradeButton.onClick.AddListener(UpgradeEnergyRegen);

        UpdateUI();
    }

    public void StartGame()
    {
        gameStarted = true;
        gameOver = false;
        currentDistance = 0f;

        // Ocultar panel de mejoras y botón de inicio
        upgradePanel.SetActive(false);
        startButton.gameObject.SetActive(false);

        Debug.Log("Juego iniciado");
    }

    void RegenerateEnergy()
    {
        if (currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenRate * energyRegenMultiplier * Time.deltaTime;
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
        }
    }

    void UpdateProgress()
    {
        // Simular progreso automático (puedes ajustar la velocidad)
        currentDistance += 10f * Time.deltaTime;

        if (currentDistance >= totalDistance)
        {
            // Nivel completado
            WinLevel();
        }
    }

    void UpdateUI()
    {
        // Actualizar textos y barras
        if (moneyText != null)
            moneyText.text = "Money: $" + currentMoney;

        if (energyText != null)
            energyText.text = Mathf.RoundToInt(currentEnergy) + "/" + Mathf.RoundToInt(maxEnergy);

        if (energyBar != null)
            energyBar.value = currentEnergy / maxEnergy;

        if (progressBar != null)
            progressBar.value = currentDistance / totalDistance;

        // Actualizar botón de mejora de energía
        if (energyUpgradeButton != null)
        {
            int cost = GetEnergyUpgradeCost();
            energyUpgradeButton.GetComponentInChildren<Text>().text = "Energy Upgrade $" + cost;
            energyUpgradeButton.interactable = currentMoney >= cost;
        }
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;

        // Si el juego terminó, actualizar botones de mejora
        if (gameOver)
        {
            UpdateUpgradeButtons();
        }
    }

    public bool SpendEnergy(float amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            return true;
        }
        return false;
    }

    public void TakeDamageToModule()
    {
        if (vehicleModules.Count > 0)
        {
            // Atacar el módulo más bajo (índice 0)
            vehicleModules[0].currentHealth -= 20f; // Daño por ataque

            if (vehicleModules[0].currentHealth <= 0)
            {
                DestroyLowestModule();
            }
        }
        else
        {
            // No hay módulos, atacar chasis principal
            TakeDamageToBase();
        }
    }

    void DestroyLowestModule()
    {
        if (vehicleModules.Count > 0)
        {
            // Destruir el módulo más bajo visualmente
            Destroy(vehicleModules[0].moduleObject);
            vehicleModules.RemoveAt(0);

            // Reposicionar módulos restantes
            RepositionModules();

            Debug.Log("Módulo destruido. Módulos restantes: " + vehicleModules.Count);
        }
    }

    void RepositionModules()
    {
        // Reposicionar todos los módulos hacia abajo
        for (int i = 0; i < vehicleModules.Count; i++)
        {
            Vector3 newPos = moduleSpawnPoint.position + Vector3.up * (moduleHeight * i);
            vehicleModules[i].moduleObject.transform.position = newPos;
        }
    }

    void TakeDamageToBase()
    {
        // Si atacan el chasis base, game over
        GameOver();
    }

    void GameOver()
    {
        gameOver = true;
        gameStarted = false;

        // Mostrar panel de mejoras
        upgradePanel.SetActive(true);
        startButton.gameObject.SetActive(true);

        // Generar botones de mejora
        GenerateUpgradeButtons();

        Debug.Log("Game Over. Dinero disponible: $" + currentMoney);
    }

    void WinLevel()
    {
        // Completó el nivel
        gameStarted = false;
        currentMoney += 500; // Bonus por completar nivel

        upgradePanel.SetActive(true);
        startButton.gameObject.SetActive(true);
        GenerateUpgradeButtons();

        Debug.Log("¡Nivel completado! Bonus: $500");
    }

    void GenerateUpgradeButtons()
    {
        // Limpiar botones anteriores
        ClearUpgradeButtons();

        // Generar botón para añadir módulo
        if (vehicleModules.Count < 5) // Máximo 5 módulos
        {
            CreateAddModuleButton();
        }

        // Generar botones para mejorar módulos existentes
        for (int i = 0; i < vehicleModules.Count; i++)
        {
            CreateUpgradeModuleButton(i);
            CreateWeaponButtons(i);
        }
    }

    void CreateAddModuleButton()
    {
        int cost = GetAddModuleCost();

        GameObject buttonObj = Instantiate(addModuleButtonPrefab.gameObject, upgradePanel.transform);
        Button button = buttonObj.GetComponent<Button>();

        button.GetComponentInChildren<Text>().text = "+ Module $" + cost;
        button.interactable = currentMoney >= cost;
        button.onClick.AddListener(() => AddModule(cost));

        currentUpgradeButtons.Add(buttonObj);
    }

    void CreateUpgradeModuleButton(int moduleIndex)
    {
        if (vehicleModules[moduleIndex].level >= 5) return; // Máximo nivel

        int cost = GetUpgradeModuleCost(vehicleModules[moduleIndex].level);

        GameObject buttonObj = Instantiate(upgradeModuleButtonPrefab.gameObject, upgradePanel.transform);
        Button button = buttonObj.GetComponent<Button>();

        button.GetComponentInChildren<Text>().text = "Upgrade $" + cost;
        button.interactable = currentMoney >= cost;
        button.onClick.AddListener(() => UpgradeModule(moduleIndex, cost));

        currentUpgradeButtons.Add(buttonObj);
    }

    void CreateWeaponButtons(int moduleIndex)
    {
        // Si ya tiene arma, crear botón de mejora de arma
        if (vehicleModules[moduleIndex].hasWeapon)
        {
            CreateWeaponUpgradeButton(moduleIndex);
            return;
        }

        // Crear botones para los 3 tipos de armas
        string[] weaponNames = { "Ballesta", "SMG", "Cohetes" };
        int[] weaponCosts = { 150, 300, 500 };

        for (int i = 0; i < weaponNames.Length; i++)
        {
            int weaponType = i;
            int cost = weaponCosts[i];

            GameObject buttonObj = Instantiate(weaponButtonPrefab.gameObject, upgradePanel.transform);
            Button button = buttonObj.GetComponent<Button>();

            button.GetComponentInChildren<Text>().text = weaponNames[i] + " $" + cost;
            button.interactable = currentMoney >= cost;
            button.onClick.AddListener(() => AddWeapon(moduleIndex, weaponType, cost));

            currentUpgradeButtons.Add(buttonObj);
        }
    }

    void CreateWeaponUpgradeButton(int moduleIndex)
    {
        int cost = GetWeaponUpgradeCost(vehicleModules[moduleIndex].weaponLevel);

        GameObject buttonObj = Instantiate(weaponButtonPrefab.gameObject, upgradePanel.transform);
        Button button = buttonObj.GetComponent<Button>();

        button.GetComponentInChildren<Text>().text = "+ Weapon Upgrade $" + cost;
        button.interactable = currentMoney >= cost;
        button.onClick.AddListener(() => UpgradeWeapon(moduleIndex, cost));

        currentUpgradeButtons.Add(buttonObj);
    }

    void AddModule(int cost)
    {
        if (currentMoney >= cost && vehicleModules.Count < 5)
        {
            currentMoney -= cost;

            // Crear módulo básico (nivel 1)
            Vector3 spawnPos = moduleSpawnPoint.position + Vector3.up * (moduleHeight * vehicleModules.Count);
            GameObject newModule = Instantiate(modulePrefabs[0], spawnPos, Quaternion.identity);

            ModuleData moduleData = new ModuleData
            {
                moduleObject = newModule,
                level = 1,
                maxHealth = 100f,
                currentHealth = 100f,
                hasWeapon = false,
                weaponType = -1,
                weaponLevel = 0
            };

            vehicleModules.Add(moduleData);
            UpdateUpgradeButtons();

            Debug.Log("Módulo añadido. Total: " + vehicleModules.Count);
        }
    }

    void UpgradeModule(int moduleIndex, int cost)
    {
        if (currentMoney >= cost)
        {
            currentMoney -= cost;
            vehicleModules[moduleIndex].level++;
            vehicleModules[moduleIndex].maxHealth += 50f;
            vehicleModules[moduleIndex].currentHealth = vehicleModules[moduleIndex].maxHealth;

            // Cambiar sprite del módulo al siguiente nivel
            int spriteIndex = Mathf.Min(vehicleModules[moduleIndex].level - 1, modulePrefabs.Length - 1);
            vehicleModules[moduleIndex].moduleObject.GetComponent<SpriteRenderer>().sprite =
                modulePrefabs[spriteIndex].GetComponent<SpriteRenderer>().sprite;

            UpdateUpgradeButtons();

            Debug.Log("Módulo mejorado a nivel " + vehicleModules[moduleIndex].level);
        }
    }

    void AddWeapon(int moduleIndex, int weaponType, int cost)
    {
        if (currentMoney >= cost)
        {
            currentMoney -= cost;
            vehicleModules[moduleIndex].hasWeapon = true;
            vehicleModules[moduleIndex].weaponType = weaponType;
            vehicleModules[moduleIndex].weaponLevel = 1;

            // Aquí conectarías con el script de Guns para instanciar el arma
            Debug.Log("Arma añadida al módulo " + moduleIndex + ": tipo " + weaponType);

            UpdateUpgradeButtons();
        }
    }

    void UpgradeWeapon(int moduleIndex, int cost)
    {
        if (currentMoney >= cost)
        {
            currentMoney -= cost;
            vehicleModules[moduleIndex].weaponLevel++;

            UpdateUpgradeButtons();

            Debug.Log("Arma mejorada a nivel " + vehicleModules[moduleIndex].weaponLevel);
        }
    }

    void UpgradeEnergyRegen()
    {
        int cost = GetEnergyUpgradeCost();
        if (currentMoney >= cost)
        {
            currentMoney -= cost;
            energyRegenMultiplier *= 1.5f; // 50% más rápido cada mejora

            Debug.Log("Regeneración de energía mejorada. Multiplicador: " + energyRegenMultiplier);
        }
    }

    void UpdateUpgradeButtons()
    {
        // Regenerar todos los botones
        GenerateUpgradeButtons();
    }

    void ClearUpgradeButtons()
    {
        foreach (GameObject button in currentUpgradeButtons)
        {
            if (button != null)
                Destroy(button);
        }
        currentUpgradeButtons.Clear();
    }

    // Métodos para calcular costos
    int GetAddModuleCost()
    {
        return baseModuleCost * (vehicleModules.Count + 1) * (vehicleModules.Count + 1);
    }

    int GetUpgradeModuleCost(int currentLevel)
    {
        return baseUpgradeCost * currentLevel * 2;
    }

    int GetWeaponUpgradeCost(int currentLevel)
    {
        return 100 * currentLevel * 2;
    }

    int GetEnergyUpgradeCost()
    {
        return Mathf.RoundToInt(baseEnergyUpgradeCost * Mathf.Pow(2, energyRegenMultiplier - 1));
    }
}

[System.Serializable]
public class ModuleData
{
    public GameObject moduleObject;
    public int level;
    public float maxHealth;
    public float currentHealth;
    public bool hasWeapon;
    public int weaponType; // 0=Ballesta, 1=SMG, 2=Cohetes
    public int weaponLevel;
}