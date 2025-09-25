using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public Transform gridParent;
    public Vector2 gridCellSize = new Vector2(2, 2);
    public int maxModules = 5;

    [Header("References")]
    public GameObject chasis;
    public GameObject modulePrefab;

    [Header("UI")]
    public Button addModuleButton;

    private int currentModuleCount = 1;
    private float[] gridYPositions;

    [Header("Money")]
    public TextMeshProUGUI addModuleCostText;
    public int moduleBaseCost = 50;
    public int moduleCostIncrement = 15;
    private int currentModuleCost { get { return moduleBaseCost + ((currentModuleCount - 1) * moduleCostIncrement); } }

    // NUEVO: Registrar módulos iniciales para regeneración
    private int modulosIniciales = 1; // El chasis cuenta como 1

    void Start()
    {
        // Registrar para el evento de reset al lobby
        MoneyManager.Instance.onResetToLobby += RegenerarModulosPerdidos;

        addModuleButton.gameObject.SetActive(!MoneyManager.Instance.isPlaying);

        if (!ValidateReferences()) return;

        CalculateGridPositions();
        addModuleButton.onClick.AddListener(AddModule);

        if (chasis.transform.parent != gridParent)
        {
            chasis.transform.SetParent(gridParent);
        }

        PositionObjectInGrid(chasis, 0);

        // NUEVO: Guardar cantidad inicial de módulos
        modulosIniciales = currentModuleCount;

        Debug.Log($"GameManager inicializado. Módulos actuales: {currentModuleCount}, Iniciales: {modulosIniciales}");
    }

    private void OnDestroy()
    {
        // Desregistrar evento para evitar memory leaks
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.onResetToLobby -= RegenerarModulosPerdidos;
        }
    }

    void Update()
    {
        addModuleButton.gameObject.SetActive(!MoneyManager.Instance.isPlaying);
        if (addModuleCostText) addModuleCostText.text = currentModuleCost.ToString();
        addModuleCostText.gameObject.SetActive(!MoneyManager.Instance.isPlaying);
    }

    // NUEVO MÉTODO: Regenera los módulos perdidos cuando volvemos al lobby
    private void RegenerarModulosPerdidos()
    {
        Debug.Log($"Regenerando módulos perdidos. Teníamos {modulosIniciales}, tenemos {currentModuleCount}");

        // Si hemos perdido módulos, regenerarlos
        if (currentModuleCount < modulosIniciales)
        {
            int modulosARegenerar = modulosIniciales - currentModuleCount;
            Debug.Log($"Regenerando {modulosARegenerar} módulos...");

            for (int i = 0; i < modulosARegenerar; i++)
            {
                RegenerarUnModulo();
            }
        }

        // Reposicionar todos los módulos en el grid
        ReposicionarTodosLosModulos();
    }

    // NUEVO MÉTODO: Regenera un módulo individual
    private void RegenerarUnModulo()
    {
        if (currentModuleCount >= maxModules) return;

        GameObject newModule = Instantiate(modulePrefab, gridParent);
        if (newModule == null) return;

        // Posicionar el nuevo módulo correctamente
        ShiftExistingModulesUp();
        PositionObjectInGrid(newModule, 0);
        currentModuleCount++;

        Debug.Log($"Módulo regenerado. Total: {currentModuleCount}/{maxModules}");
    }

    // NUEVO MÉTODO: Reposiciona todos los módulos en el grid
    private void ReposicionarTodosLosModulos()
    {
        var modulosActuales = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < gridParent.childCount; i++)
        {
            modulosActuales.Add(gridParent.GetChild(i));
        }

        // Ordenar por posición Y (más bajo primero)
        modulosActuales.Sort((a, b) => a.localPosition.y.CompareTo(b.localPosition.y));

        // Reposicionar desde la celda 0
        for (int i = 0; i < modulosActuales.Count; i++)
        {
            if (modulosActuales[i] != null)
            {
                PositionObjectInGrid(modulosActuales[i].gameObject, i);
            }
        }

        Debug.Log($"Todos los módulos reposicionados. Total: {modulosActuales.Count}");
    }

    private bool ValidateReferences()
    {
        if (gridParent == null || chasis == null || modulePrefab == null || addModuleButton == null)
        {
            Debug.LogError("GameManager: Referencias faltantes");
            return false;
        }
        return true;
    }

    private void CalculateGridPositions()
    {
        gridYPositions = new float[maxModules];
        for (int i = 0; i < maxModules; i++)
        {
            gridYPositions[i] = i * gridCellSize.y;
        }
    }

    public void AddModule()
    {
        if (currentModuleCount >= maxModules) return;

        int cost = currentModuleCost;
        if (MoneyManager.Instance.SpendMoney(cost))
        {
            GameObject newModule = Instantiate(modulePrefab, gridParent);
            if (newModule == null) return;

            ShiftExistingModulesUp();
            PositionObjectInGrid(newModule, 0);
            currentModuleCount++;

            // NUEVO: Actualizar módulos iniciales si estamos comprando en el lobby
            if (!MoneyManager.Instance.isPlaying)
            {
                modulosIniciales = currentModuleCount;
            }

            Debug.Log($"Módulo agregado. Total: {currentModuleCount}/{maxModules}, Iniciales: {modulosIniciales}");
        }
    }

    private void PositionObjectInGrid(GameObject obj, int cellIndex)
    {
        if (obj == null || cellIndex < 0 || cellIndex >= maxModules) return;
        obj.transform.localPosition = new Vector3(0, gridYPositions[cellIndex], 0);
    }

    private void ShiftExistingModulesUp()
    {
        var objectsToMove = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < gridParent.childCount; i++)
        {
            objectsToMove.Add(gridParent.GetChild(i));
        }

        foreach (Transform child in objectsToMove)
        {
            if (child == null) continue;
            int currentCellIndex = GetCellIndexFromYPosition(child.localPosition.y);
            if (currentCellIndex < maxModules - 1)
            {
                PositionObjectInGrid(child.gameObject, currentCellIndex + 1);
            }
        }
    }

    private int GetCellIndexFromYPosition(float yPos)
    {
        for (int i = 0; i < gridYPositions.Length; i++)
        {
            if (Mathf.Approximately(yPos, gridYPositions[i]))
                return i;
        }
        return 0;
    }

    public void EliminarModulo(GameObject moduloEliminar)
    {
        if (moduloEliminar == null) return;

        Debug.Log($"Eliminando módulo: {moduloEliminar.name}");

        var modulosAntesDeEliminar = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < gridParent.childCount; i++)
        {
            Transform child = gridParent.GetChild(i);
            if (child.gameObject != moduloEliminar)
            {
                modulosAntesDeEliminar.Add(child);
            }
        }

        currentModuleCount--;
        Destroy(moduloEliminar);

        StartCoroutine(ReposicionarModulosCorrutina(modulosAntesDeEliminar));
    }

    private System.Collections.IEnumerator ReposicionarModulosCorrutina(System.Collections.Generic.List<Transform> modulos)
    {
        yield return null;

        modulos.Sort((a, b) => a.localPosition.y.CompareTo(b.localPosition.y));

        for (int i = 0; i < modulos.Count; i++)
        {
            if (modulos[i] != null)
            {
                PositionObjectInGrid(modulos[i].gameObject, i);
            }
        }

        Debug.Log($"Reposicionamiento completo. Módulos totales: {currentModuleCount}");
    }

    private void OnDrawGizmos()
    {
        if (gridParent == null) return;

        if (gridYPositions == null || gridYPositions.Length != maxModules)
        {
            CalculateGridPositionsForGizmos();
        }

        Gizmos.color = Color.cyan;
        Vector3 parentPos = gridParent.position;

        for (int i = 0; i < maxModules; i++)
        {
            if (i < gridYPositions.Length)
            {
                Vector3 cellCenter = parentPos + new Vector3(0, gridYPositions[i], 0);
                Gizmos.DrawWireCube(cellCenter, new Vector3(gridCellSize.x, gridCellSize.y, 0.1f));

#if UNITY_EDITOR
                Handles.Label(cellCenter + Vector3.left * (gridCellSize.x * 0.6f),
                             $"Celda {i + 1}\nY: {gridYPositions[i]:F1}");
#endif
            }
        }
    }

    private void CalculateGridPositionsForGizmos()
    {
        gridYPositions = new float[maxModules];
        for (int i = 0; i < maxModules; i++)
        {
            gridYPositions[i] = i * gridCellSize.y;
        }
    }

    [ContextMenu("Test Reposicionar Módulos")]
    public void TestReposicionarModulos()
    {
        var modulos = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < gridParent.childCount; i++)
        {
            modulos.Add(gridParent.GetChild(i));
        }
        StartCoroutine(ReposicionarModulosCorrutina(modulos));
    }
}