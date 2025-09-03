using UnityEngine;
using System.Collections.Generic;

public class MEO : MonoBehaviour
{
    [System.Serializable]
    public class MovableObject
    {
        public GameObject objectToMove;       // Objeto a mover
        public Vector3 moveDirection;         // Dirección del movimiento
        public int maxMoves = 3;              // Cantidad máxima de movimientos
        public float moveDistance = 1f;       // Distancia por movimiento
        public int currentMoves = 0;          // Movimientos actuales
        public bool isLocked = false;         // Si ya llegó al máximo
    }

    [Header("Configuración")]
    public List<MovableObject> movableObjects = new List<MovableObject>();
    public KeyCode interactKey = KeyCode.E;
    public float interactionDistance = 2f;

    [Header("Recompensa")]
    public GameObject itemToAppear;           // Item que aparece cuando todos terminan
    public bool rewardShown = false;

    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (itemToAppear != null)
            itemToAppear.SetActive(false);
    }

    void Update()
    {
        CheckForInteraction();
        CheckAllObjectsCompleted();
    }

    void CheckForInteraction()
    {
        if (player == null) return;

        foreach (MovableObject movable in movableObjects)
        {
            if (movable.objectToMove != null && !movable.isLocked)
            {
                float distance = Vector2.Distance(player.transform.position, movable.objectToMove.transform.position);

                if (distance <= interactionDistance && Input.GetKeyDown(interactKey))
                {
                    MoveObject(movable);
                }
            }
        }
    }

    void MoveObject(MovableObject movable)
    {
        if (movable.currentMoves < movable.maxMoves)
        {
            // Calcular nueva posición
            Vector3 newPosition = movable.objectToMove.transform.position +
                                 (movable.moveDirection.normalized * movable.moveDistance);

            // Mover el objeto
            movable.objectToMove.transform.position = newPosition;
            movable.currentMoves++;

            // Verificar si llegó al máximo
            if (movable.currentMoves >= movable.maxMoves)
            {
                movable.isLocked = true;
            }
        }
    }

    void CheckAllObjectsCompleted()
    {
        if (rewardShown || itemToAppear == null) return;

        bool allCompleted = true;

        foreach (MovableObject movable in movableObjects)
        {
            if (!movable.isLocked)
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            ShowReward();
        }
    }

    void ShowReward()
    {
        rewardShown = true;
        itemToAppear.SetActive(true);
    }

    // Debug visual con Gizmos
    void OnDrawGizmosSelected()
    {
        // Configuración de colores
        Gizmos.color = Color.yellow;
        Gizmos.color = Color.blue;

        foreach (MovableObject movable in movableObjects)
        {
            if (movable.objectToMove != null)
            {
                Vector3 startPos = movable.objectToMove.transform.position;

                // Dibujar ruta completa
                for (int i = 0; i < movable.maxMoves; i++)
                {
                    Vector3 segmentStart = startPos + (movable.moveDirection.normalized * movable.moveDistance * i);
                    Vector3 segmentEnd = segmentStart + (movable.moveDirection.normalized * movable.moveDistance);

                    Gizmos.color = i < movable.currentMoves ? Color.green : Color.blue;
                    Gizmos.DrawLine(segmentStart, segmentEnd);
                    Gizmos.DrawWireCube(segmentEnd, Vector3.one * 0.3f);
                }

                // Dibujar esfera de interacción
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(startPos, interactionDistance);
            }
        }

        // Dibujar item de recompensa
        if (itemToAppear != null)
        {
            Gizmos.color = rewardShown ? Color.green : Color.red;
            Gizmos.DrawWireCube(itemToAppear.transform.position, Vector3.one * 0.5f);
        }
    }
}