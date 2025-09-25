using UnityEngine;

public class IGUn : MonoBehaviour
{
    [Header("Configuración Disparo")]
    public float fireRate = 1f;          // Disparos por segundo
    public float range = 5f;             // Rango de detección
    public float damage = 10f;           // Daño por bala
    public GameObject balaPrefab;        // Prefab de la bala
    public Transform pointFire;          // Punto de origen del disparo

    [Header("Debug")]
    public bool mostrarDebug = false;

    private float nextFireTime = 0f;

    void Start()
    {
        // Crear pointFire automáticamente si no existe
        if (pointFire == null)
        {
            pointFire = new GameObject("PointFire").transform;
            pointFire.SetParent(transform);
            pointFire.localPosition = new Vector3(0.5f, 0, 0);
        }

        Debug.Log("IGUn inicializado - Disparo automático activado");
    }

    void Update()
    {
        // Disparar automáticamente cada intervalo
        if (Time.time >= nextFireTime)
        {
            Disparar();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    private void Disparar()
    {
        if (balaPrefab == null || pointFire == null) return;

        // Buscar enemigo más cercano
        GameObject enemigo = BuscarEnemigoCercano();
        if (enemigo != null)
        {
            // Calcular dirección hacia el enemigo
            Vector2 direccion = (enemigo.transform.position - pointFire.position).normalized;

            // Crear bala
            GameObject bala = Instantiate(balaPrefab, pointFire.position, Quaternion.identity);

            // Configurar bala
            Bala balaScript = bala.GetComponent<Bala>();
            if (balaScript != null)
            {
                balaScript.SetConfiguracion(direccion, damage, 0f, false);
            }

            if (mostrarDebug)
            {
                Debug.Log($"IGUn disparó a: {enemigo.name}");
            }
        }
    }

    private GameObject BuscarEnemigoCercano()
    {
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject enemigoCercano = null;
        float distanciaMinima = Mathf.Infinity;

        foreach (GameObject enemigo in enemigos)
        {
            if (enemigo == null) continue;

            float distancia = Vector2.Distance(transform.position, enemigo.transform.position);
            if (distancia <= range && distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                enemigoCercano = enemigo;
            }
        }

        return enemigoCercano;
    }

    private void OnDrawGizmosSelected()
    {
        // Mostrar rango de disparo
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);

        // Mostrar punto de disparo
        if (pointFire != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pointFire.position, 0.1f);
        }
    }
}