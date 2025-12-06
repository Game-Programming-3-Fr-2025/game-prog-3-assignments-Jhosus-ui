using UnityEngine;
using UnityEngine.AI;

public class Fathers : MonoBehaviour
{
    [Header("Settings")]
    public Transform[] patrolPoints;
    public float waitTime = 2f;
    public float detectionRadius = 3f;
    public float chaseRadius = 5f;
    public bool isMother = false;

    [Header("Mother Special Settings")]
    public KeyRoom specialKey;
    public float keyAppearDelay = 5f;
    public float keyDisappearDelay = 10f;

    [Header("Loop Reset")]
    public Transform initialPosition;

    private NavMeshAgent agent;
    private int currentPoint = 0;
    private float waitCounter;
    private bool isWaiting = false;
    private Transform player;
    private bool isChasing = false;
    private bool keySpawned = false;
    private Vector3 startPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        startPosition = initialPosition != null ? initialPosition.position : transform.position;

        TimeLoopManager.Instance.OnLoopReset += ResetFather;
        InitializePatrol();
    }

    void OnDestroy()
    {
        if (TimeLoopManager.Instance != null)
            TimeLoopManager.Instance.OnLoopReset -= ResetFather; // Limpiar suscripción
    }

    void InitializePatrol()
    {
        if (initialPosition != null) agent.Warp(initialPosition.position);

        currentPoint = 0;
        if (patrolPoints.Length > 0) GoToNextPoint();

        if (isMother && TimeLoopManager.Instance.IsLoopActive()) // Solo si el loop está activo
            Invoke("SpawnSpecialKey", keyAppearDelay);
    }

    void Update() // Lógica de comportamiento
    {
        if (isWaiting)
        {
            waitCounter -= Time.deltaTime;
            if (waitCounter <= 0)
            {
                isWaiting = false;
                GoToNextPoint();
            }
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position); // Distancia al jugador

        if (distanceToPlayer <= detectionRadius && !isChasing) StartChasing();
        else if (isChasing && distanceToPlayer > chaseRadius) StopChasing();

        if (isChasing) agent.SetDestination(player.position);
        else if (!agent.pathPending && agent.remainingDistance < 0.1f && !isWaiting) StartWaiting();
    }

    void SpawnSpecialKey() // Aparición de llave especial
    {
        if (isMother && specialKey != null && !keySpawned && TimeLoopManager.Instance.IsLoopActive())
        {
            specialKey.ActivateSpecialKey();
            keySpawned = true;
            TimeLoopManager.Instance.RegisterObjectState("MotherKey_spawned", true);
            Invoke("DespawnSpecialKey", keyDisappearDelay);
        }
    }

    void DespawnSpecialKey()
    {
        if (isMother && specialKey != null && keySpawned)
        {
            specialKey.gameObject.SetActive(false);
            keySpawned = false;
        }
    }

    void StartChasing() // Comenzar persecución
    {
        isChasing = true;
        isWaiting = false;
    }

    void StopChasing()
    {
        isChasing = false;
        GoToNextPoint();
    }

    void GoToNextPoint()
    {
        if (patrolPoints.Length == 0) return;
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.SetDestination(patrolPoints[currentPoint].position);
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
        }
    }

    void StartWaiting()
    {
        isWaiting = true;
        waitCounter = waitTime;
    }

    void ResetFather()// Reseteo al inicio del loop
    {
        CancelInvoke("SpawnSpecialKey");
        CancelInvoke("DespawnSpecialKey");

        isChasing = false;
        isWaiting = false;
        keySpawned = false;

        if (initialPosition != null) agent.Warp(initialPosition.position);
        else agent.Warp(startPosition);

        currentPoint = 0;
        if (patrolPoints.Length > 0 && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.SetDestination(patrolPoints[0].position);
        }

        if (isMother && specialKey != null)
        {
            specialKey.gameObject.SetActive(false); 
            keySpawned = false; // Resetear estado de spawn
            if (TimeLoopManager.Instance.IsLoopActive())
            {
                Invoke("SpawnSpecialKey", keyAppearDelay); 
            }
        }
    }

    void OnDrawGizmosSelected() // Visualización en editor
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        if (initialPosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(initialPosition.position, Vector3.one * 0.5f);
            Gizmos.DrawLine(transform.position, initialPosition.position);
        }
    }
}