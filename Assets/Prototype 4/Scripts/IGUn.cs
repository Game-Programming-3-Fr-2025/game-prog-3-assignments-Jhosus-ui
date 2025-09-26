using UnityEngine;

public class IGUn : MonoBehaviour
{
    [Header("Configuración Disparo")]
    public float fireRate = 1f, range = 5f, damage = 10f; // Disparos por segundo, Rango de detección, Daño por bala
    public GameObject balaPrefab; // Prefab de la bala
    public Transform pointFire; // Punto de origen del disparo

    [Header("Debug")]
    public bool mostrarDebug = false;

    private float nextFireTime = 0f;

    void Start()
    {
        if (pointFire == null) // Crear pointFire automáticamente si no existe
        {
            pointFire = new GameObject("PointFire").transform;
            pointFire.SetParent(transform);
            pointFire.localPosition = new Vector3(0.5f, 0, 0);
        }
        Debug.Log("IGUn inicializado - Disparo automático activado");
    }

    void Update()
    {
        if (Time.time >= nextFireTime) // Disparar automáticamente cada intervalo
        {
            Disparar();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    private void Disparar()
    {
        if (balaPrefab == null || pointFire == null) return;

        GameObject enemigo = BuscarEnemigoCercano(); // Buscar enemigo más cercano
        if (enemigo != null)
        {
            Vector2 direccion = (enemigo.transform.position - pointFire.position).normalized; // Calcular dirección hacia el enemigo
            GameObject bala = Instantiate(balaPrefab, pointFire.position, Quaternion.identity); // Crear bala
            Bala balaScript = bala.GetComponent<Bala>(); // Configurar bala
            if (balaScript != null) balaScript.SetConfiguracion(direccion, damage, 0f, false);
            if (mostrarDebug) Debug.Log($"IGUn disparó a: {enemigo.name}");
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
        Gizmos.color = Color.yellow; // Mostrar rango de disparo
        Gizmos.DrawWireSphere(transform.position, range);
        if (pointFire != null) // Mostrar punto de disparo
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pointFire.position, 0.1f);
        }
    }
}