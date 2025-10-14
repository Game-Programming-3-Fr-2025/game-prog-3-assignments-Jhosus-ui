using UnityEngine;

public class Bala7 : MonoBehaviour
{
    [Header("Configuración de Bala")]
    public float velocidad = 10f;
    public float tiempoVida = 3f;

    [HideInInspector]
    public Vector2 direccion;

    private float damage; // Damage ahora es privado y se setea desde Combate7
    private float tiempoCreada;

    void Start()
    {
        tiempoCreada = Time.time;
    }

    void Update()
    {
        // Movimiento de la bala
        transform.Translate(direccion * velocidad * Time.deltaTime);

        // Destruir bala después de cierto tiempo
        if (Time.time - tiempoCreada >= tiempoVida)
        {
            Destroy(gameObject);
        }
    }

    // Método para que Combate7 pueda setear el damage
    public void SetDamage(float nuevoDamage)
    {
        damage = nuevoDamage;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy7 enemy = other.GetComponent<Enemy7>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}