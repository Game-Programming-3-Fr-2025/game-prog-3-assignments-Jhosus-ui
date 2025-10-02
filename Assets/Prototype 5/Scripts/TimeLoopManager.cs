using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class TimeLoopManager : MonoBehaviour
{
    public static TimeLoopManager Instance;
    public Button loopButton; // Botón UI para loop manual

    [Header("Loop Settings")]
    public float loopDuration = 300f; // 5 minutos por defecto
    public bool allowManualLoop = true;

    [Header("Player Reset")]
    public Transform playerStartPosition;
    private GameObject player;

    private float currentLoopTime;
    private bool isLoopActive = true;

    // Listas para guardar estados
    private List<GameObject> collectedItems = new List<GameObject>();
    private List<GameObject> activatedUI = new List<GameObject>();
    private Dictionary<string, bool> objectStates = new Dictionary<string, bool>();

    public System.Action OnLoopReset; // Evento para notificar reset

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persistir entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); // Buscar jugador automáticamente

        if (loopButton != null && allowManualLoop)
        {
            loopButton.onClick.AddListener(ForceLoopReset); // Configurar botón manual
            loopButton.gameObject.SetActive(true);
        }

        StartTimeLoop();
    }

    void Update()
    {
        if (!isLoopActive) return;

        currentLoopTime += Time.deltaTime; // Contador de tiempo del loop

        if (currentLoopTime >= loopDuration) 
        {
            StartCoroutine(ResetLoopAfterDelay(2f));
        }
    }
    public void RegisterCollectedItem(GameObject item)
    {
        if (!collectedItems.Contains(item))
        {
            collectedItems.Add(item); // Registrar item recolectado
        }
    }

    public void RegisterActivatedUI(GameObject uiElement)
    {
        if (!activatedUI.Contains(uiElement))
        {
            activatedUI.Add(uiElement); // Registrar UI activado
        }
    }

    public void RegisterObjectState(string objectId, bool state)
    {
        objectStates[objectId] = state; // Guardar estado de objeto
    }
    void StartTimeLoop()
    {
        isLoopActive = true;
        currentLoopTime = 0f; // Reiniciar contador
    }

    IEnumerator ResetLoopAfterDelay(float delay)
    {
        isLoopActive = false; // Pausar loop durante reset

        yield return new WaitForSeconds(delay);
        ResetTimeLoop();
    }

    public void ForceLoopReset()
    {
        if (!isLoopActive) return;

        StartCoroutine(ResetLoopAfterDelay(1f)); // Loop manual más rápido
    }

    void ResetTimeLoop()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.loopReset); // SONIDO LOOP

        // 1. Resetear posición del jugador
        if (player != null && playerStartPosition != null)
        {
            player.transform.position = playerStartPosition.position; // Volver al inicio
        }
        else
        {
            // Buscar componentes si no están asignados
            if (player == null) player = GameObject.FindGameObjectWithTag("Player");
            if (playerStartPosition == null)
            {
                GameObject startPos = GameObject.Find("PlayerStartPosition");
                if (startPos != null) playerStartPosition = startPos.transform;
            }
        }

        // 2. Notificar a todos los objetos registrados
        OnLoopReset?.Invoke(); 

        // 3. Reactivar objetos recolectados
        foreach (GameObject item in collectedItems)
        {
            if (item != null) item.SetActive(true); 
        }
        collectedItems.Clear(); 

        // 4. Desactivar UI elements activados
        foreach (GameObject uiElement in activatedUI)
        {
            if (uiElement != null) uiElement.SetActive(false); 
        }
        activatedUI.Clear();

        // 5. Limpiar estados de objetos
        objectStates.Clear(); 

        // 6. Reiniciar tiempo y estados
        currentLoopTime = 0f;
        isLoopActive = true; 
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(loopDuration - currentLoopTime, 0f); // Tiempo restante
    }

    public bool IsLoopActive()
    {
        return isLoopActive; // Estado actual del loop
    }

    public void SetPlayerStartPosition(Transform newStartPosition)
    {
        playerStartPosition = newStartPosition; // Cambiar posición inicial
    }

    public bool GetObjectState(string objectId)
    {
        if (objectStates.ContainsKey(objectId))
            return objectStates[objectId]; // Obtener estado guardado
        return false;
    }
}