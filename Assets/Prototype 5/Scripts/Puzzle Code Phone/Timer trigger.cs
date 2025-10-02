using UnityEngine;

public class TimerTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Cuando el jugador entra al trigger
        {
            CodeRSecret phone = FindObjectOfType<CodeRSecret>(); // Buscar teléfono en escena
            if (phone != null)
            {
                phone.StartCodeTimer(); 
            }
        }
    }
}