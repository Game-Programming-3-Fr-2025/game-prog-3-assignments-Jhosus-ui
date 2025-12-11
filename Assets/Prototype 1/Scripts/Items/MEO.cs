using UnityEngine;
using System.Collections.Generic;

public class MEO : MonoBehaviour
{
    [System.Serializable]
    public class MovableObject
    {
        public GameObject objectToMove;      
        public Vector3 moveDirection;         
        public int maxMoves = 3;              
        public float moveDistance = 1f;       
        public int currentMoves = 0;          
        public bool isLocked = false;         
    }

    [Header("Configuración")]
    public List<MovableObject> movableObjects = new List<MovableObject>();
    public KeyCode interactKey = KeyCode.E;
    public float interactionDistance = 2f;

    [Header("Recompensa")]
    public GameObject itemToAppear;          
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

    void CheckForInteraction()  //Seguir comprobando si no existe problema de area
    {
        if (player == null) return;

        foreach (MovableObject movable in movableObjects)
        {
            if (movable.objectToMove != null && !movable.isLocked)
            {
                float distance = Vector2.Distance(player.transform.position, movable.objectToMove.transform.position);

                if (distance <= interactionDistance && Input.GetKeyDown(interactKey)) //Opcion 1 de codigo mmm
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
            Vector3 newPosition = movable.objectToMove.transform.position +
                                 (movable.moveDirection.normalized * movable.moveDistance);

            movable.objectToMove.transform.position = newPosition;
            movable.currentMoves++;

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

    void OnDrawGizmosSelected() //Trazo de rutas y posiciones
    {

        Gizmos.color = Color.yellow;
        Gizmos.color = Color.blue;

        foreach (MovableObject movable in movableObjects)
        {
            if (movable.objectToMove != null)
            {
                Vector3 startPos = movable.objectToMove.transform.position;
                for (int i = 0; i < movable.maxMoves; i++)
                {
                    Vector3 segmentStart = startPos + (movable.moveDirection.normalized * movable.moveDistance * i);
                    Vector3 segmentEnd = segmentStart + (movable.moveDirection.normalized * movable.moveDistance);

                    Gizmos.color = i < movable.currentMoves ? Color.green : Color.blue;
                    Gizmos.DrawLine(segmentStart, segmentEnd);
                    Gizmos.DrawWireCube(segmentEnd, Vector3.one * 0.3f);
                }
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