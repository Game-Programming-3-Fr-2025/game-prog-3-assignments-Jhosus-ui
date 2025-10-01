using UnityEngine;

public class KeyRoom : MonoBehaviour
{
    public GameObject keyImageUI; // Image UI que se activa al obtener la llave

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Activar el Image UI de la llave
            if (keyImageUI != null)
            {
                keyImageUI.SetActive(true);
            }

            // Ocultar la llave del mundo
            gameObject.SetActive(false);

            Debug.Log("¡Llave obtenida!");
        }
    }
}