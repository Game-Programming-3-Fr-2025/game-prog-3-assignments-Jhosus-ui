using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Falling : MonoBehaviour
{
    [Header("Configuración")]
    public float margin = 0.5f, respawnDelay = 0.5f;
    public string respawnTag = "RespawnPoint";
    public Camera playerCamera;
    public bool mostrarGizmos = true;

    [Header("Efectos Visuales")]
    public float parpadeoDuracion = 1.5f;
    public float parpadeoVelocidad = 0.1f;
    public Color parpadeoColor = new Color(1, 1, 1, 0.5f);

    [Header("Efectos de Vibración (Mando)")]
    public bool usarVibracion = true;
    public float vibracionFuerza = 0.7f;
    public float vibracionDuracion = 0.3f;
    public int vibracionCantidad = 3;

    private LifeSystem playerLife;
    private Rigidbody2D rb;
    private List<Transform> respawnPoints = new List<Transform>();
    private Vector3 cameraBoundsMin, cameraBoundsMax;
    private float lastOutOfBoundsTime = -10f, halfCameraHeight, halfCameraWidth;
    private int outOfBoundsCount, frameCounter;
    private bool isRespawning, isPlayer1;

    // Referencias para efectos
    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;
    private bool tieneMandoConectado;
    private int playerIndex;

    void Start()
    {
        playerLife = GetComponent<LifeSystem>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer) colorOriginal = spriteRenderer.color;

        isPlayer1 = CompareTag("Player 1");
        playerIndex = isPlayer1 ? 0 : 1;

        FindPlayerCamera();
        CalculateCameraBounds();
        FindAllRespawnPoints();

        // Detectar si hay mando conectado
        CheckMandoConectado();
    }

    void CheckMandoConectado()
    {
        // Detectar si hay mandos conectados
        string[] joystickNames = Input.GetJoystickNames();
        tieneMandoConectado = joystickNames.Length > playerIndex &&
                              !string.IsNullOrEmpty(joystickNames[playerIndex]);

        Debug.Log($"{gameObject.name} - Mando conectado: {tieneMandoConectado}");
    }

    void FindPlayerCamera()
    {
        if (playerCamera) return;
        var camObj = GameObject.Find(isPlayer1 ? "MainCameraP1" : "MainCameraP2");
        if (playerCamera = camObj ? camObj.GetComponent<Camera>() : Camera.main)
        {
            halfCameraHeight = playerCamera.orthographicSize;
            halfCameraWidth = halfCameraHeight * playerCamera.aspect;
        }
    }

    void CalculateCameraBounds() => halfCameraWidth = (halfCameraHeight = playerCamera.orthographicSize) * playerCamera.aspect;

    void FindAllRespawnPoints()
    {
        respawnPoints.Clear();
        foreach (var obj in GameObject.FindGameObjectsWithTag(respawnTag))
            respawnPoints.Add(obj.transform);
    }

    void Update()
    {
        if (!playerCamera) { FindPlayerCamera(); return; }
        if (isRespawning) return;

        UpdateCameraBounds();
        if (++frameCounter >= 120)
        {
            frameCounter = 0;
            FindAllRespawnPoints();
            CheckMandoConectado(); // Revisar periódicamente si se conectó/desconectó mando
        }

        var pos = transform.position;
        if (pos.x < cameraBoundsMin.x || pos.x > cameraBoundsMax.x ||
            pos.y < cameraBoundsMin.y || pos.y > cameraBoundsMax.y)
        {
            if (Time.time - lastOutOfBoundsTime > respawnDelay)
            {
                HandleOutOfBounds();
                lastOutOfBoundsTime = Time.time;
            }
        }
        else outOfBoundsCount = 0;
    }

    void UpdateCameraBounds()
    {
        var camPos = playerCamera.transform.position;
        cameraBoundsMin = new Vector3(camPos.x - halfCameraWidth - margin, camPos.y - halfCameraHeight - margin, transform.position.z);
        cameraBoundsMax = new Vector3(camPos.x + halfCameraWidth + margin, camPos.y + halfCameraHeight + margin, transform.position.z);
    }

    void HandleOutOfBounds()
    {
        isRespawning = true;
        if (++outOfBoundsCount >= 2 && playerLife)
        {
            playerLife.TakeDamage(1);
            outOfBoundsCount = 0;
        }

        var bestRespawn = FindClosestRespawnToCenter();
        if (bestRespawn)
        {
            StartCoroutine(EfectoRespawnCompleto(bestRespawn.position));
        }
        else
        {
            ResetPhysics();
            isRespawning = false;
        }
    }

    IEnumerator EfectoRespawnCompleto(Vector3 targetPosition)
    {
        // 1. Vibrar si hay mando conectado
        if (usarVibracion && tieneMandoConectado)
        {
            StartCoroutine(EfectoVibracionMando());
        }

        // 2. Parpadear durante el movimiento
        StartCoroutine(EfectoParpadeo());

        // 3. Esperar un momento antes de teletransportar
        yield return new WaitForSeconds(0.1f);

        // 4. Mover al jugador
        transform.position = targetPosition;
        ResetPhysics();

        // 5. Continuar parpadeando después del respawn
        yield return new WaitForSeconds(parpadeoDuracion);

        // 6. Restaurar color normal
        if (spriteRenderer)
        {
            spriteRenderer.color = colorOriginal;
        }

        isRespawning = false;
    }

    IEnumerator EfectoParpadeo()
    {
        if (!spriteRenderer) yield break;

        float tiempoInicio = Time.time;
        bool visible = true;

        while (Time.time - tiempoInicio < parpadeoDuracion)
        {
            if (visible)
            {
                spriteRenderer.color = parpadeoColor;
            }
            else
            {
                spriteRenderer.color = colorOriginal;
            }

            visible = !visible;
            yield return new WaitForSeconds(parpadeoVelocidad);
        }

        spriteRenderer.color = colorOriginal;
    }

    IEnumerator EfectoVibracionMando()
    {
        if (!tieneMandoConectado) yield break;

        for (int i = 0; i < vibracionCantidad; i++)
        {
            // Usar InputSystem si está disponible, si no usar el sistema antiguo
#if ENABLE_INPUT_SYSTEM
            // Para el nuevo Input System
            var gamepad = UnityEngine.InputSystem.Gamepad.current;
            if (gamepad != null)
            {
                gamepad.SetMotorSpeeds(vibracionFuerza, vibracionFuerza * 0.8f);
                yield return new WaitForSeconds(vibracionDuracion / vibracionCantidad);
                gamepad.SetMotorSpeeds(0, 0);
            }
#else
            // Para el Input Manager tradicional (Unity viejo)
            try
            {
                // Vibrar el mando específico del jugador
                if (playerIndex < Input.GetJoystickNames().Length)
                {
                    // Nota: SetVibration solo funciona en algunas plataformas
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
                    // Para PC
                    StartCoroutine(VibrarPC());
#elif UNITY_PS4 || UNITY_XBOXONE || UNITY_SWITCH
                    // Para consolas (requiere APIs específicas)
                    Debug.Log("Vibración en consola - implementación específica requerida");
#endif
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error en vibración: {e.Message}");
            }
#endif

            yield return new WaitForSeconds(vibracionDuracion / vibracionCantidad);
        }
    }

#if !ENABLE_INPUT_SYSTEM && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX)
    IEnumerator VibrarPC()
    {
        // Método alternativo para PC usando transform shaking si no hay API de vibración
        float shakeDuration = vibracionDuracion / 3f;
        float shakeMagnitude = 0.1f;
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = originalPos.x + Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = originalPos.y + Random.Range(-shakeMagnitude, shakeMagnitude);
            transform.localPosition = new Vector3(x, y, originalPos.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localPosition = originalPos;
    }
#endif

    Transform FindClosestRespawnToCenter()
    {
        if (respawnPoints.Count == 0) return null;

        var cameraCenter = new Vector2(playerCamera.transform.position.x, playerCamera.transform.position.y);
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var respawn in respawnPoints)
        {
            if (!respawn) continue;
            float dist = Vector2.Distance(new Vector2(respawn.position.x, respawn.position.y), cameraCenter);
            if (dist < minDist) { minDist = dist; closest = respawn; }
        }

        return closest;
    }

    void ResetPhysics()
    {
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // También resetear cualquier movimiento residual
            rb.linearVelocity = Vector2.zero;
            rb.Sleep();
            rb.WakeUp();
        }
    }

    public void ForceRespawnTest() => HandleOutOfBounds();

    void OnDrawGizmos()
    {
        if (!mostrarGizmos || !Application.isPlaying || !playerCamera) return;

        var center = playerCamera.transform.position;

        // Dibujar límites de la cámara
        Gizmos.color = isPlayer1 ? new Color(0, 0, 1, 0.3f) : new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireCube(center, new Vector3(halfCameraWidth * 2, halfCameraHeight * 2, 0.1f));

        // Dibujar límites con margen (área segura)
        Gizmos.color = isPlayer1 ? new Color(0, 1, 1, 0.5f) : new Color(1, 1, 0, 0.5f);
        Gizmos.DrawWireCube(center, new Vector3((halfCameraWidth + margin) * 2, (halfCameraHeight + margin) * 2, 0.1f));

        // Dibujar posición del jugador
        Gizmos.color = isPlayer1 ? Color.blue : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // Indicador de mando conectado
        if (tieneMandoConectado)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.2f);
        }

        // Centro de la cámara
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, 0.2f);

        // Puntos de respawn
        foreach (var respawn in respawnPoints)
        {
            if (!respawn) continue;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(respawn.position, 0.5f);
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawLine(center, respawn.position);
        }
    }

    // Método público para probar efectos
    public void ProbarEfectos()
    {
        if (!isRespawning)
        {
            StartCoroutine(EfectoParpadeo());
            if (tieneMandoConectado)
            {
                StartCoroutine(EfectoVibracionMando());
            }
        }
    }
}