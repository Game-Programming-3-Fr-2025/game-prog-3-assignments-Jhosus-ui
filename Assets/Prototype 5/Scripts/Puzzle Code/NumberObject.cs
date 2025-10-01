using UnityEngine;
using UnityEngine.UI;

public class NumberObject : MonoBehaviour
{
    public int number; // N�mero que representa (0-9)
    public Image numberImageUI; // Image UI que muestra este n�mero

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Activar el Image UI que muestra el n�mero
            if (numberImageUI != null)
            {
                numberImageUI.gameObject.SetActive(true);
            }

            // Ocultar el objeto del mundo
            gameObject.SetActive(false);

            Debug.Log($"N�mero {number} recolectado");
        }
    }
}