using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CheckPM : MonoBehaviour
{
    [Header("Prefab References")]
    public List<GameObject> aldeanoPrefabs = new List<GameObject>();

    [Header("Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Generation Settings")]
    [Range(1, 5)] public int minAldeanosPorLote = 1;
    [Range(1, 10)] public int maxAldeanosPorLote = 5;
    [Range(1, 50)] public int maxAldeanosConcurrentes = 25;
    [Range(1, 100)] public int maxAldeanosTotalesGenerados = 50;

    [Header("Timing Settings")]
    public float minSpawnInterval = 3f;
    public float maxSpawnInterval = 8f;
    public float checkInterval = 1f;

    [Header("Infection Control")]
    [Range(0f, 1f)] public float baseInfectedChance = 0.15f;
    [Range(1, 10)] public int maxInfectedSpawn = 6;
    [Range(1, 5)] public int minInfectedRequired = 2;

    [Header("Immunity Control")]
    [Range(0f, 1f)] public float immuneChance = 0.30f;

    private List<People> aldeanosActivos = new List<People>();
    private int aldeanosTotalesGenerados = 0;
    private float nextSpawnTime = 0f;

    void Start() //Puntos importantes a llamar
    {
        if (spawnPoints.Count == 0 || aldeanoPrefabs.Count == 0) return;

        SetNextSpawnTime();
        StartCoroutine(GenerationLoop());
        StartCoroutine(UpdateCountersLoop());
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime && CanSpawnAldeano())
            SpawnAldeano();
    }

    IEnumerator GenerationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            CleanupAldeanosList();
        }
    }

    IEnumerator UpdateCountersLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            UpdateCounters();
        }
    }

    bool CanSpawnAldeano() //Quedarnos con esta persion o buscar algo mas simplificada
    {
        return aldeanosActivos.Count < maxAldeanosConcurrentes &&
               aldeanosTotalesGenerados < maxAldeanosTotalesGenerados &&
               spawnPoints.Count > 0 &&
               aldeanoPrefabs.Count > 0;
    }

    void SpawnAldeano()
    {
        int cantidadAGenerar = Random.Range(minAldeanosPorLote, maxAldeanosPorLote + 1);

        for (int i = 0; i < cantidadAGenerar; i++)
        {
            if (!CanSpawnAldeano()) break;

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            if (IsSpawnPointOccupied(spawnPoint)) continue;

            GameObject selectedPrefab = aldeanoPrefabs[Random.Range(0, aldeanoPrefabs.Count)];
            GameObject newAldeano = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
            People peopleScript = newAldeano.GetComponent<People>();

            if (peopleScript != null)
            {
                bool shouldBeInfected = DetermineInfectionStatus();
                if (shouldBeInfected)
                {
                    peopleScript.SetImmunity(false);
                    StartCoroutine(MakeInfectedAfterDelay(peopleScript, Random.Range(0.5f, 2f)));
                }
                else
                {
                    peopleScript.SetImmunity(Random.value <= immuneChance);
                }

                aldeanosActivos.Add(peopleScript);
            }
            aldeanosTotalesGenerados++;
        }
        SetNextSpawnTime();
    }
    //Buscar algo mas logico y simple, no me convence de toda la logica
    bool DetermineInfectionStatus()
    {
        int infectadosActuales = GetInfectadosActuales();
        if (infectadosActuales >= maxInfectedSpawn) return false;
        return Random.value <= (infectadosActuales < minInfectedRequired ? baseInfectedChance * 2f : baseInfectedChance);
    }

    IEnumerator MakeInfectedAfterDelay(People person, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (person != null)
        {
            person.infectionProgress = 100f;
            person.StartDirectInfection();
        }
    }

    bool IsSpawnPointOccupied(Transform spawnPoint)  //Verifiquemos que no se anden generando mismo sitio
    {
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(spawnPoint.position, 1.5f);
        foreach (var obj in nearbyObjects)
            if (obj.CompareTag("Aldeano") || obj.CompareTag("Infected"))
                return true;
        return false;
    }

    void SetNextSpawnTime() => nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);

    void CleanupAldeanosList() => aldeanosActivos.RemoveAll(aldeano => aldeano == null);

    void UpdateCounters() => CleanupAldeanosList();

    public void RemoveAldeano(People aldeano)  //Seguir chequeando esta parte del codigo causa algo de problemas
    {
        if (aldeanosActivos.Contains(aldeano))
        {
            aldeanosActivos.Remove(aldeano);
            if (aldeano != null) Destroy(aldeano.gameObject);
        }
    }

    [ContextMenu("Force Spawn Infected")]
    public void ForceSpawnInfected()
    {
        if (!CanSpawnAldeano()) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        GameObject selectedPrefab = aldeanoPrefabs[Random.Range(0, aldeanoPrefabs.Count)];
        GameObject newAldeano = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
        People peopleScript = newAldeano.GetComponent<People>();

        if (peopleScript != null)
        {
            peopleScript.SetImmunity(false);
            StartCoroutine(MakeInfectedAfterDelay(peopleScript, 0.1f));
            aldeanosActivos.Add(peopleScript);
        }

        aldeanosTotalesGenerados++;
    }

    [ContextMenu("Force Spawn Immune")]
    public void ForceSpawnImmune()
    {
        if (!CanSpawnAldeano()) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        GameObject selectedPrefab = aldeanoPrefabs[Random.Range(0, aldeanoPrefabs.Count)];
        GameObject newAldeano = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
        People peopleScript = newAldeano.GetComponent<People>();

        if (peopleScript != null)
        {
            peopleScript.SetImmunity(true);
            aldeanosActivos.Add(peopleScript);
        }

        aldeanosTotalesGenerados++;
    }
    //Maneja aun mas la logica
    public int GetAldeanosConcurrentes() => aldeanosActivos.Count;
    public int GetAldeanosTotalesGenerados() => aldeanosTotalesGenerados;
    public bool HasReachedMaxGeneration() => aldeanosTotalesGenerados >= maxAldeanosTotalesGenerados;

    public int GetInfectadosActuales()
    {
        int count = 0;
        foreach (People aldeano in aldeanosActivos)
            if (aldeano != null && (aldeano.currentState == People.PersonState.Infected ||
                                   aldeano.currentState == People.PersonState.Chasing ||
                                   aldeano.infectionProgress >= 75f))
                count++;
        return count;
    }

    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(spawnPoint.position, 1.5f);
            }
        }
    }
}