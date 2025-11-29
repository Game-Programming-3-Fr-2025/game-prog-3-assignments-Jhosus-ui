using UnityEngine;

public class TriggerHorizontal : MonoBehaviour
{
    [Header("Ajuste de Cámara")]
    public bool usarPuntoAjuste = true;
    public Transform puntoAjusteCamara; // Arrastra aquí el objeto que marca la altura deseada

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player 1"))
        {
            FC2 camara = FindObjectOfType<FC2>();
            if (camara != null)
            {
                if (usarPuntoAjuste && puntoAjusteCamara != null)
                {
                    // Usar la posición del punto de ajuste
                    camara.CambiarAModoHorizontal(puntoAjusteCamara.position);
                    Debug.Log($"Cámara ajustada usando punto: {puntoAjusteCamara.name}");
                }
                else
                {
                    // Usar la posición actual del trigger
                    camara.CambiarAModoHorizontal(transform.position);
                    Debug.Log("Cámara ajustada usando posición del trigger");
                }
            }

            gameObject.SetActive(false);
        }
    }
}