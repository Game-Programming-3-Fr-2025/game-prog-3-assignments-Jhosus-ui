using UnityEngine;

public class Enemy3 : MonoBehaviour
{
    public float velocidad = 2f;
    public int vida = 30;

    void Start()
    {
        // ASIGNAR TAG AL ENEMIGO AUTOMÁTICAMENTE
        gameObject.tag = "Enemy";
    }

    void Update()
    {
        transform.Translate(Vector3.left * velocidad * Time.deltaTime);
    }

    public void RecibirDanio(int cantidad)
    {
        vida -= cantidad;
        if (vida <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }

        // DETECTAR BALAS
        if (other.CompareTag("Bullet"))
        {
            Bala bala = other.GetComponent<Bala>();
            if (bala != null)
            {
                // Aquí necesitamos obtener el daño de la bala
                RecibirDanio(10); // Daño temporal
                Destroy(other.gameObject); // Destruir bala
            }
        }
    }
}