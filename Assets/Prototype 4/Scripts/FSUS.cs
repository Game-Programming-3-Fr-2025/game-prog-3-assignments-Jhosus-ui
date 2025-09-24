using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FSUS : MonoBehaviour
{
    [Header("=== ESTADO PASIVO ===")]
    [Tooltip("Marcar para corta distancia, desmarcar para larga distancia")]
    public bool isShortRange = true;

    [Header("Configuración Pasiva")]
    public float fireRate = 1f;
    public float range = 5f;
    public GameObject balaPrefabPasiva;
    public float damagePasivo = 10f;
    public float radioDanoPasivo = 1f;

    // CAMBIO CRÍTICO: Buscar point automáticamente en este mismo objeto
    [Header("Point Fire Pasivo")]
    [Tooltip("Si está vacío, creará un point automáticamente")]
    public Transform pointFirePasivo;

    [Header("=== ESTADO ACTIVO ===")]
    [Header("Configuración UI")]
    public string nombreBotonActivo = "BotonHabilidadActiva";
    [System.NonSerialized] public Button buttonSpriteActivo;
    public bool activoDisponible = false;

    [Header("Configuración Ataque Activo")]
    public int rafagaCantidad = 3;
    public float tiempoEntreRafagas = 0.2f;
    public int balasPorRafaga = 3;
    public float separacionVerticalBalas = 0.5f;
    public GameObject balaPrefabActiva;
    public float damageActivo = 25f;
    public float radioDanoActivo = 2f;
    public string nombrePointFire = "Point";

    [Header("Tiempos Activo")]
    public float duracionModoActivo = 5f;
    public float cooldownModoActivo = 10f;

    private float nextFireTime = 0f;
    private bool modoActivo = false;
    private float tiempoFinActivo = 0f;
    private float nextCooldownTime = 0f;
    private bool enCooldown = false;
    private Transform pointFireActivo;

    private GameManager gameManager;
    private Guns gunsSystem;

    void Start()
    {
        BuscarReferencias();
        ConfigurarPointsFire();

        if (buttonSpriteActivo != null)
        {
            buttonSpriteActivo.onClick.AddListener(ActivarModoActivo);
            buttonSpriteActivo.gameObject.SetActive(false);
        }

        VerificarArmasEquipadas();
        Debug.Log($"FSUS iniciado en {gameObject.name} - Point Pasivo: {pointFirePasivo != null}");
    }

    private void BuscarReferencias()
    {
        gameManager = FindObjectOfType<GameManager>();
        gunsSystem = FindObjectOfType<Guns>();
        buttonSpriteActivo = BuscarBotonPorNombre(nombreBotonActivo);
    }

    // MÉTODO CRÍTICO: Configurar points de disparo
    private void ConfigurarPointsFire()
    {
        // Point Fire Pasivo: Si no está asignado, buscar en hijos o crear uno
        if (pointFirePasivo == null)
        {
            // Buscar en hijos primero
            pointFirePasivo = transform.Find("PointFirePasivo");

            if (pointFirePasivo == null)
            {
                // Crear automáticamente
                GameObject newPoint = new GameObject("PointFirePasivo");
                newPoint.transform.SetParent(transform);
                newPoint.transform.localPosition = new Vector3(0.5f, 0, 0); // Adelante del arma
                pointFirePasivo = newPoint.transform;
                Debug.Log($"Point Fire Pasivo creado automáticamente en {gameObject.name}");
            }
        }

        // Point Fire Activo
        pointFireActivo = BuscarObjetoPorNombre(nombrePointFire);
        if (pointFireActivo == null)
        {
            // Usar el mismo point que el pasivo si no encuentra el activo
            pointFireActivo = pointFirePasivo;
            Debug.Log($"Usando Point Fire Pasivo para modo activo en {gameObject.name}");
        }
    }

    private Transform BuscarObjetoPorNombre(string nombre)
    {
        GameObject obj = GameObject.Find(nombre);
        if (obj != null)
        {
            return obj.transform;
        }
        return null;
    }

    private Button BuscarBotonPorNombre(string nombre)
    {
        Button[] todosBotones = FindObjectsOfType<Button>(true);
        foreach (Button boton in todosBotones)
        {
            if (boton.name == nombre)
            {
                return boton;
            }
        }
        return null;
    }

    void Update()
    {
        VerificarArmasEquipadas();

        if (buttonSpriteActivo != null)
        {
            buttonSpriteActivo.interactable = activoDisponible && !enCooldown && !modoActivo;
        }

        // DISPARO PASIVO - Verificación mejorada
        if (!modoActivo && Time.time >= nextFireTime && EstaArmaEquipada())
        {
            DisparoPasivo();
            nextFireTime = Time.time + 1f / fireRate;
        }

        if (modoActivo && Time.time >= tiempoFinActivo)
        {
            DesactivarModoActivo();
        }

        if (enCooldown && Time.time >= nextCooldownTime)
        {
            enCooldown = false;
        }
    }

    // MÉTODO CRÍTICO: Verificar si esta arma específica está equipada
    private bool EstaArmaEquipada()
    {
        if (gunsSystem == null) return false;

        // Verificar si este script pertenece a un arma equipada
        foreach (var slot in gunsSystem.moduleSlots)
        {
            if (slot.hasWeapon && slot.weaponInstance != null)
            {
                // Verificar si este script está en el arma equipada
                FSUS armaScript = slot.weaponInstance.GetComponent<FSUS>();
                if (armaScript == this)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void DisparoPasivo()
    {
        if (balaPrefabPasiva == null || pointFirePasivo == null)
        {
            Debug.LogWarning($"Falta bala prefab o point fire en {gameObject.name}");
            return;
        }

        GameObject enemigoCercano = BuscarEnemigoCercano();

        if (enemigoCercano != null)
        {
            Vector2 direccion = (enemigoCercano.transform.position - pointFirePasivo.position).normalized;
            GameObject nuevaBala = Instantiate(balaPrefabPasiva, pointFirePasivo.position, Quaternion.identity);

            Bala balaScript = nuevaBala.GetComponent<Bala>();
            if (balaScript != null)
            {
                balaScript.SetConfiguracion(direccion, damagePasivo, radioDanoPasivo, isShortRange);
                Debug.Log($"{gameObject.name} disparó a {enemigoCercano.name}");
            }
        }
    }

    private GameObject BuscarEnemigoCercano()
    {
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject enemigoMasCercano = null;
        float distanciaMasCercana = Mathf.Infinity;

        foreach (GameObject enemigo in enemigos)
        {
            if (enemigo == null) continue;

            float distancia = Vector2.Distance(transform.position, enemigo.transform.position);
            if (distancia <= range && distancia < distanciaMasCercana)
            {
                distanciaMasCercana = distancia;
                enemigoMasCercano = enemigo;
            }
        }
        return enemigoMasCercano;
    }

    public void ActivarModoActivo()
    {
        if (enCooldown || modoActivo || !activoDisponible) return;

        modoActivo = true;
        tiempoFinActivo = Time.time + duracionModoActivo;
        StartCoroutine(DispararRafagas());

        if (buttonSpriteActivo != null)
        {
            buttonSpriteActivo.interactable = false;
        }

        Debug.Log($"Modo activo iniciado en {gameObject.name}");
    }

    private void DesactivarModoActivo()
    {
        modoActivo = false;
        enCooldown = true;
        nextCooldownTime = Time.time + cooldownModoActivo;
        Debug.Log($"Modo activo finalizado en {gameObject.name}");
    }

    private IEnumerator DispararRafagas()
    {
        for (int i = 0; i < rafagaCantidad; i++)
        {
            DispararRafaga();
            yield return new WaitForSeconds(tiempoEntreRafagas);
        }
    }

    private void DispararRafaga()
    {
        if (balaPrefabActiva == null || pointFireActivo == null) return;

        for (int i = 0; i < balasPorRafaga; i++)
        {
            Vector3 posicionDisparo = pointFireActivo.position;
            posicionDisparo.y += (i - (balasPorRafaga - 1) / 2f) * separacionVerticalBalas;

            Vector2 direccion = transform.right;

            GameObject nuevaBala = Instantiate(balaPrefabActiva, posicionDisparo, Quaternion.identity);

            Bala balaScript = nuevaBala.GetComponent<Bala>();
            if (balaScript != null)
            {
                balaScript.SetConfiguracion(direccion, damageActivo, radioDanoActivo, false);
            }
        }
    }

    private void VerificarArmasEquipadas()
    {
        if (gunsSystem != null)
        {
            int armasEquipadas = 0;
            foreach (var slot in gunsSystem.moduleSlots)
            {
                if (slot.hasWeapon)
                {
                    armasEquipadas++;
                }
            }

            activoDisponible = armasEquipadas > 0;

            if (buttonSpriteActivo != null)
            {
                buttonSpriteActivo.gameObject.SetActive(activoDisponible);
            }
        }
    }

    public void ActualizarDisponibilidad()
    {
        VerificarArmasEquipadas();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);

        if (pointFirePasivo != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pointFirePasivo.position, 0.2f);
            Gizmos.DrawLine(transform.position, pointFirePasivo.position);
        }

        if (pointFireActivo != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(pointFireActivo.position, radioDanoActivo);
        }

        Gizmos.color = isShortRange ? Color.red : Color.blue;
        Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, isShortRange ? "ShortRange" : "LongRange");
    }

    // Métodos de debugging
    [ContextMenu("Test Disparo Pasivo")]
    public void TestDisparoPasivo()
    {
        DisparoPasivo();
    }

    [ContextMenu("Verificar Estado Arma")]
    public void VerificarEstadoArma()
    {
        Debug.Log($"=== {gameObject.name} ===");
        Debug.Log($"Arma equipada: {EstaArmaEquipada()}");
        Debug.Log($"Point Fire Pasivo: {pointFirePasivo != null}");
        Debug.Log($"Bala Prefab: {balaPrefabPasiva != null}");
        Debug.Log($"Enemigos cercanos: {BuscarEnemigoCercano() != null}");
    }
}