using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TPI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI normalCountText;
    public TextMeshProUGUI intermittentCoughCountText;
    public TextMeshProUGUI infectedCountText;
    public TextMeshProUGUI chasingCountText;
    public TextMeshProUGUI totalCountText;

    [Header("Update Settings")]
    public float updateInterval = 0.5f; // Actualizar cada 0.5 segundos

    private float updateTimer = 0f;

    void Start()
    {
        // Inicializar textos si están asignados
        UpdateCounters();
    }

    void Update()
    {
        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            UpdateCounters();
            updateTimer = 0f;
        }
    }

    void UpdateCounters()
    {
        // Encontrar todos los objetos con componente People
        People[] allPeople = FindObjectsOfType<People>();

        int normalCount = 0;
        int intermittentCoughCount = 0;
        int infectedCount = 0;
        int chasingCount = 0;
        int coughingCount = 0;

        // Contar cada estado
        foreach (People person in allPeople)
        {
            switch (person.currentState)
            {
                case People.PersonState.Normal:
                    normalCount++;
                    break;
                case People.PersonState.IntermittentCough:
                    intermittentCoughCount++;
                    break;
                case People.PersonState.Coughing:
                    coughingCount++;
                    break;
                case People.PersonState.Infected:
                    infectedCount++;
                    break;
                case People.PersonState.Chasing:
                    chasingCount++;
                    break;
            }
        }

        // Actualizar textos UI
        if (normalCountText != null)
            normalCountText.text = $"Sanos: {normalCount}";

        if (intermittentCoughCountText != null)
            intermittentCoughCountText.text = $"Con Síntomas: {intermittentCoughCount + coughingCount}";

        if (infectedCountText != null)
            infectedCountText.text = $"Infectados: {infectedCount}";

        if (chasingCountText != null)
            chasingCountText.text = $"Persiguiendo: {chasingCount}";

        if (totalCountText != null)
            totalCountText.text = $"Total: {allPeople.Length}";
    }

    // Método público para obtener estadísticas (opcional)
    public void GetStats(out int normal, out int symptoms, out int infected, out int chasing, out int total)
    {
        People[] allPeople = FindObjectsOfType<People>();

        normal = 0;
        symptoms = 0;
        infected = 0;
        chasing = 0;

        foreach (People person in allPeople)
        {
            switch (person.currentState)
            {
                case People.PersonState.Normal:
                    normal++;
                    break;
                case People.PersonState.IntermittentCough:
                case People.PersonState.Coughing:
                    symptoms++;
                    break;
                case People.PersonState.Infected:
                    infected++;
                    break;
                case People.PersonState.Chasing:
                    chasing++;
                    break;
            }
        }

        total = allPeople.Length;
    }

    // Método para mostrar porcentajes (opcional)
    public void UpdatePercentages()
    {
        GetStats(out int normal, out int symptoms, out int infected, out int chasing, out int total);

        if (total == 0) return;

        float normalPercent = (normal / (float)total) * 100f;
        float symptomsPercent = (symptoms / (float)total) * 100f;
        float infectedPercent = (infected / (float)total) * 100f;

        if (normalCountText != null)
            normalCountText.text = $"Sanos: {normal} ({normalPercent:F1}%)";

        if (intermittentCoughCountText != null)
            intermittentCoughCountText.text = $"Síntomas: {symptoms} ({symptomsPercent:F1}%)";

        if (infectedCountText != null)
            infectedCountText.text = $"Infectados: {infected} ({infectedPercent:F1}%)";

        if (chasingCountText != null)
            chasingCountText.text = $"Persiguiendo: {chasing}";

        if (totalCountText != null)
            totalCountText.text = $"Total: {total}";
    }
}