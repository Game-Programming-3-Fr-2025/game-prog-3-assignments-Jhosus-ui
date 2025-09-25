using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Guns : MonoBehaviour
{
    [System.Serializable]
    public class ModuleSlot
    {
        public Transform assignedModule;
        public Transform slotPoint;
        public GameObject weaponButtonsUI;
        public GameObject weaponInstance;
        public bool hasWeapon = false;
    }

    [Header("UI Setup")]
    public Canvas uiCanvas;
    public GameObject weaponButtonsPrefab;

    [Header("Weapon Prefabs")]
    public GameObject weapon1Prefab;
    public GameObject weapon2Prefab;
    public GameObject weapon3Prefab;

    [Header("Weapon Money")]
    public int weapon1Cost = 20;
    public int weapon2Cost = 30;
    public int weapon3Cost = 40;

    [Header("Settings")]
    public Vector2 buttonOffset = new Vector2(0, 50);

    private GameManager gameManager;
    public List<ModuleSlot> moduleSlots = new List<ModuleSlot>();
    private int minCost; // Nuevo: Costo mínimo de las armas

    void Start()
    {
        // Calcular minCost al inicio
        minCost = Mathf.Min(weapon1Cost, weapon2Cost, weapon3Cost);

        if (!ValidateReferences()) return;

        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("No se encontró GameManager");
            return;
        }

        InvokeRepeating(nameof(CheckForModules), 0.1f, 0.3f);
        Debug.Log("Guns system inicializado");
    }

    private bool ValidateReferences()
    {
        if (uiCanvas == null || weaponButtonsPrefab == null)
        {
            Debug.LogError("Referencias UI faltantes en Guns");
            return false;
        }
        return true;
    }

    void Update()
    {
        UpdateButtonPositions();
        HandleRightClick();
    }

    private void CheckForModules()
    {
        if (gameManager?.gridParent == null) return;

        for (int i = 0; i < gameManager.gridParent.childCount; i++)
        {
            Transform module = gameManager.gridParent.GetChild(i);

            // VERIFICACIÓN DEL CHASIS - No crear slot si tiene tag "Chasis"
            if (module.CompareTag("Chasis"))
            {
                continue; // Saltar este módulo
            }

            if (FindSlotForModule(module) == null)
            {
                CreateSlotForModule(module);
            }
        }

        CleanupOrphanedSlots();
    }

    private void CreateSlotForModule(Transform module)
    {
        ModuleSlot newSlot = new ModuleSlot();
        newSlot.assignedModule = module;

        // Crear punto en el centro del módulo
        GameObject slotPoint = new GameObject($"SlotPoint_{module.name}");
        slotPoint.transform.SetParent(module);
        slotPoint.transform.localPosition = Vector3.zero;
        newSlot.slotPoint = slotPoint.transform;

        // Crear UI de botones
        CreateButtonsUIForSlot(newSlot);
        moduleSlots.Add(newSlot);

        Debug.Log($"Slot creado para módulo: {module.name}");
    }

    private void CreateButtonsUIForSlot(ModuleSlot slot)
    {
        GameObject buttonsPanel = Instantiate(weaponButtonsPrefab, uiCanvas.transform);
        buttonsPanel.name = $"WeaponButtons_{slot.assignedModule.name}";
        slot.weaponButtonsUI = buttonsPanel;

        Button[] buttons = buttonsPanel.GetComponentsInChildren<Button>();
        if (buttons.Length >= 3)
        {
            buttons[0].onClick.AddListener(() => EquipWeapon(slot, weapon1Prefab, weapon1Cost));
            buttons[1].onClick.AddListener(() => EquipWeapon(slot, weapon2Prefab, weapon2Cost));
            buttons[2].onClick.AddListener(() => EquipWeapon(slot, weapon3Prefab, weapon3Cost));
        }

        TextMeshProUGUI[] costTexts = buttonsPanel.GetComponentsInChildren<TextMeshProUGUI>();
        if (costTexts.Length >= 3)
        {
            costTexts[0].text = weapon1Cost.ToString();
            costTexts[1].text = weapon2Cost.ToString();
            costTexts[2].text = weapon3Cost.ToString();
        }
    }

    private void UpdateButtonPositions()
    {
        foreach (ModuleSlot slot in moduleSlots)
        {
            if (slot.weaponButtonsUI == null || slot.slotPoint == null) continue;

            Vector2 screenPosition = Camera.main.WorldToScreenPoint(slot.slotPoint.position);
            screenPosition += buttonOffset;

            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                uiCanvas.transform as RectTransform,
                screenPosition,
                uiCanvas.worldCamera,
                out localPosition
            );

            slot.weaponButtonsUI.GetComponent<RectTransform>().localPosition = localPosition;

            // Mostrar botones solo si no tiene arma, en lobby, y dinero suficiente
            slot.weaponButtonsUI.SetActive(!slot.hasWeapon && !MoneyManager.Instance.isPlaying && MoneyManager.Instance.money >= minCost);
        }
    }

    private void EquipWeapon(ModuleSlot slot, GameObject weaponPrefab, int cost)
    {
        if (slot.hasWeapon || weaponPrefab == null) return;

        if (MoneyManager.Instance.SpendMoney(cost))
        {
            GameObject newWeapon = Instantiate(weaponPrefab, slot.slotPoint);
            newWeapon.transform.localPosition = Vector3.zero;

            slot.hasWeapon = true;
            slot.weaponInstance = newWeapon;

            Debug.Log($"Arma {weaponPrefab.name} equipada en {slot.assignedModule.name} por {cost}");
        }
        else
        {
            Debug.Log($"No hay suficiente dinero para equipar {weaponPrefab.name} (Costo: {cost})");
        }
    }

    public void RemoveWeapon(ModuleSlot slot)
    {
        if (slot?.hasWeapon != true) return;

        Destroy(slot.weaponInstance);
        slot.weaponInstance = null;
        slot.hasWeapon = false;
    }

    private ModuleSlot FindSlotForModule(Transform module)
    {
        foreach (ModuleSlot slot in moduleSlots)
        {
            if (slot.assignedModule == module)
                return slot;
        }
        return null;
    }

    private void CleanupOrphanedSlots()
    {
        for (int i = moduleSlots.Count - 1; i >= 0; i--)
        {
            ModuleSlot slot = moduleSlots[i];

            if (slot.assignedModule == null || slot.assignedModule.parent != gameManager.gridParent)
            {
                if (slot.slotPoint != null) Destroy(slot.slotPoint.gameObject);
                if (slot.weaponButtonsUI != null) Destroy(slot.weaponButtonsUI);
                if (slot.weaponInstance != null) Destroy(slot.weaponInstance);

                moduleSlots.RemoveAt(i);
            }
        }
    }

    private void HandleRightClick()
    {
        if (!Input.GetMouseButtonDown(1)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            foreach (ModuleSlot slot in moduleSlots)
            {
                if (slot.hasWeapon && slot.weaponInstance != null)
                {
                    if (hit.collider.transform.IsChildOf(slot.weaponInstance.transform))
                    {
                        RemoveWeapon(slot);
                        break;
                    }
                }
            }
        }
    }

    [ContextMenu("Log All Slots")]
    public void LogAllSlots()
    {
        Debug.Log("=== SLOTS DE MÓDULOS ===");
        foreach (ModuleSlot slot in moduleSlots)
        {
            string weaponName = slot.hasWeapon ? slot.weaponInstance.name : "Sin arma";
            Debug.Log($"Módulo: {slot.assignedModule.name} - Arma: {weaponName}");
        }
    }

    void OnDestroy()
    {
        CancelInvoke();
    }
}