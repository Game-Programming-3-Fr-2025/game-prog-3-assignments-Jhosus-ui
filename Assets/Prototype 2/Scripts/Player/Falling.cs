using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Falling : MonoBehaviour
{
    [Header("Configuración")]
    public float marginHorizontal = 0.5f, marginVertical = 0.5f, respawnDelay = 0.5f;
    public string respawnTag = "RespawnPoint";
    public Camera playerCamera;
    public bool mostrarGizmos = true;

    [Header("Efectos")]
    public float parpadeoDuracion = 1.5f, parpadeoVelocidad = 0.1f;
    public Color parpadeoColor = new Color(1, 1, 1, 0.5f);
    public bool usarVibracion = true;
    public float vibracionFuerza = 0.7f, vibracionDuracion = 0.3f;
    public int vibracionCantidad = 3;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private LifeSystem playerLife;
    private Color colorOriginal;
    private List<Transform> respawnPoints = new List<Transform>();
    private Vector3 cameraBoundsMin, cameraBoundsMax;
    private float lastOutOfBoundsTime = -10f, halfCameraHeight, halfCameraWidth;
    private int caidasCount, frameCounter, playerIndex;
    private bool isRespawning, isPlayer1, tieneMandoConectado;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerLife = GetComponent<LifeSystem>();
        if (spriteRenderer) colorOriginal = spriteRenderer.color;
        isPlayer1 = CompareTag("Player 1");
        playerIndex = isPlayer1 ? 0 : 1;
        FindPlayerCamera();
        FindAllRespawnPoints();
        CheckMandoConectado();
    }

    void CheckMandoConectado()
    {
        var joysticks = Input.GetJoystickNames();
        tieneMandoConectado = joysticks.Length > playerIndex && !string.IsNullOrEmpty(joysticks[playerIndex]);
    }

    void FindPlayerCamera()
    {
        if (playerCamera) return;
        var camObj = GameObject.Find(isPlayer1 ? "MainCameraP1" : "MainCameraP2");
        if (playerCamera = camObj ? camObj.GetComponent<Camera>() : Camera.main)
            halfCameraWidth = (halfCameraHeight = playerCamera.orthographicSize) * playerCamera.aspect;
    }

    void FindAllRespawnPoints()
    {
        respawnPoints.Clear();
        foreach (var obj in GameObject.FindGameObjectsWithTag(respawnTag))
            respawnPoints.Add(obj.transform);
    }

    void Update()
    {
        if (!playerCamera || isRespawning) return;

        var camPos = playerCamera.transform.position;
        cameraBoundsMin = new Vector3(camPos.x - halfCameraWidth - marginHorizontal, camPos.y - halfCameraHeight - marginVertical, transform.position.z);
        cameraBoundsMax = new Vector3(camPos.x + halfCameraWidth + marginHorizontal, camPos.y + halfCameraHeight + marginVertical, transform.position.z);

        if (++frameCounter >= 120) { frameCounter = 0; FindAllRespawnPoints(); CheckMandoConectado(); }

        var pos = transform.position;
        if ((pos.x < cameraBoundsMin.x || pos.x > cameraBoundsMax.x || pos.y < cameraBoundsMin.y || pos.y > cameraBoundsMax.y)
            && Time.time - lastOutOfBoundsTime > respawnDelay)
        {
            HandleOutOfBounds();
            lastOutOfBoundsTime = Time.time;
        }
    }

    void HandleOutOfBounds()
    {
        isRespawning = true;
        caidasCount++;

        bool aplicarDanio = caidasCount >= 2;
        if (aplicarDanio)
        {
            if (playerLife) playerLife.TakeDamage(1);
            caidasCount = 0;
        }

        var respawn = FindClosestRespawnToCenter();
        if (respawn) StartCoroutine(RespawnConEfectos(respawn.position));
        else { ResetPhysics(); isRespawning = false; }
    }

    IEnumerator RespawnConEfectos(Vector3 targetPos)
    {
        if (usarVibracion && tieneMandoConectado) StartCoroutine(Vibrar());
        StartCoroutine(Parpadear());

        yield return new WaitForSeconds(0.1f);
        transform.position = targetPos;
        if (rb) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }

        yield return new WaitForSeconds(parpadeoDuracion);
        if (spriteRenderer) spriteRenderer.color = colorOriginal;
        isRespawning = false;
    }

    IEnumerator Parpadear()
    {
        if (!spriteRenderer) yield break;
        float inicio = Time.time;
        bool visible = true;
        while (Time.time - inicio < parpadeoDuracion)
        {
            spriteRenderer.color = visible ? parpadeoColor : colorOriginal;
            visible = !visible;
            yield return new WaitForSeconds(parpadeoVelocidad);
        }
    }

    IEnumerator Vibrar()
    {
        for (int i = 0; i < vibracionCantidad; i++)
        {
#if ENABLE_INPUT_SYSTEM
            var gamepad = UnityEngine.InputSystem.Gamepad.all.Count > playerIndex ? UnityEngine.InputSystem.Gamepad.all[playerIndex] : null;
            if (gamepad != null)
            {
                gamepad.SetMotorSpeeds(vibracionFuerza, vibracionFuerza * 0.8f);
                yield return new WaitForSeconds(vibracionDuracion / vibracionCantidad);
                gamepad.SetMotorSpeeds(0, 0);
            }
#else
            yield return new WaitForSeconds(vibracionDuracion / vibracionCantidad);
#endif
        }
    }

    Transform FindClosestRespawnToCenter()
    {
        if (respawnPoints.Count == 0) return null;
        var center = new Vector2(playerCamera.transform.position.x, playerCamera.transform.position.y);
        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (var respawn in respawnPoints)
        {
            if (!respawn) continue;
            float dist = Vector2.Distance(new Vector2(respawn.position.x, respawn.position.y), center);
            if (dist < minDist) { minDist = dist; closest = respawn; }
        }
        return closest;
    }

    void ResetPhysics() { if (rb) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; } }

    public void ForceRespawnTest() => HandleOutOfBounds();

    void OnDrawGizmos()
    {
        if (!mostrarGizmos || !Application.isPlaying || !playerCamera) return;
        var center = playerCamera.transform.position;

        Gizmos.color = isPlayer1 ? new Color(0, 0, 1, 0.3f) : new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireCube(center, new Vector3(halfCameraWidth * 2, halfCameraHeight * 2, 0.1f));
        Gizmos.color = isPlayer1 ? new Color(0, 1, 1, 0.5f) : new Color(1, 1, 0, 0.5f);
        Gizmos.DrawWireCube(center, new Vector3((halfCameraWidth + marginHorizontal) * 2, (halfCameraHeight + marginVertical) * 2, 0.1f));
        Gizmos.color = isPlayer1 ? Color.blue : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        if (tieneMandoConectado)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.2f);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, 0.2f);

        foreach (var respawn in respawnPoints)
        {
            if (!respawn) continue;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(respawn.position, 0.5f);
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawLine(center, respawn.position);
        }
    }
}