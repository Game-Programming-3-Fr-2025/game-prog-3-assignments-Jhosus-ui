using UnityEngine;
using UnityEngine.AI;

public class Enemy7 : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 1f;

    private Transform player;
    private NavMeshAgent navMeshAgent;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed;
            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.updateRotation = false; 
            navMeshAgent.updateUpAxis = false; 
        }
    }

    void Update()
    {
        if (player != null && navMeshAgent != null)
        {
            navMeshAgent.SetDestination(player.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}