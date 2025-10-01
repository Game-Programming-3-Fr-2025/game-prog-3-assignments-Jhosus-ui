using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class TimeLoopManager : MonoBehaviour
{
    public static TimeLoopManager Instance;
    public Button loopButton; // Botón UI para hacer loop manual

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

    // Evento para notificar el reset del loop
    public System.Action OnLoopReset;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Encontrar al jugador automáticamente
        player = GameObject.FindGameObjectWithTag("Player");

        // Configurar botón de loop
        if (loopButton != null && allowManualLoop)
        {
            loopButton.onClick.AddListener(ForceLoopReset);
            loopButton.gameObject.SetActive(true);
        }

        StartTimeLoop();
    }

    void Update()
    {
        if (!isLoopActive) return;

        currentLoopTime += Time.deltaTime;

        // Loop automático
        if (currentLoopTime >= loopDuration)
        {
            StartCoroutine(ResetLoopAfterDelay(2f));
        }
    }

    // ===== REGISTRO DE ESTADOS IMPORTANTES =====

    public void RegisterCollectedItem(GameObject item)
    {
        if (!collectedItems.Contains(item))
        {
            collectedItems.Add(item);
            Debug.Log($"Item registrado: {item.name}");
        }
    }

    public void RegisterActivatedUI(GameObject uiElement)
    {
        if (!activatedUI.Contains(uiElement))
        {
            activatedUI.Add(uiElement);
        }
    }

    public void RegisterObjectState(string objectId, bool state)
    {
        objectStates[objectId] = state;
    }

    // ===== SISTEMA DE LOOP =====

    void StartTimeLoop()
    {
        isLoopActive = true;
        currentLoopTime = 0f;
        Debug.Log("🔄 Bucle Temporal Iniciado");
    }

    IEnumerator ResetLoopAfterDelay(float delay)
    {
        isLoopActive = false;
        Debug.Log("⏰ Bucle Terminado - Reiniciando en " + delay + " segundos");

        yield return new WaitForSeconds(delay);
        ResetTimeLoop();
    }

    public void ForceLoopReset()
    {
        if (!isLoopActive) return;

        Debug.Log("🎮 Loop manual activado por jugador");
        StartCoroutine(ResetLoopAfterDelay(1f));
    }

    void ResetTimeLoop()
    {
        Debug.Log("🔄 Aplicando Reset del Loop...");

        // 1. Resetear posición del jugador
        if (player != null && playerStartPosition != null)
        {
            player.transform.position = playerStartPosition.position;
            Debug.Log("👤 Jugador regresado a posición inicial");
        }
        else
        {
            // Buscar jugador y posición inicial si no están asignados
            if (player == null) player = GameObject.FindGameObjectWithTag("Player");
            if (playerStartPosition == null)
            {
                GameObject startPos = GameObject.Find("PlayerStartPosition");
                if (startPos != null) playerStartPosition = startPos.transform;
            }
        }

        // 2. Notificar a todos los objetos registrados que se reinicien
        OnLoopReset?.Invoke();

        // 3. Reactivar objetos recolectados
        foreach (GameObject item in collectedItems)
        {
            if (item != null)
            {
                item.SetActive(true);
                Debug.Log($"Reactivating: {item.name}");
            }
        }
        collectedItems.Clear();

        // 4. Desactivar UI elements activados
        foreach (GameObject uiElement in activatedUI)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(false);
            }
        }
        activatedUI.Clear();

        // 5. Limpiar estados de objetos
        objectStates.Clear();

        // 6. Reiniciar tiempo y estados
        currentLoopTime = 0f;
        isLoopActive = true;

        Debug.Log("✅ Mundo Reiniciado - Nuevo Loop Iniciado");
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(loopDuration - currentLoopTime, 0f);
    }

    public bool IsLoopActive()
    {
        return isLoopActive;
    }

    // Para asignar posición inicial desde otros scripts
    public void SetPlayerStartPosition(Transform newStartPosition)
    {
        playerStartPosition = newStartPosition;
    }

    // Para obtener estados guardados
    public bool GetObjectState(string objectId)
    {
        if (objectStates.ContainsKey(objectId))
            return objectStates[objectId];
        return false;
    }
}