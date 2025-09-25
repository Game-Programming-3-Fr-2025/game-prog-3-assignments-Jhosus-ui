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
    private int currentModuleCost => moduleBaseCost + ((currentModuleCount - 1) * moduleCostIncrement);

    private int modulosIniciales = 1;

    void Start()
    {
        MoneyManager.Instance.onResetToLobby += RegenerarModulosPerdidos;
        addModuleButton.gameObject.SetActive(!MoneyManager.Instance.isPlaying);
        if (!ValidateReferences()) return;
        CalculateGridPositions();
        addModuleButton.onClick.AddListener(AddModule);
        if (chasis.transform.parent != gridParent) chasis.transform.SetParent(gridParent);
        PositionObjectInGrid(chasis, 0);
        modulosIniciales = currentModuleCount;
    }

    private void OnDestroy()
    {
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.onResetToLobby -= RegenerarModulosPerdidos;
    }

    void Update()
    {
        bool show = !MoneyManager.Instance.isPlaying;
        addModuleButton.gameObject.SetActive(show);
        if (addModuleCostText)
        {
            addModuleCostText.text = currentModuleCost.ToString();
            addModuleCostText.gameObject.SetActive(show);
        }
    }

    private void RegenerarModulosPerdidos()
    {
        if (currentModuleCount < modulosIniciales)
            for (int i = 0, n = modulosIniciales - currentModuleCount; i < n; i++) RegenerarUnModulo();
        ReposicionarTodosLosModulos();
    }

    private void RegenerarUnModulo()
    {
        if (currentModuleCount >= maxModules) return;
        var newModule = Instantiate(modulePrefab, gridParent);
        if (newModule == null) return;
        ShiftExistingModulesUp();
        PositionObjectInGrid(newModule, 0);
        currentModuleCount++;
    }

    private void ReposicionarTodosLosModulos()
    {
        var modulos = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < gridParent.childCount; i++) modulos.Add(gridParent.GetChild(i));
        modulos.Sort((a, b) => a.localPosition.y.CompareTo(b.localPosition.y));
        for (int i = 0; i < modulos.Count; i++)
            if (modulos[i] != null) PositionObjectInGrid(modulos[i].gameObject, i);
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
        for (int i = 0; i < maxModules; i++) gridYPositions[i] = i * gridCellSize.y;
    }

    public void AddModule()
    {
        if (currentModuleCount >= maxModules) return;
        if (MoneyManager.Instance.SpendMoney(currentModuleCost))
        {
            var newModule = Instantiate(modulePrefab, gridParent);
            if (newModule == null) return;
            ShiftExistingModulesUp();
            PositionObjectInGrid(newModule, 0);
            currentModuleCount++;
            if (!MoneyManager.Instance.isPlaying) modulosIniciales = currentModuleCount;
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
        for (int i = 0; i < gridParent.childCount; i++) objectsToMove.Add(gridParent.GetChild(i));
        foreach (var child in objectsToMove)
        {
            if (child == null) continue;
            int idx = GetCellIndexFromYPosition(child.localPosition.y);
            if (idx < maxModules - 1) PositionObjectInGrid(child.gameObject, idx + 1);
        }
    }

    private int GetCellIndexFromYPosition(float yPos)
    {
        for (int i = 0; i < gridYPositions.Length; i++)
            if (Mathf.Approximately(yPos, gridYPositions[i])) return i;
        return 0;
    }

    public void EliminarModulo(GameObject moduloEliminar)
    {
        if (moduloEliminar == null) return;
        var modulos = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < gridParent.childCount; i++)
        {
            var child = gridParent.GetChild(i);
            if (child.gameObject != moduloEliminar) modulos.Add(child);
        }
        currentModuleCount--;
        Destroy(moduloEliminar);
        StartCoroutine(ReposicionarModulosCorrutina(modulos));
    }

    private System.Collections.IEnumerator ReposicionarModulosCorrutina(System.Collections.Generic.List<Transform> modulos)
    {
        yield return null;
        modulos.Sort((a, b) => a.localPosition.y.CompareTo(b.localPosition.y));
        for (int i = 0; i < modulos.Count; i++)
            if (modulos[i] != null) PositionObjectInGrid(modulos[i].gameObject, i);
    }

    private void OnDrawGizmos()
    {
        if (gridParent == null) return;
        if (gridYPositions == null || gridYPositions.Length != maxModules) CalculateGridPositionsForGizmos();
        Gizmos.color = Color.cyan;
        Vector3 parentPos = gridParent.position;
        for (int i = 0; i < maxModules; i++)
        {
            if (i < gridYPositions.Length)
            {
                Vector3 cellCenter = parentPos + new Vector3(0, gridYPositions[i], 0);
                Gizmos.DrawWireCube(cellCenter, new Vector3(gridCellSize.x, gridCellSize.y, 0.1f));
#if UNITY_EDITOR
                Handles.Label(cellCenter + Vector3.left * (gridCellSize.x * 0.6f), $"Celda {i + 1}\nY: {gridYPositions[i]:F1}");
#endif
            }
        }
    }

    private void CalculateGridPositionsForGizmos()
    {
        gridYPositions = new float[maxModules];
        for (int i = 0; i < maxModules; i++) gridYPositions[i] = i * gridCellSize.y;
    }

    [ContextMenu("Test Reposicionar Módulos")]
    public void TestReposicionarModulos()
    {
        var modulos = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < gridParent.childCount; i++) modulos.Add(gridParent.GetChild(i));
        StartCoroutine(ReposicionarModulosCorrutina(modulos));
    }
}   