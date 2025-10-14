using UnityEngine;

public class Combate7 : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public GameObject balaPrefab;
    public float cadenciaDisparo = 0.5f;
    public float damage = 10f;
    public float rangoDeteccion = 10f;

    [Header("Punto de Disparo")]
    public Transform puntoDisparo;

    private float tiempoUltimoDisparo;
    private Transform enemigoCercano;

    void Start()
    {
        if (puntoDisparo == null)
            puntoDisparo = transform;
    }

    void Update()
    {
        BuscarEnemigoCercano();

        if (Time.time - tiempoUltimoDisparo >= cadenciaDisparo && enemigoCercano != null)
        {
            Disparar();
            tiempoUltimoDisparo = Time.time;
        }
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

    void Disparar()
    {
        if (balaPrefab == null) return;

        GameObject bala = Instantiate(balaPrefab, puntoDisparo.position, Quaternion.identity);

        Bala7 scriptBala = bala.GetComponent<Bala7>();
        if (scriptBala != null)
        {
            // Pasar el damage actual al crear la bala
            scriptBala.SetDamage(damage);

            Vector2 direccion = (enemigoCercano.position - puntoDisparo.position).normalized;
            scriptBala.direccion = direccion;
        }
    }

    // Método para que el GameManager pueda aumentar el damage
    public void AumentarDamage(float aumento)
    {
        damage += aumento;
        Debug.Log($"Damage aumentado a: {damage}");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}