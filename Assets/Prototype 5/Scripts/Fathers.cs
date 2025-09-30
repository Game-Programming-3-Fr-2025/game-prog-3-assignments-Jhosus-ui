using UnityEngine;
using UnityEngine.AI;

public class Fathers : MonoBehaviour
{
    [Header("Settings")]
    public Transform[] patrolPoints; // Puntos de patrullaje
    public float waitTime = 2f; // Tiempo en cada punto

    private NavMeshAgent agent;
    private int currentPoint = 0;
    private float waitCounter;
    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Configurar para 2D (evitar rotaciones en Y)
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // Ir al primer punto de patrullaje directamente
        if (patrolPoints.Length > 0)
        {
            GoToNextPoint();
        }
    }

    void Update()
    {
        // Si está esperando, contar tiempo
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

        // Si llegó al destino y no está esperando, empezar a esperar
        if (!agent.pathPending && agent.remainingDistance < 0.1f && !isWaiting)
        {
            StartWaiting();
        }
    }

    void GoToNextPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentPoint].position);

        // Avanzar al siguiente punto (circular)
        currentPoint = (currentPoint + 1) % patrolPoints.Length;
    }

    void StartWaiting()
    {
        isWaiting = true;
        waitCounter = waitTime;
    }
}