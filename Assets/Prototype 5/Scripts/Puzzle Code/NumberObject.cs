using UnityEngine;
using UnityEngine.UI;

public class NumberObject : MonoBehaviour
{
    public int number; 
    public Image numberImageUI; 

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.pickup);

            if (numberImageUI != null)
            {
                numberImageUI.gameObject.SetActive(true); // Mostrar en UI
            }

            gameObject.SetActive(false); // Ocultar del mundo
        }
    }
}