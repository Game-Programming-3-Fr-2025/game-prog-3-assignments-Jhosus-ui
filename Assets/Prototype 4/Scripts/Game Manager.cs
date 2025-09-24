using UnityEngine;
using UnityEngine.UI;
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

    void Start()
    {
        if (!ValidateReferences()) return;

        CalculateGridPositions();
        addModuleButton.onClick.AddListener(AddModule);

        if (chasis.transform.parent != gridParent)
        {
            chasis.transform.SetParent(gridParent);
        }

        PositionObjectInGrid(chasis, 0);
        Debug.Log($"GameManager inicializado. Módulos actuales: {currentModuleCount}");
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
        if (currentModuleCount >= maxModules)
        {
            Debug.Log("Máximo de módulos alcanzado: " + maxModules);
            return;
        }

        GameObject newModule = Instantiate(modulePrefab, gridParent);
        if (newModule == null) return;

        ShiftExistingModulesUp();
        PositionObjectInGrid(newModule, 0);
        currentModuleCount++;

        Debug.Log($"Módulo agregado. Total: {currentModuleCount}/{maxModules}");
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

        // Encontrar índice del módulo a eliminar
        int indiceEliminar = -1;
        for (int i = 0; i < gridParent.childCount; i++)
        {
            if (gridParent.GetChild(i).gameObject == moduloEliminar)
            {
                indiceEliminar = i;
                break;
            }
        }

        if (indiceEliminar == -1) return;

        // Eliminar módulo
        Destroy(moduloEliminar);
        currentModuleCount--;

        // Bajar módulos superiores
        for (int i = indiceEliminar; i < gridParent.childCount; i++)
        {
            Transform modulo = gridParent.GetChild(i);
            int nuevoIndice = GetCellIndexFromYPosition(modulo.localPosition.y) - 1;
            if (nuevoIndice >= 0)
            {
                PositionObjectInGrid(modulo.gameObject, nuevoIndice);
            }
        }

        Debug.Log($"Módulo eliminado. Total: {currentModuleCount}/{maxModules}");
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
}