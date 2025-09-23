using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SimpleVehicle : MonoBehaviour
{
    [Header("Configuración Base")]
    public float baseSpeed = 3f;
    public float baseMaxHealth = 100f;
    public float currentHealth;
    public int money = 0;
    public TextMeshProUGUI moneyText;

    [Header("Sistema de Módulos Apilados")]
    public GameObject chassis; // El chasis base (player)
    public GameObject[] modulePrefabs; // Prefabs de módulos nivel 1-5
    public Transform moduleContainer; // Donde se instancian los módulos
    public float moduleHeight = 0.5f; // Altura de cada módulo
    public int maxModules = 5; // Máximo de módulos apilados

    [Header("Configuración de Llantas")]
    public Transform[] wheels; // Arrastra aquí las llantas del vehículo
    public float wheelRotationSpeed = 180f; // Velocidad de rotación en grados/segundo

    [Header("UI de Mejoras")]
    public Button addModuleButton;
    public Button upgradeModuleButton;
    public TextMeshProUGUI moduleInfoText;
    public GameObject upgradePanel; // Panel que se muestra al morir

    // Lista de módulos instalados (de abajo hacia arriba)
    private List<VehicleModule> installedModules = new List<VehicleModule>();
    private bool isAlive = true;
    private bool inUpgradePhase = false;

    [System.Serializable]
    public class VehicleModule
    {
        public GameObject moduleObject;
        public int level;
        public float maxHealth;
        public float currentHealth;
        public int upgradeCost;
        public int addCost;
    }

    void Start()
    {
        currentHealth = baseMaxHealth;
        UpdateMoneyUI();
        SetupInitialChassis();
    }

    void SetupInitialChassis()
    {
        // El chasis base actúa como módulo nivel 0
        VehicleModule chassisModule = new VehicleModule();
        chassisModule.moduleObject = chassis;
        chassisModule.level = 0;
        chassisModule.maxHealth = baseMaxHealth;
        chassisModule.currentHealth = baseMaxHealth;
        installedModules.Add(chassisModule);
    }

    void Update()
    {
        if (isAlive && !inUpgradePhase)
        {
            MoveForward();
            RotateWheels(); // ← NUEVO: Rotar llantas continuamente
        }
    }

    void MoveForward()
    {
        transform.Translate(Vector3.right * baseSpeed * Time.deltaTime);
    }

    // NUEVO: Rotación continua de las llantas para efecto visual
    void RotateWheels()
    {
        if (wheels == null || wheels.Length == 0) return;

        float rotationAmount = wheelRotationSpeed * Time.deltaTime;

        foreach (Transform wheel in wheels)
        {
            if (wheel != null)
            {
                // Rotar cada llanta alrededor de su eje Z (para 2D)
                wheel.Rotate(0, 0, -rotationAmount);
            }
        }
    }

    public void EnterUpgradePhase()
    {
        inUpgradePhase = true;
        upgradePanel.SetActive(true);
        UpdateUpgradeUI();
    }

    public void ExitUpgradePhase()
    {
        inUpgradePhase = false;
        upgradePanel.SetActive(false);
        RespawnVehicle();
    }

    // BOTÓN: Añadir nuevo módulo en la parte superior
    public void AddNewModule()
    {
        if (installedModules.Count >= maxModules + 1) return;

        int cost = GetAddModuleCost();
        if (money >= cost)
        {
            money -= cost;
            InstallNewModule(1);
            UpdateMoneyUI();
            UpdateUpgradeUI();
        }
    }

    // BOTÓN: Mejorar módulo existente
    public void UpgradeTopModule()
    {
        if (installedModules.Count <= 1) return;

        VehicleModule topModule = installedModules[installedModules.Count - 1];
        if (topModule.level >= 5) return;

        int cost = topModule.upgradeCost;
        if (money >= cost)
        {
            money -= cost;
            UpgradeModule(topModule);
            UpdateMoneyUI();
            UpdateUpgradeUI();
        }
    }

    void InstallNewModule(int level)
    {
        if (level < 1 || level > 5) return;

        GameObject newModuleObj = Instantiate(modulePrefabs[level - 1], moduleContainer);

        // Calcular posición (encima del módulo anterior)
        float yPos = (installedModules.Count - 1) * moduleHeight;
        newModuleObj.transform.localPosition = new Vector3(0, yPos, 0);

        VehicleModule newModule = new VehicleModule();
        newModule.moduleObject = newModuleObj;
        newModule.level = level;
        newModule.maxHealth = GetModuleHealth(level);
        newModule.currentHealth = newModule.maxHealth;
        newModule.upgradeCost = GetUpgradeCost(level);
        newModule.addCost = GetAddModuleCost();

        installedModules.Add(newModule);
        UpdateVehicleStats();

        // NUEVO: Ajustar posición del chasis y módulos para mantener llantas fijas
        AdjustVehicleHeight();
    }

    void UpgradeModule(VehicleModule module)
    {
        int newLevel = module.level + 1;
        if (newLevel > 5) return;

        Destroy(module.moduleObject);

        GameObject upgradedModule = Instantiate(modulePrefabs[newLevel - 1], moduleContainer);
        float yPos = (installedModules.IndexOf(module) - 1) * moduleHeight;
        upgradedModule.transform.localPosition = new Vector3(0, yPos, 0);

        module.moduleObject = upgradedModule;
        module.level = newLevel;
        module.maxHealth = GetModuleHealth(newLevel);
        module.currentHealth = module.maxHealth;
        module.upgradeCost = GetUpgradeCost(newLevel);

        UpdateVehicleStats();
    }

    // NUEVO: Ajustar la altura del contenedor de módulos para mantener llantas en posición fija
    void AdjustVehicleHeight()
    {
        if (moduleContainer != null)
        {
            // El contenedor se eleva, pero las llantas permanecen en su posición mundial
            float totalHeight = (installedModules.Count - 1) * moduleHeight;
            moduleContainer.localPosition = new Vector3(0, totalHeight, 0);
        }
    }

    void RecalculateModulePositions()
    {
        for (int i = 0; i < installedModules.Count; i++)
        {
            if (installedModules[i].level > 0)
            {
                float yPos = (i - 1) * moduleHeight;
                installedModules[i].moduleObject.transform.localPosition = new Vector3(0, yPos, 0);
            }
        }

        // NUEVO: Reajustar altura después de recalcular posiciones
        AdjustVehicleHeight();
    }

    // Cuando un enemigo ataca, daña el módulo superior primero
    public void TakeDamage(float damage)
    {
        if (installedModules.Count == 0) return;

        for (int i = installedModules.Count - 1; i >= 0; i--)
        {
            VehicleModule module = installedModules[i];
            if (module.currentHealth > 0)
            {
                module.currentHealth -= damage;

                if (module.currentHealth <= 0)
                {
                    DestroyModule(module);
                    damage = -module.currentHealth;
                }
                else
                {
                    break;
                }
            }
        }

        if (installedModules.Count == 1 && installedModules[0].currentHealth <= 0)
        {
            Die();
        }
    }

    void DestroyModule(VehicleModule module)
    {
        if (module.level == 0) return;

        if (module.moduleObject != null)
        {
            Destroy(module.moduleObject);
        }

        installedModules.Remove(module);
        RecalculateModulePositions();
        UpdateVehicleStats();

        // NUEVO: Ajustar altura después de destruir módulo
        AdjustVehicleHeight();
    }

    void UpdateVehicleStats()
    {
        float totalHealth = 0;
        foreach (VehicleModule module in installedModules)
        {
            totalHealth += module.maxHealth;
        }

        baseMaxHealth = totalHealth;
        currentHealth = Mathf.Min(currentHealth, baseMaxHealth);
    }

    void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = $"Money: ${money}";
    }

    void UpdateUpgradeUI()
    {
        if (moduleInfoText != null)
        {
            string info = $"Módulos: {installedModules.Count - 1}/{maxModules}\n";
            if (installedModules.Count > 1)
            {
                info += $"Módulo Superior: Nivel {installedModules[installedModules.Count - 1].level}";
            }
            moduleInfoText.text = info;
        }

        if (addModuleButton != null)
        {
            addModuleButton.interactable = (installedModules.Count < maxModules + 1) && (money >= GetAddModuleCost());
            addModuleButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Añadir Módulo (${GetAddModuleCost()})";
        }

        if (upgradeModuleButton != null && installedModules.Count > 1)
        {
            VehicleModule topModule = installedModules[installedModules.Count - 1];
            upgradeModuleButton.interactable = (topModule.level < 5) && (money >= topModule.upgradeCost);
            upgradeModuleButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Mejorar a Nivel {topModule.level + 1} (${topModule.upgradeCost})";
        }
    }

    // Sistema de costos
    int GetAddModuleCost()
    {
        return 100 + (installedModules.Count - 1) * 50;
    }

    int GetUpgradeCost(int level)
    {
        return level * 150;
    }

    float GetModuleHealth(int level)
    {
        return level * 50f;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateMoneyUI();
        if (inUpgradePhase) UpdateUpgradeUI();
    }

    void Die()
    {
        isAlive = false;
        EnterUpgradePhase();
    }

    void RespawnVehicle()
    {
        for (int i = installedModules.Count - 1; i > 0; i--)
        {
            if (installedModules[i].moduleObject != null)
                Destroy(installedModules[i].moduleObject);
        }

        VehicleModule chassisModule = installedModules[0];
        installedModules.Clear();
        installedModules.Add(chassisModule);

        chassisModule.currentHealth = chassisModule.maxHealth;
        currentHealth = baseMaxHealth;

        // NUEVO: Resetear altura del contenedor de módulos
        if (moduleContainer != null)
        {
            moduleContainer.localPosition = Vector3.zero;
        }

        isAlive = true;
        transform.position = Vector3.zero;
    }
}