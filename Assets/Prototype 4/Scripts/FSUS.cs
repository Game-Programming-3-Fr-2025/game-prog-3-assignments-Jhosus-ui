using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public float intervaloDanoCorto = 1f; // Cada cuánto aplica daño (solo corto alcance)

    [Header("Point Fire Pasivo")]
    [Tooltip("Solo necesario para armas de largo alcance")]
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

    [Header("Mejoras armas")]
    public string upgradeButtonName = "UpgradeButton"; // Nombre único del botón de mejora
    public string upgradeCostTextName = "UpgradeCostText"; // Nombre único del texto de costo
    private Button upgradeButton; // Ahora privado, se buscará por nombre
    private TextMeshProUGUI upgradeCostText; // Ahora privado, se buscará por nombre
    public int upgradeLevel = 0;
    public int maxUpgradeLevel = 4;
    public float damageIncreasePerLevel = 5f;
    public int upgradeBaseCost = 20;
    public int upgradeIncrement = 10;
    public int energyCost = 1; // Para activo

    private float nextFireTime = 0f;
    private float nextDanoCortoTime = 0f; // Timer para daño de corto alcance
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

        // Configurar botón de mejora si se encontró
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(UpgradeWeapon);
            upgradeButton.gameObject.SetActive(!MoneyManager.Instance.isPlaying && upgradeLevel < maxUpgradeLevel);
        }

        if (buttonSpriteActivo != null)
        {
            buttonSpriteActivo.onClick.AddListener(ActivarModoActivo);
            buttonSpriteActivo.gameObject.SetActive(false);
        }

        VerificarArmasEquipadas();
        Debug.Log($"FSUS iniciado - Tipo: {(isShortRange ? "CORTO ALCANCE" : "LARGO ALCANCE")}");
    }

    private void BuscarReferencias()
    {
        gameManager = FindObjectOfType<GameManager>();
        gunsSystem = FindObjectOfType<Guns>();
        buttonSpriteActivo = BuscarBotonPorNombre(nombreBotonActivo);

        // Buscar botón de mejora y texto de costo por nombre
        upgradeButton = BuscarBotonPorNombre(upgradeButtonName);
        upgradeCostText = BuscarTextoPorNombre(upgradeCostTextName);
    }

    private TextMeshProUGUI BuscarTextoPorNombre(string nombre)
    {
        TextMeshProUGUI[] todosTextos = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI texto in todosTextos)
        {
            if (texto.name == nombre) return texto;
        }
        return null;
    }

    private void ConfigurarPointsFire()
    {
        // Solo configurar point fire si es arma de largo alcance
        if (!isShortRange && pointFirePasivo == null)
        {
            pointFirePasivo = transform.Find("PointFirePasivo");
            if (pointFirePasivo == null)
            {
                GameObject newPoint = new GameObject("PointFirePasivo");
                newPoint.transform.SetParent(transform);
                newPoint.transform.localPosition = new Vector3(0.5f, 0, 0);
                pointFirePasivo = newPoint.transform;
            }
        }

        pointFireActivo = BuscarObjetoPorNombre(nombrePointFire);
        if (pointFireActivo == null) pointFireActivo = pointFirePasivo;
    }

    private Transform BuscarObjetoPorNombre(string nombre)
    {
        GameObject obj = GameObject.Find(nombre);
        return obj != null ? obj.transform : null;
    }

    private Button BuscarBotonPorNombre(string nombre)
    {
        Button[] todosBotones = FindObjectsOfType<Button>(true);
        foreach (Button boton in todosBotones)
        {
            if (boton.name == nombre) return boton;
        }
        return null;
    }

    void Update()
    {
        // Actualizar visibilidad del botón de mejora
        if (upgradeButton != null)
        {
            upgradeButton.gameObject.SetActive(!MoneyManager.Instance.isPlaying && upgradeLevel < maxUpgradeLevel);
        }

        // Actualizar texto de costo
        if (upgradeCostText != null)
        {
            upgradeCostText.text = GetUpgradeCost().ToString();
        }

        VerificarArmasEquipadas();

        if (buttonSpriteActivo != null)
        {
            buttonSpriteActivo.interactable = activoDisponible && !enCooldown && !modoActivo;
        }

        if (!modoActivo && EstaArmaEquipada())
        {
            if (isShortRange)
            {
                // ARMA CORTO ALCANCE: Daño por área cada X segundos
                if (Time.time >= nextDanoCortoTime)
                {
                    AplicarDanoAreaCorto();
                    nextDanoCortoTime = Time.time + intervaloDanoCorto;
                }
            }
            else
            {
                // ARMA LARGO ALCANCE: Disparar balas
                if (Time.time >= nextFireTime)
                {
                    DisparoPasivo();
                    nextFireTime = Time.time + 1f / fireRate;
                }
            }
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

    // MÉTODO NUEVO: Daño por área para armas cortas
    private void AplicarDanoAreaCorto()
    {
        Collider2D[] enemigos = Physics2D.OverlapCircleAll(transform.position, radioDanoPasivo);
        int enemigosAfectados = 0;

        foreach (Collider2D colision in enemigos)
        {
            if (colision.CompareTag("Enemy"))
            {
                Enemy3 enemy = colision.GetComponent<Enemy3>();
                if (enemy != null)
                {
                    enemy.RecibirDanio((int)damagePasivo);
                    enemigosAfectados++;
                }
            }
        }

        if (enemigosAfectados > 0)
        {
            Debug.Log($"{gameObject.name} aplicó daño de área a {enemigosAfectados} enemigos");
        }
    }

    // MÉTODO ORIGINAL: Disparo para armas largas
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
                balaScript.SetConfiguracion(direccion, damagePasivo, radioDanoPasivo, false);
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

    private bool EstaArmaEquipada()
    {
        if (gunsSystem == null) return false;

        foreach (var slot in gunsSystem.moduleSlots)
        {
            if (slot.hasWeapon && slot.weaponInstance != null)
            {
                FSUS armaScript = slot.weaponInstance.GetComponent<FSUS>();
                if (armaScript == this) return true;
            }
        }
        return false;
    }

    // Los métodos de MODO ACTIVO se mantienen igual
    public void ActivarModoActivo()
    {
        if (modoActivo || !activoDisponible) return;
        if (MoneyManager.Instance.SpendEnergy(energyCost))

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
                if (slot.hasWeapon) armasEquipadas++;
            }
            activoDisponible = armasEquipadas > 0;

            if (buttonSpriteActivo != null)
            {
                buttonSpriteActivo.gameObject.SetActive(activoDisponible);
            }
        }
    }

    private void UpgradeWeapon()
    {
        int cost = GetUpgradeCost();
        if (MoneyManager.Instance.SpendMoney(cost) && upgradeLevel < maxUpgradeLevel)
        {
            upgradeLevel++;
            damagePasivo += damageIncreasePerLevel;
            damageActivo += damageIncreasePerLevel;
        }
    }

    private int GetUpgradeCost()
    {
        return upgradeBaseCost + (upgradeLevel * upgradeIncrement);
    }

    private void OnDrawGizmosSelected()
    {
        // Gizmo para área de daño (corto alcance)
        if (isShortRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radioDanoPasivo);
        }
        else
        {
            // Gizmo para rango de disparo (largo alcance)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);

            if (pointFirePasivo != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(pointFirePasivo.position, 0.2f);
            }
        }

        if (pointFireActivo != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(pointFireActivo.position, 0.3f);
        }
    }
}