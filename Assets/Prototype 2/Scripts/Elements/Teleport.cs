using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform destination;
    public FC2 cameraController;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (destination != null)
            {
                // Teleportar al jugador
                other.transform.position = destination.position;

                // Reiniciar la l�nea de muerte si hay una c�mara configurada
                if (cameraController != null)
                {
                    // Necesitar�amos agregar un m�todo p�blico en FC2 para reiniciar la altura
                    // Por ahora, simplemente forzamos un reinicio llamando al m�todo ResetLevel
                    // cameraController.ResetLevel(); // Descomenta si agregas este m�todo a FC2
                }
            }
        }
    }


    void OnDrawGizmos()
    {
        if (destination != null)
        {
            // Dibujar el punto de destino
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(destination.position, new Vector3(1, 1, 0.1f));

            // Dibujar una l�nea desde este objeto al destino
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, destination.position);
        }
    }
}