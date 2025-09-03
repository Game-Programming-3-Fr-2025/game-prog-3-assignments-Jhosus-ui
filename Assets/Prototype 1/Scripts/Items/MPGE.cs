using UnityEngine;
using System.Collections.Generic;

public class MPGE : MonoBehaviour
{
    [System.Serializable]
    public class ToggleableObjects
    {
        public GameObject[] objectsToToggle; 
        public bool isActive = true; 
    }

    public ToggleableObjects toggleGroup;
    public KeyCode interactionKey = KeyCode.E;
    public float interactionDistance = 3f;

    [Header("Configuración de Alternancia")]
    public bool alternarObjetos = false;
    public ToggleableObjects grupoAlternativo;

    private GameObject player;
    private Camera mainCamera;
    private bool canInteract = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        mainCamera = Camera.main;

        // Inicializar el estado de los objetos
        InitializeObjects();
    }

    void Update()
    {
        CheckForInteraction();
    }

    void InitializeObjects()
    {
        // Inicializar grupo principal
        if (toggleGroup.objectsToToggle != null && toggleGroup.objectsToToggle.Length > 0)
        {
            SetObjectsState(toggleGroup.objectsToToggle, toggleGroup.isActive);
        }

        // Inicializar grupo alternativo si está activado
        if (alternarObjetos && grupoAlternativo.objectsToToggle != null && grupoAlternativo.objectsToToggle.Length > 0)
        {
            SetObjectsState(grupoAlternativo.objectsToToggle, grupoAlternativo.isActive);
        }
    }

    void CheckForInteraction()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        bool isLookingAtObject = IsLookingAtObject();

        canInteract = (distance <= interactionDistance && isLookingAtObject);

        if (canInteract)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                ToggleObjects();
            }
        }
    }

    bool IsLookingAtObject()
    {
        if (mainCamera == null || player == null) return false;
        Vector2 directionToObject = (transform.position - player.transform.position).normalized;
        Vector2 playerFacing = player.GetComponent<Player>().GetMovementDirection();

        if (playerFacing != Vector2.zero)
        {
            float dotProduct = Vector2.Dot(playerFacing, directionToObject);
            return dotProduct > 0.5f; // Umbral de detección frontal
        }

        return true; // Si no se mueve, permite interacción
    }

    void ToggleObjects()
    {
        if (alternarObjetos)
        {
            // Modo alternar: invertir ambos grupos
            toggleGroup.isActive = !toggleGroup.isActive;
            grupoAlternativo.isActive = !toggleGroup.isActive; // Estado opuesto

            SetObjectsState(toggleGroup.objectsToToggle, toggleGroup.isActive);
            SetObjectsState(grupoAlternativo.objectsToToggle, grupoAlternativo.isActive);

            Debug.Log(gameObject.name + ": Grupo principal " + (toggleGroup.isActive ? "activado" : "desactivado") +
                     ", Grupo alternativo " + (grupoAlternativo.isActive ? "activado" : "desactivado"));
        }
        else
        {
            // Modo simple: solo toggle del grupo principal
            toggleGroup.isActive = !toggleGroup.isActive;
            SetObjectsState(toggleGroup.objectsToToggle, toggleGroup.isActive);

            Debug.Log(gameObject.name + ": Objetos " + (toggleGroup.isActive ? "activados" : "desactivados"));
        }
    }

    void SetObjectsState(GameObject[] objects, bool state)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(state);
            }
        }
    }

    // Método para debug visual
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        // Líneas al grupo principal (rojo)
        if (toggleGroup.objectsToToggle != null)
        {
            Gizmos.color = Color.red;
            foreach (GameObject obj in toggleGroup.objectsToToggle)
            {
                if (obj != null)
                {
                    Gizmos.DrawLine(transform.position, obj.transform.position);
                }
            }
        }

        // Líneas al grupo alternativo (verde) - solo si está activado
        if (alternarObjetos && grupoAlternativo.objectsToToggle != null)
        {
            Gizmos.color = Color.green;
            foreach (GameObject obj in grupoAlternativo.objectsToToggle)
            {
                if (obj != null)
                {
                    Gizmos.DrawLine(transform.position, obj.transform.position);
                    Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.5f);
                }
            }
        }
    }

    // Opcional: Mostrar mensaje cuando el jugador está cerca
    void OnGUI()
    {
        if (canInteract)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 50, 200, 30),
                     "Presiona " + interactionKey + " para interactuar");
        }
    }
}