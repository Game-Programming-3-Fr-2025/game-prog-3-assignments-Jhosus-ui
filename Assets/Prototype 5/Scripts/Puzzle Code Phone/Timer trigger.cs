using UnityEngine;

public class TimerTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Buscar el teléfono y activar su timer
            CodeRSecret phone = FindObjectOfType<CodeRSecret>();
            if (phone != null)
            {
                phone.StartCodeTimer();
            }
        }
    }
}