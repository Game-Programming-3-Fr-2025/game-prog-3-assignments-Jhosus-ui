using UnityEngine;
using System.Collections;

public class Starts : MonoBehaviour
{
    [Header("Configuración Simple")]
    public float tiempoEspera = 0.3f; // Tiempo mínimo de espera

    private bool puedeComenzar = false;

    void Start()
    {
        // Pausar todo inmediatamente
        Time.timeScale = 0f;

        // Esperar un momento (para que cargue todo)
        StartCoroutine(PrepararInicio());
    }

    IEnumerator PrepararInicio()
    {
        // Esperar en tiempo real (no afectado por Time.timeScale)
        yield return new WaitForSecondsRealtime(tiempoEspera);

        puedeComenzar = true;
        Debug.Log("Presiona cualquier tecla para comenzar...");
    }

    void Update()
    {
        if (puedeComenzar && Input.anyKeyDown)
        {
            Time.timeScale = 1f;
            puedeComenzar = false;
            Debug.Log("¡Juego comenzado!");

            // Desactivar este script para que no siga ejecutándose
            this.enabled = false;
        }
    }
}