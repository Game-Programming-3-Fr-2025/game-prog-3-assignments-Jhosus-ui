using UnityEngine;

public class Combate7 : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public GameObject balaPrefab;
    public float cadenciaDisparo = 1.8f;
    public float damage = 10f;
    public float rangoDeteccion = 4f;

    [Header("Punto de Disparo")]
    public Transform puntoDisparo;

    [Header("Sistema Crítico")]
    public float critChance = 6f; // % de probabilidad
    public float critMultiplier = 2f; // Multiplicador de daño crítico

    [Header("Sistema de Área")]
    public float areaDamage = 0f;
    public float areaRange = 2f;
    public float areaCooldown = 2f;

    [Header("Referencias Visuales")]
    public UnityEngine.Rendering.Universal.Light2D areaLight;
    public UnityEngine.Rendering.Universal.Light2D visionLight;

    private float tiempoUltimoDisparo;
    private float tiempoUltimoAreaDamage;
    private Transform enemigoCercano;
    private UPManager7 upManager;

    void Start()
    {
        if (puntoDisparo == null)
            puntoDisparo = transform;

        upManager = FindObjectOfType<UPManager7>();
        ActualizarLuces();
    }

    void Update()
    {
        BuscarEnemigoCercano();
        DispararAutomatico();
        AplicarDamageArea();
    }

    void BuscarEnemigoCercano()
    {
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");
        float distanciaMasCercana = Mathf.Infinity;
        enemigoCercano = null;

        foreach (GameObject enemigo in enemigos)
        {
            float distancia = Vector2.Distance(transform.position, enemigo.transform.position);
            if (distancia < distanciaMasCercana && distancia <= rangoDeteccion)
            {
                distanciaMasCercana = distancia;
                enemigoCercano = enemigo.transform;
            }
        }
    }

    void DispararAutomatico()
    {
        if (Time.time - tiempoUltimoDisparo >= cadenciaDisparo && enemigoCercano != null)
        {
            Disparar();
            tiempoUltimoDisparo = Time.time;
        }
    }

    void Disparar()
    {
        if (balaPrefab == null) return;

        GameObject bala = Instantiate(balaPrefab, puntoDisparo.position, Quaternion.identity);
        Bala7 scriptBala = bala.GetComponent<Bala7>();

        if (scriptBala != null)
        {
            float damageFinal = damage;
            bool esCritico = Random.Range(0f, 100f) <= critChance;

            if (esCritico)
            {
                damageFinal *= critMultiplier;
                Debug.Log("¡DISPARO CRÍTICO!");
            }

            scriptBala.SetDamage(damageFinal);
            Vector2 direccion = (enemigoCercano.position - puntoDisparo.position).normalized;
            scriptBala.direccion = direccion;
        }
    }

    void AplicarDamageArea()
    {
        if (areaDamage <= 0) return;

        if (Time.time - tiempoUltimoAreaDamage >= areaCooldown)
        {
            Collider2D[] enemigosEnArea = Physics2D.OverlapCircleAll(transform.position, areaRange);

            foreach (Collider2D col in enemigosEnArea)
            {
                if (col.CompareTag("Enemy"))
                {
                    Enemy7 enemy = col.GetComponent<Enemy7>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(areaDamage);
                        Debug.Log($"Daño de área: {areaDamage} a {col.name}");
                    }
                }
            }

            tiempoUltimoAreaDamage = Time.time;
        }
    }

    // Métodos para que UPManager actualice las stats
    public void ActualizarVelocidadDisparo(float nuevaCadencia)
    {
        cadenciaDisparo = nuevaCadencia;
    }

    public void ActualizarAreaDisparo(float nuevoRango)
    {
        rangoDeteccion = nuevoRango;
        ActualizarLuces();
    }

    public void ActualizarCritico(float nuevoCritChance)
    {
        critChance = nuevoCritChance;
    }

    public void ActualizarDamageArea(float nuevoAreaDamage, float nuevoAreaRange)
    {
        areaDamage = nuevoAreaDamage;
        areaRange = nuevoAreaRange;
        ActualizarLuces();
    }

    void ActualizarLuces()
    {
        // Actualizar luz de visión (rango de disparo)
        if (visionLight != null)
        {
            visionLight.pointLightOuterRadius = rangoDeteccion;
        }

        // Actualizar luz de área de daño
        if (areaLight != null)
        {
            areaLight.pointLightOuterRadius = areaRange;
            areaLight.gameObject.SetActive(areaDamage > 0);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Rango de detección (verde)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        // Área de daño (rojo)
        if (areaDamage > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, areaRange);
        }
    }
}