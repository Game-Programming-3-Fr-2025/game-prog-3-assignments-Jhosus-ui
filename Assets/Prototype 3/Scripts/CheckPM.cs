using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CheckPM : MonoBehaviour
{
    [Header("Prefab References")]
    public List<GameObject> aldeanoPrefabs = new List<GameObject>();

    [Header("Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Generation Settings")]
    [Range(1, 50)]
    public int maxAldeanosConcurrentes = 25;
    [Range(1, 100)]
    public int maxAldeanosTotalesGenerados = 50;

    [Header("Timing Settings")]
    public float minSpawnInterval = 3f;
    public float maxSpawnInterval = 8f;
    public float checkInterval = 1f;

    [Header("Infection Control")]
    [Range(0f, 1f)]
    public float baseInfectedChance = 0.15f;
    [Range(1, 10)]
    public int maxInfectedSpawn = 6;
    [Range(1, 5)]
    public int minInfectedRequired = 2;

    [Header("Immunity Control")]
    [Range(0f, 1f)]
    public float immuneChance = 0.30f;
    [Range(0f, 1f)]
    public float susceptibleChance = 0.70f;

    [Header("Debug Info")]
    [SerializeField] private int aldeanosConcurrentes = 0;
    [SerializeField] private int aldeanosTotalesGenerados = 0;
    [SerializeField] private int infectadosActuales = 0;
    [SerializeField] private float nextSpawnTime = 0f;

    private List<People> aldeanosActivos = new List<People>();

    void Start()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("No hay puntos de spawn asignados!");
            return;
        }

        if (aldeanoPrefabs.Count == 0)
        {
            Debug.LogWarning("No hay prefabs de aldeanos asignados!");
            return;
        }

        SetNextSpawnTime();
        StartCoroutine(GenerationLoop());
        StartCoroutine(UpdateCountersLoop());
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime && CanSpawnAldeano())
        {
            SpawnAldeano();
        }
    }

    IEnumerator GenerationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            CleanupAldeanosList();
            UpdateCounters();
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

    bool CanSpawnAldeano()
    {
        if (aldeanosConcurrentes >= maxAldeanosConcurrentes) return false;
        if (aldeanosTotalesGenerados >= maxAldeanosTotalesGenerados) return false;
        if (spawnPoints.Count == 0) return false;
        if (aldeanoPrefabs.Count == 0) return false;
        return true;
    }

    void SpawnAldeano()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        if (IsSpawnPointOccupied(spawnPoint))
        {
            SetNextSpawnTime();
            return;
        }

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
                bool shouldBeImmune = Random.value <= immuneChance;
                peopleScript.SetImmunity(shouldBeImmune);
            }

            aldeanosActivos.Add(peopleScript);
        }

        aldeanosConcurrentes++;
        aldeanosTotalesGenerados++;
        SetNextSpawnTime();
    }

    bool DetermineInfectionStatus()
    {
        if (infectadosActuales >= maxInfectedSpawn)
        {
            return false;
        }

        if (infectadosActuales < minInfectedRequired)
        {
            return Random.value <= (baseInfectedChance * 2f);
        }

        return Random.value <= baseInfectedChance;
    }

    IEnumerator MakeInfectedAfterDelay(People person, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (person != null && person.gameObject != null)
        {
            person.infectionProgress = 100f;
            person.StartDirectInfection();
        }
    }

    bool IsSpawnPointOccupied(Transform spawnPoint)
    {
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(spawnPoint.position, 1.5f);

        foreach (var obj in nearbyObjects)
        {
            if (obj.CompareTag("Aldeano") || obj.CompareTag("Infected"))
            {
                return true;
            }
        }

        return false;
    }

    void SetNextSpawnTime()
    {
        float randomInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        nextSpawnTime = Time.time + randomInterval;
    }

    void CleanupAldeanosList()
    {
        aldeanosActivos.RemoveAll(aldeano => aldeano == null || aldeano.gameObject == null);
        aldeanosConcurrentes = aldeanosActivos.Count;
    }

    void UpdateCounters()
    {
        CleanupAldeanosList();
        infectadosActuales = 0;

        foreach (People aldeano in aldeanosActivos)
        {
            if (aldeano != null &&
                (aldeano.currentState == People.PersonState.Infected ||
                 aldeano.currentState == People.PersonState.Chasing ||
                 aldeano.infectionProgress >= 75f))
            {
                infectadosActuales++;
            }
        }
    }

    public void RemoveAldeano(People aldeano)
    {
        if (aldeanosActivos.Contains(aldeano))
        {
            aldeanosActivos.Remove(aldeano);
            aldeanosConcurrentes--;

            if (aldeano != null && aldeano.gameObject != null)
            {
                Destroy(aldeano.gameObject);
            }
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

        aldeanosConcurrentes++;
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

        aldeanosConcurrentes++;
        aldeanosTotalesGenerados++;
    }

    public int GetAldeanosConcurrentes() => aldeanosConcurrentes;
    public int GetAldeanosTotalesGenerados() => aldeanosTotalesGenerados;
    public int GetInfectadosActuales() => infectadosActuales;
    public bool HasReachedMaxGeneration() => aldeanosTotalesGenerados >= maxAldeanosTotalesGenerados;

    void OnDrawGizmosSelected()
    {
        if (spawnPoints != null)
        {
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
}