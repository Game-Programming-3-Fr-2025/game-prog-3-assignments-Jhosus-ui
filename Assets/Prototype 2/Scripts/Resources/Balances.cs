using UnityEngine;

public class Balances : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidadRotacion = 10f;
    public float maxAngulo = 30f;
    public float fuerzaResorte = 5f;

    [Header("Puntos de Balance")]
    public Transform puntoIzquierdo;
    public Transform puntoDerecho;
    public float radioDeteccion = 1.5f;

    private Rigidbody2D rb;
    private float anguloObjetivo = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezePosition;
        }
    }

    void FixedUpdate()
    {
        // Contar objetos en cada lado
        int pesoIzquierda = ContarPeso(puntoIzquierdo);
        int pesoDerecha = ContarPeso(puntoDerecho);

        // Calcular balance
        if (pesoIzquierda == 0 && pesoDerecha == 0)
        {
            // Volver al centro
            anguloObjetivo = Mathf.Lerp(anguloObjetivo, 0f, Time.fixedDeltaTime * fuerzaResorte);
        }
        else
        {
            // Calcular inclinación
            float diferencia = pesoDerecha - pesoIzquierda;
            anguloObjetivo = Mathf.Clamp(diferencia, -1f, 1f) * maxAngulo;
        }

        // Aplicar rotación suave
        Quaternion rotacion = Quaternion.Euler(0, 0,
            Mathf.LerpAngle(rb.rotation, anguloObjetivo, Time.fixedDeltaTime * velocidadRotacion));
        rb.MoveRotation(rotacion);
    }

    int ContarPeso(Transform punto)
    {
        if (punto == null) return 0;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(punto.position, radioDeteccion);
        int contador = 0;

        foreach (var col in colliders)
        {
            // Ignorar la plataforma misma y triggers
            if (col.gameObject != gameObject && !col.isTrigger && col.attachedRigidbody != null)
            {
                contador++;
            }
        }
        return contador;
    }

    void OnDrawGizmosSelected()
    {
        if (puntoIzquierdo != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(puntoIzquierdo.position, radioDeteccion);
        }

        if (puntoDerecho != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(puntoDerecho.position, radioDeteccion);
        }
    }
}