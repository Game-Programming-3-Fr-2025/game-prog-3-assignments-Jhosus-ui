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
    public TextMeshProUGUI addModuleCostText; // Texto costo
    public int moduleBaseCost = 50;
    public int moduleCostIncrement = 15;
    private int currentModuleCost { get { return moduleBaseCost + ((currentModuleCount - 1) * moduleCostIncrement); } }
    void Start()
    {
        addModuleButton.gameObject.SetActive(!MoneyManager.Instance.isPlaying);

        if (!ValidateReferences()) return;

        CalculateGridPositions();
        addModuleButton.onClick.AddListener(AddModule);

        if (chasis.transform.parent != gridParent)
        {
            chasis.transform.SetParent(gridParent);
        }

        PositionObjectInGrid(chasis, 0);
        Debug.Log($"GameManager inicializado. M�dulos actuales: {currentModuleCount}");
    }

    void Update()
    {
        addModuleButton.gameObject.SetActive(!MoneyManager.Instance.isPlaying);
        if (addModuleCostText) addModuleCostText.text = currentModuleCost.ToString();
        addModuleCostText.gameObject.SetActive(!MoneyManager.Instance.isPlaying);
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

            Debug.Log($"M�dulo agregado. Total: {currentModuleCount}/{maxModules}");

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

    // PROBLEMA 2 SOLUCIONADO: M�todo de eliminaci�n completamente reescrito
    public void EliminarModulo(GameObject moduloEliminar)
    {
        if (moduloEliminar == null) return;

        Debug.Log($"Eliminando m�dulo: {moduloEliminar.name}");

        // Obtener lista de todos los m�dulos ANTES de eliminar
        var modulosAntesDeEliminar = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < gridParent.childCount; i++)
        {
            Transform child = gridParent.GetChild(i);
            if (child.gameObject != moduloEliminar) // Excluir el que vamos a eliminar
            {
                modulosAntesDeEliminar.Add(child);
            }
        }

        // Eliminar el m�dulo
        currentModuleCount--;
        Destroy(moduloEliminar);

        // Esperar un frame para que se complete la destrucci�n
        StartCoroutine(ReposicionarModulosCorrutina(modulosAntesDeEliminar));
    }

    private System.Collections.IEnumerator ReposicionarModulosCorrutina(System.Collections.Generic.List<Transform> modulos)
    {
        yield return null; // Esperar un frame

        // Ordenar m�dulos por posici�n Y (de abajo hacia arriba)
        modulos.Sort((a, b) => a.localPosition.y.CompareTo(b.localPosition.y));

        // Reposicionar todos desde la celda 0
        for (int i = 0; i < modulos.Count; i++)
        {
            if (modulos[i] != null)
            {
                PositionObjectInGrid(modulos[i].gameObject, i);
                Debug.Log($"Reposicionado {modulos[i].name} a celda {i}");
            }
        }

        Debug.Log($"Reposicionamiento completo. M�dulos totales: {currentModuleCount}");
    }

    private void OnDrawGizmos()
    {
        if (gridParent == null) return;
        if (gridYPositions == null) CalculateGridPositions();

        Gizmos.color = Color.cyan;
        Vector3 parentPos = gridParent.position;

        for (int i = 0; i < maxModules; i++)
        {
            Vector3 cellCenter = parentPos + new Vector3(0, gridYPositions[i], 0);
            Gizmos.DrawWireCube(cellCenter, new Vector3(gridCellSize.x, gridCellSize.y, 0.1f));

#if UNITY_EDITOR
            Handles.Label(cellCenter + Vector3.left * (gridCellSize.x * 0.6f),
                         $"Celda {i + 1}\nY: {gridYPositions[i]:F1}");
#endif
        }
    }

    [ContextMenu("Test Reposicionar M�dulos")]
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