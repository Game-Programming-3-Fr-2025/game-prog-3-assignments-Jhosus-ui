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
    public Transform pointFirePasivo;

    [Header("=== ESTADO ACTIVO ===")]
    [Header("Configuración UI")]
    public string nombreBotonActivo = "BotonHabilidadActiva"; // NOMBRE PERSONALIZABLE
    [System.NonSerialized] public Button buttonSpriteActivo; // Ahora se busca por nombre
    public bool activoDisponible = false;

    [Header("Configuración Ataque Activo")]
    public int rafagaCantidad = 3;
    public float tiempoEntreRafagas = 0.2f;
    public int balasPorRafaga = 3;
    public float separacionVerticalBalas = 0.5f;
    public GameObject balaPrefabActiva;
    public float damageActivo = 25f;
    public float radioDanoActivo = 2f;
    public string nombrePointFire = "Point"; // Buscar por nombre

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
        // BUSCAR REFERENCIAS AUTOMÁTICAMENTE
        BuscarReferencias();

        if (buttonSpriteActivo != null)
        {
            buttonSpriteActivo.onClick.AddListener(ActivarModoActivo);
            buttonSpriteActivo.gameObject.SetActive(false);
        }

        VerificarArmasEquipadas();
    }

    // BUSCAR TODAS LAS REFERENCIAS NECESARIAS
    private void BuscarReferencias()
    {
        gameManager = FindObjectOfType<GameManager>();
        gunsSystem = FindObjectOfType<Guns>();

        // Buscar Point Fire Activo por nombre
        pointFireActivo = BuscarObjetoPorNombre(nombrePointFire);

        // Buscar Botón UI por nombre
        buttonSpriteActivo = BuscarBotonPorNombre(nombreBotonActivo);
    }

    // BUSCAR OBJETO POR NOMBRE EN LA ESCENA
    private Transform BuscarObjetoPorNombre(string nombre)
    {
        GameObject obj = GameObject.Find(nombre);
        if (obj != null)
        {
            Debug.Log($"Point encontrado: {obj.name}");
            return obj.transform;
        }

        Debug.LogWarning($"No se encontró objeto: {nombre}");
        return null;
    }

    // BUSCAR BOTÓN UI POR NOMBRE EN LA ESCENA
    private Button BuscarBotonPorNombre(string nombre)
    {
        Button[] todosBotones = FindObjectsOfType<Button>(true);

        foreach (Button boton in todosBotones)
        {
            if (boton.name == nombre)
            {
                Debug.Log($"Botón encontrado: {boton.name}");
                return boton;
            }
        }

        Debug.LogWarning($"No se encontró botón: {nombre}");
        return null;
    }

    // TODO EL RESTO DEL CÓDIGO PERMANECE EXACTAMENTE IGUAL
    void Update()
    {
        VerificarArmasEquipadas();

        if (buttonSpriteActivo != null)
        {
            buttonSpriteActivo.interactable = activoDisponible && !enCooldown && !modoActivo;
        }

        if (!modoActivo && Time.time >= nextFireTime)
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

    private void DisparoPasivo()
    {
        if (balaPrefabPasiva == null || pointFirePasivo == null) return;

        GameObject enemigoCercano = BuscarEnemigoCercano();

        if (enemigoCercano != null)
        {
            Vector2 direccion = (enemigoCercano.transform.position - pointFirePasivo.position).normalized;
            GameObject nuevaBala = Instantiate(balaPrefabPasiva, pointFirePasivo.position, Quaternion.identity);

            Bala balaScript = nuevaBala.GetComponent<Bala>();
            if (balaScript != null)
            {
                balaScript.SetConfiguracion(direccion, damagePasivo, radioDanoPasivo, isShortRange);
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
    }

    private void DesactivarModoActivo()
    {
        modoActivo = false;
        enCooldown = true;
        nextCooldownTime = Time.time + cooldownModoActivo;
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
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pointFirePasivo.position, radioDanoPasivo);
        }

        if (pointFireActivo != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(pointFireActivo.position, radioDanoActivo);
        }

        Gizmos.color = isShortRange ? Color.red : Color.blue;
        Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, isShortRange ? "ShortRange" : "LongRange");
    }
}