using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Combate7 : MonoBehaviour
{
    [Header("Shoot Settings")]
    [SerializeField] private GameObject balaPrefab;
    [SerializeField] private Transform puntoDisparo;
    [SerializeField] private float cadenciaDisparo = 1.8f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float rangoDeteccion = 4f;

    [Header("Critical System")]
    [SerializeField] private float critChance = 6f;
    [SerializeField] private float critMultiplier = 2f;

    [Header("Area Damage")]
    [SerializeField] private float areaDamage = 0f;
    [SerializeField] private float areaRange = 2f;
    [SerializeField] private float areaCooldown = 2f;

    [Header("Lights")]
    [SerializeField] private Light2D areaLight;
    [SerializeField] private Light2D visionLight;

    private float lastShootTime;
    private float lastAreaDamageTime;
    private Transform nearestEnemy;

    void Start()
    {
        if (!puntoDisparo)
            puntoDisparo = transform;

        UpdateLights();
    }

    void Update()
    {
        FindNearestEnemy();
        AutoShoot();
        ApplyAreaDamage();
    }

    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float minDistance = Mathf.Infinity;
        nearestEnemy = null;

        foreach (var enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance && distance <= rangoDeteccion)
            {
                minDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }
    }

    void AutoShoot()
    {
        if (Time.time - lastShootTime >= cadenciaDisparo && nearestEnemy)
        {
            Shoot();
            lastShootTime = Time.time;
        }
    }

    void Shoot()
    {
        if (!balaPrefab) return;

        GameObject bullet = Instantiate(balaPrefab, puntoDisparo.position, Quaternion.identity);
        Bala7 bulletScript = bullet.GetComponent<Bala7>();

        if (bulletScript)
        {
            float finalDamage = damage;

            if (Random.Range(0f, 100f) <= critChance)
                finalDamage *= critMultiplier;

            bulletScript.SetDamage(finalDamage);
            bulletScript.direccion = (nearestEnemy.position - puntoDisparo.position).normalized;
        }
    }

    void ApplyAreaDamage()
    {
        if (areaDamage <= 0 || Time.time - lastAreaDamageTime < areaCooldown) return;

        Collider2D[] enemiesInArea = Physics2D.OverlapCircleAll(transform.position, areaRange);

        foreach (var col in enemiesInArea)
        {
            if (col.CompareTag("Enemy"))
                col.GetComponent<Enemy7>()?.TakeDamage(areaDamage);
        }

        lastAreaDamageTime = Time.time;
    }

    void UpdateLights()
    {
        if (visionLight)
            visionLight.pointLightOuterRadius = rangoDeteccion;

        if (areaLight)
        {
            areaLight.pointLightOuterRadius = areaRange;
            areaLight.gameObject.SetActive(areaDamage > 0);
        }
    }

    public void ActualizarVelocidadDisparo(float newRate)
    {
        cadenciaDisparo = newRate;
    }

    public void ActualizarAreaDisparo(float newRange)
    {
        rangoDeteccion = newRange;
        UpdateLights();
    }

    public void ActualizarCritico(float newCritChance)
    {
        critChance = newCritChance;
    }

    public void ActualizarDamageArea(float newDamage, float newRange)
    {
        areaDamage = newDamage;
        areaRange = newRange;
        UpdateLights();
    }
}