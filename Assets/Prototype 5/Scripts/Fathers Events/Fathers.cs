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
    public KeyRoom specialKey; // Arrastrar la llave especial aquí
    public float keyAppearDelay = 5f;
    public float keyDisappearDelay = 10f;

    private NavMeshAgent agent;
    private int currentPoint = 0;
    private float waitCounter;
    private bool isWaiting = false;
    private Transform player;
    private bool isChasing = false;
    private bool keySpawned = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (patrolPoints.Length > 0 && !isMother)
        {
            GoToNextPoint();
        }

        // Si es Mother, empieza patrullaje y programa aparición de llave
        if (isMother && patrolPoints.Length > 0)
        {
            GoToNextPoint();
            Invoke("SpawnSpecialKey", keyAppearDelay);
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
        if (isMother && specialKey != null && !keySpawned)
        {
            specialKey.gameObject.SetActive(true);
            keySpawned = true;
            Debug.Log("Llave especial apareció por Mother");

            // Programar desaparición
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
    }
}