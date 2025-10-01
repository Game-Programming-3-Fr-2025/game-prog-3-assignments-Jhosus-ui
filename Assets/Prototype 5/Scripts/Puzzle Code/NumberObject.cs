using UnityEngine;
using UnityEngine.UI;

public class NumberObject : MonoBehaviour
{
    public int number; // Número que representa (0-9)
    public Image numberImageUI; // Image UI que muestra este número

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Activar el Image UI que muestra el número
            if (numberImageUI != null)
            {
                numberImageUI.gameObject.SetActive(true);
            }

            // Ocultar el objeto del mundo
            gameObject.SetActive(false);

            Debug.Log($"Número {number} recolectado");
        }
    }
}