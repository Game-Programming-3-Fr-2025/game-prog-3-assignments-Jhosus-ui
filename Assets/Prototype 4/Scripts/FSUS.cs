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
    public float fireRate = 1f, range = 5f, damagePasivo = 10f, radioDanoPasivo = 1f, intervaloDanoCorto = 1f;
    public GameObject balaPrefabPasiva;
    public Transform pointFirePasivo;

    [Header("=== ESTADO ACTIVO ===")]
    [Header("Configuración UI")]
    public string nombreBotonActivo = "BotonHabilidadActiva";
    [System.NonSerialized] public Button buttonSpriteActivo;
    public bool activoDisponible = false;

    [Header("Configuración Ataque Activo")]
    public int rafagaCantidad = 3, balasPorRafaga = 3;
    public float tiempoEntreRafagas = 0.2f, separacionVerticalBalas = 0.5f, damageActivo = 25f, radioDanoActivo = 2f;
    public GameObject balaPrefabActiva;
    public string nombrePointFire = "Point";

    [Header("Tiempos Activo")]
    public float duracionModoActivo = 5f, cooldownModoActivo = 10f;

    [Header("Mejoras armas")]
    public string upgradeButtonName = "UpgradeButton"; // Buscar por nombre
    private Button upgradeButton;
    public TextMeshPro upgradeCostText; // Arrastrar objeto con TextMeshPro (3D)
    public int upgradeLevel = 0, maxUpgradeLevel = 4, upgradeBaseCost = 20, upgradeIncrement = 10, energyCost = 1;
    public float damageIncreasePerLevel = 5f;

    private float nextFireTime = 0f, nextDanoCortoTime = 0f, tiempoFinActivo = 0f, nextCooldownTime = 0f;
    private bool modoActivo = false, enCooldown = false;
    private Transform pointFireActivo;
    private GameManager gameManager;
    private Guns gunsSystem;

    void Start()
    {
        BuscarReferencias();
        ConfigurarPointsFire();
        if (upgradeButton != null) // Configurar botón de mejora si se encontró
        {
            upgradeButton.onClick.AddListener(UpgradeWeapon);
            ActualizarUIUpgrade();
        }
        if (buttonSpriteActivo != null)
        {
            buttonSpriteActivo.onClick.AddListener(ActivarModoActivo);
            buttonSpriteActivo.gameObject.SetActive(false);
        }
        if (MoneyManager.Instance != null) MoneyManager.Instance.onResetToLobby += OnResetToLobby; // Suscribirse al evento de reset
        VerificarArmasEquipadas();
        Debug.Log($"FSUS iniciado - Tipo: {(isShortRange ? "CORTO ALCANCE" : "LARGO ALCANCE")}");
    }

    void OnDestroy()
    {
        if (MoneyManager.Instance != null) MoneyManager.Instance.onResetToLobby -= OnResetToLobby;
    }

    private void BuscarReferencias()
    {
        gameManager = FindObjectOfType<GameManager>();
        gunsSystem = FindObjectOfType<Guns>();
        if (buttonSpriteActivo == null) buttonSpriteActivo = BuscarBotonPorNombre(nombreBotonActivo); // Necesiito buscar el botón por nombre   
        if (upgradeButton == null) upgradeButton = BuscarBotonPorNombre(upgradeButtonName);
    }

    private Button BuscarBotonPorNombre(string nombre)
    {
        Button[] todosBotones = FindObjectsOfType<Button>(true);
        foreach (Button boton in todosBotones)
            if (boton.name == nombre) return boton;
        return null;
    }

    void Update()
    {
        if (MoneyManager.Instance != null && !MoneyManager.Instance.isPlaying) return;

        ActualizarUIUpgrade();
        VerificarArmasEquipadas();
        if (buttonSpriteActivo != null) buttonSpriteActivo.interactable = activoDisponible && !enCooldown && !modoActivo;

        if (!modoActivo && EstaArmaEquipada())
        {
            if (isShortRange)
            {
                if (Time.time >= nextDanoCortoTime)
                {
                    AplicarDanoAreaCorto();
                    nextDanoCortoTime = Time.time + intervaloDanoCorto;
                }
            }
            else if (Time.time >= nextFireTime)
            {
                DisparoPasivo();
                nextFireTime = Time.time + 1f / fireRate;
            }
        }

        if (modoActivo && Time.time >= tiempoFinActivo) DesactivarModoActivo();
        if (enCooldown && Time.time >= nextCooldownTime) enCooldown = false;
    }

    private void ActualizarUIUpgrade()
    {
        if (upgradeButton != null && MoneyManager.Instance != null) // Actualizar visibilidad del botón de mejora
        {
            bool puedeMejorar = upgradeLevel < maxUpgradeLevel;
            bool enLobby = !MoneyManager.Instance.isPlaying;
            upgradeButton.gameObject.SetActive(puedeMejorar && enLobby);
            upgradeButton.interactable = puedeMejorar && enLobby;
        }
        if (upgradeCostText != null) // Actualizar texto de costo (solo si está asignado)
        {
            upgradeCostText.text = GetUpgradeCost().ToString();
            upgradeCostText.gameObject.SetActive(upgradeLevel < maxUpgradeLevel);
        }
    }

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
        if (enemigosAfectados > 0) Debug.Log($"{gameObject.name} aplicó daño de área a {enemigosAfectados} enemigos");
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
            if (balaScript != null) balaScript.SetConfiguracion(direccion, damagePasivo, radioDanoPasivo, false);
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
            if (slot.hasWeapon && slot.weaponInstance != null)
            {
                FSUS armaScript = slot.weaponInstance.GetComponent<FSUS>();
                if (armaScript == this) return true;
            }
        return false;
    }

    public void ActivarModoActivo()
    {
        if (modoActivo || !activoDisponible || MoneyManager.Instance == null) return;
        if (MoneyManager.Instance.SpendEnergy(energyCost))
        {
            modoActivo = true;
            tiempoFinActivo = Time.time + duracionModoActivo;
            StartCoroutine(DispararRafagas());
            if (buttonSpriteActivo != null) buttonSpriteActivo.interactable = false;
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
            if (balaScript != null) balaScript.SetConfiguracion(direccion, damageActivo, radioDanoActivo, false);
        }
    }

    private void VerificarArmasEquipadas()
    {
        if (gunsSystem != null)
        {
            int armasEquipadas = 0;
            foreach (var slot in gunsSystem.moduleSlots)
                if (slot.hasWeapon) armasEquipadas++;
            activoDisponible = armasEquipadas > 0;
            if (buttonSpriteActivo != null) buttonSpriteActivo.gameObject.SetActive(activoDisponible);
        }
    }

    private void UpgradeWeapon()
    {
        int cost = GetUpgradeCost();
        if (MoneyManager.Instance != null && MoneyManager.Instance.SpendMoney(cost) && upgradeLevel < maxUpgradeLevel)
        {
            upgradeLevel++;
            damagePasivo += damageIncreasePerLevel;
            damageActivo += damageIncreasePerLevel;
            ActualizarUIUpgrade();
        }
    }

    private int GetUpgradeCost() => upgradeBaseCost + (upgradeLevel * upgradeIncrement);

    private void OnResetToLobby()
    {
        modoActivo = false;
        enCooldown = false;
        nextFireTime = 0f;
        nextDanoCortoTime = 0f;
        StopAllCoroutines();
        if (buttonSpriteActivo != null) buttonSpriteActivo.interactable = true;
        ActualizarUIUpgrade();
        Debug.Log($"FSUS resetado - modoActivo: {modoActivo}, enCooldown: {enCooldown}");
    }

    private void ConfigurarPointsFire()
    {
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
        return obj?.transform;
    }

    private void OnDrawGizmosSelected()
    {
        if (isShortRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radioDanoPasivo);
        }
        else
        {
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