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
    public Transform initialPosition; // Objeto vacío con posición inicial

    private NavMeshAgent agent;
    private int currentPoint = 0;
    private float waitCounter;
    private bool isWaiting = false;
    private Transform player;
    private bool isChasing = false;
    private bool keySpawned = false;
    private Vector3 startPosition; // Guardar posición inicial

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Guardar posición inicial
        if (initialPosition != null)
        {
            startPosition = initialPosition.position;
        }
        else
        {
            startPosition = transform.position;
        }

        // Suscribirse al Time Loop
        TimeLoopManager.Instance.OnLoopReset += ResetFather;

        InitializePatrol();
    }

    void OnDestroy()
    {
        if (TimeLoopManager.Instance != null)
            TimeLoopManager.Instance.OnLoopReset -= ResetFather;
    }

    void InitializePatrol()
    {
        // Posicionar en inicio
        if (initialPosition != null)
        {
            agent.Warp(initialPosition.position);
        }

        // Reiniciar punto de patrullaje
        currentPoint = 0;

        if (patrolPoints.Length > 0 && !isMother)
        {
            GoToNextPoint();
        }

        if (isMother && patrolPoints.Length > 0)
        {
            GoToNextPoint();
            // Solo programar la llave si el Time Loop está activo
            if (TimeLoopManager.Instance.IsLoopActive())
            {
                Invoke("SpawnSpecialKey", keyAppearDelay);
            }
        }
    }

    void Update()
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

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius && !isChasing)
        {
            StartChasing();
        }
        else if (isChasing && distanceToPlayer > chaseRadius)
        {
            StopChasing();
        }

        if (isChasing)
        {
            agent.SetDestination(player.position);
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.1f && !isWaiting)
        {
            StartWaiting();
        }
    }

    void SpawnSpecialKey()
    {
        if (isMother && specialKey != null && !keySpawned && TimeLoopManager.Instance.IsLoopActive())
        {
            specialKey.gameObject.SetActive(true);
            keySpawned = true;
            Debug.Log("Llave especial apareció por Mother");

            // Registrar la llave especial activada
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
            Debug.Log("Llave especial desapareció");
        }
    }

    void StartChasing()
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
        agent.SetDestination(patrolPoints[currentPoint].position);
        currentPoint = (currentPoint + 1) % patrolPoints.Length;
    }

    void StartWaiting()
    {
        isWaiting = true;
        waitCounter = waitTime;
    }

    // Resetear durante el Time Loop
    void ResetFather()
    {
        // Cancelar todos los Invokes
        CancelInvoke("SpawnSpecialKey");
        CancelInvoke("DespawnSpecialKey");

        // Resetear estados de comportamiento
        isChasing = false;
        isWaiting = false;
        keySpawned = false;

        // Volver a posición inicial
        if (initialPosition != null)
        {
            agent.Warp(initialPosition.position);
        }
        else
        {
            agent.Warp(startPosition);
        }

        // Reiniciar patrullaje desde el primer punto
        currentPoint = 0;
        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[0].position);
        }

        // Si es Mother, reprogramar la llave desde cero
        if (isMother && specialKey != null)
        {
            specialKey.gameObject.SetActive(false);
            if (TimeLoopManager.Instance.IsLoopActive())
            {
                Invoke("SpawnSpecialKey", keyAppearDelay);
            }
        }

        Debug.Log($"Father {gameObject.name} reseteado - Posición y patrullaje reiniciados");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        // Dibujar posición inicial
        if (initialPosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(initialPosition.position, Vector3.one * 0.5f);
            Gizmos.DrawLine(transform.position, initialPosition.position);
        }
    }
}