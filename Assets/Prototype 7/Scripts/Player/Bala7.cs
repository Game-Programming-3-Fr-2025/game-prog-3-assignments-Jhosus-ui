using UnityEngine;

public class Bala7 : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float velocidad = 10f;
    [SerializeField] private float tiempoVida = 3f;

    [HideInInspector] public Vector2 direccion;

    private float damage;
    private float creationTime;

    void Start()
    {
        creationTime = Time.time;
    }

    void Update()
    {
        transform.Translate(direccion * velocidad * Time.deltaTime);

        if (Time.time - creationTime >= tiempoVida)
            Destroy(gameObject);
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy7>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}