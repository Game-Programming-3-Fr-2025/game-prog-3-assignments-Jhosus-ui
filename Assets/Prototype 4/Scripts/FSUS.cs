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
    public string upgradeButtonName = "UpgradeButton";
    public string upgradeCostTextName = "UpgradeCostText";
    private Button upgradeButton;
    private TextMeshProUGUI upgradeCostText;
    public int upgradeLevel = 0;
    public int maxUpgradeLevel = 4;
    public float damageIncreasePerLevel = 5f;
    public int upgradeBaseCost = 20;
    public int upgradeIncrement = 10;
    public int energyCost = 1;

    private float nextFireTime = 0f;
    private float nextDanoCortoTime = 0f;
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

        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(UpgradeWeapon);
        }

        if (buttonSpriteActivo != null)
        {
            buttonSpriteActivo.onClick.AddListener(ActivarModoActivo);
        }

        // Actualizar estado inicial de la UI
        ActualizarUI();
        Debug.Log($"FSUS iniciado - Tipo: {(isShortRange ? "CORTO ALCANCE" : "LARGO ALCANCE")}");
    }

    private void BuscarReferencias()
    {
        gameManager = FindObjectOfType<GameManager>();
        gunsSystem = FindObjectOfType<Guns>();
        buttonSpriteActivo = BuscarBotonPorNombre(nombreBotonActivo);
        upgradeButton = BuscarBotonPorNombre(upgradeButtonName);
        upgradeCostText = BuscarTextoPorNombre(upgradeCostTextName);

        Debug.Log($"Referencias buscadas - Botón activo: {buttonSpriteActivo != null}, Botón mejora: {upgradeButton != null}, Texto costo: {upgradeCostText != null}");
    }

    private TextMeshProUGUI BuscarTextoPorNombre(string nombre)
    {
        // Búsqueda más robusta en toda la escena
        TextMeshProUGUI[] todosTextos = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (TextMeshProUGUI texto in todosTextos)
        {
            if (texto.name == nombre && texto.isActiveAndEnabled)
            {
                Debug.Log($"Texto encontrado: {texto.name} en objeto {texto.transform.parent.name}");
                return texto;
            }
        }
        Debug.LogWarning($"No se encontró el texto: {nombre}");
        return null;
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
        return obj != null ? obj.transform : null;
    }

    private Button BuscarBotonPorNombre(string nombre)
    {
        Button[] todosBotones = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button boton in todosBotones)
        {
            if (boton.name == nombre) return boton;
        }
        return null;
    }

    void Update()
    {
        // Actualizar UI en cada frame para mantener consistencia
        ActualizarUI();

        VerificarArmasEquipadas();

        // Funcionamiento pasivo cuando no está en modo activo
        if (!modoActivo && EstaArmaEquipada() && MoneyManager.Instance.isPlaying)
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

    // NUEVO MÉTODO: Actualizar toda la UI consistentemente
    private void ActualizarUI()
    {
        bool estaEquipada = EstaArmaEquipada();
        bool enJuego = MoneyManager.Instance.isPlaying;

        // REGLA CLARA:
        // - Mejoras: visibles solo cuando esta arma está equipada Y NO estamos en juego
        // - Ataque activo: visible solo cuando esta arma está equipada Y estamos en juego

        // Actualizar botón de mejora
        if (upgradeButton != null)
        {
            upgradeButton.gameObject.SetActive(estaEquipada && !enJuego && upgradeLevel < maxUpgradeLevel);
        }

        // Actualizar texto de costo de mejora
        if (upgradeCostText != null)
        {
            upgradeCostText.gameObject.SetActive(estaEquipada && !enJuego && upgradeLevel < maxUpgradeLevel);
            if (upgradeCostText.gameObject.activeInHierarchy)
            {
                upgradeCostText.text = GetUpgradeCost().ToString();
            }
        }

        // Actualizar botón activo
        if (buttonSpriteActivo != null)
        {
            buttonSpriteActivo.gameObject.SetActive(estaEquipada && enJuego);
            buttonSpriteActivo.interactable = activoDisponible && !enCooldown && !modoActivo && MoneyManager.Instance.energy >= energyCost;
        }
    }

    // CORREGIDO: Daño por área para armas cortas con mejoras aplicadas
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
                    // APLICAR MEJORAS AL DAÑO PASIVO
                    float danoFinal = damagePasivo + (upgradeLevel * damageIncreasePerLevel);
                    enemy.RecibirDanio((int)danoFinal);
                    enemigosAfectados++;
                }
            }
        }

        if (enemigosAfectados > 0)
        {
            Debug.Log($"{gameObject.name} aplicó daño de área a {enemigosAfectados} enemigos con daño: {damagePasivo + (upgradeLevel * damageIncreasePerLevel)}");
        }
    }

    // CORREGIDO: Disparo pasivo con mejoras aplicadas
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
                // APLICAR MEJORAS AL DAÑO PASIVO
                float danoFinal = damagePasivo + (upgradeLevel * damageIncreasePerLevel);
                balaScript.SetConfiguracion(direccion, danoFinal, radioDanoPasivo, false);
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

    // CORREGIDO: Verificación más precisa de si esta arma está equipada
    private bool EstaArmaEquipada()
    {
        if (gunsSystem == null)
        {
            gunsSystem = FindObjectOfType<Guns>();
            if (gunsSystem == null) return false;
        }

        foreach (var slot in gunsSystem.moduleSlots)
        {
            if (slot.hasWeapon && slot.weaponInstance != null)
            {
                FSUS armaScript = slot.weaponInstance.GetComponent<FSUS>();
                // Comparación más robusta
                if (armaScript != null && armaScript == this)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // CORREGIDO: Modo activo con mejoras aplicadas y funciona para corto alcance
    public void ActivarModoActivo()
    {
        if (modoActivo || !activoDisponible || enCooldown) return;

        if (MoneyManager.Instance.SpendEnergy(energyCost))
        {
            modoActivo = true;
            tiempoFinActivo = Time.time + duracionModoActivo;

            if (isShortRange)
            {
                // NUEVO: Modo activo para armas de corto alcance
                StartCoroutine(ModoActivoCortoAlcance());
            }
            else
            {
                // Modo activo original para largo alcance
                StartCoroutine(DispararRafagas());
            }

            if (buttonSpriteActivo != null)
            {
                buttonSpriteActivo.interactable = false;
            }

            Debug.Log($"Modo activo iniciado: {(isShortRange ? "CORTO ALCANCE" : "LARGO ALCANCE")}");
        }
    }

    // NUEVO: Corrutina para modo activo de corto alcance
    private IEnumerator ModoActivoCortoAlcance()
    {
        float tiempoEntreAtaques = 0.3f; // Ataques más frecuentes en modo activo

        while (modoActivo && Time.time < tiempoFinActivo)
        {
            // Aplicar daño de área más potente y frecuente
            Collider2D[] enemigos = Physics2D.OverlapCircleAll(transform.position, radioDanoActivo);
            int enemigosAfectados = 0;

            foreach (Collider2D colision in enemigos)
            {
                if (colision.CompareTag("Enemy"))
                {
                    Enemy3 enemy = colision.GetComponent<Enemy3>();
                    if (enemy != null)
                    {
                        // APLICAR MEJORAS AL DAÑO ACTIVO
                        float danoFinal = damageActivo + (upgradeLevel * damageIncreasePerLevel);
                        enemy.RecibirDanio((int)danoFinal);
                        enemigosAfectados++;
                    }
                }
            }

            if (enemigosAfectados > 0)
            {
                Debug.Log($"Modo activo corto alcance: {enemigosAfectados} enemigos dañados con {damageActivo + (upgradeLevel * damageIncreasePerLevel)} de daño");
            }

            yield return new WaitForSeconds(tiempoEntreAtaques);
        }
    }

    private void DesactivarModoActivo()
    {
        modoActivo = false;
        enCooldown = true;
        nextCooldownTime = Time.time + cooldownModoActivo;
        Debug.Log("Modo activo desactivado - Entrando en cooldown");
    }

    // CORREGIDO: Rafagas con mejoras aplicadas
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
                // APLICAR MEJORAS AL DAÑO ACTIVO
                float danoFinal = damageActivo + (upgradeLevel * damageIncreasePerLevel);
                balaScript.SetConfiguracion(direccion, danoFinal, radioDanoActivo, false);
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

            // Forzar actualización de UI cuando cambia el estado
            ActualizarUI();
        }
    }

    // CORREGIDO: Mejoras ahora afectan TANTO modo pasivo COMO activo
    private void UpgradeWeapon()
    {
        int cost = GetUpgradeCost();
        if (MoneyManager.Instance.SpendMoney(cost) && upgradeLevel < maxUpgradeLevel)
        {
            upgradeLevel++;
            // Las mejoras se aplican dinámicamente en los cálculos de daño
            Debug.Log($"Arma mejorada! Nivel {upgradeLevel} - Daño pasivo: {damagePasivo + (upgradeLevel * damageIncreasePerLevel)} - Daño activo: {damageActivo + (upgradeLevel * damageIncreasePerLevel)}");

            // Actualizar UI después de mejorar
            ActualizarUI();
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

            // Gizmo para área activa (más grande)
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, radioDanoActivo);
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