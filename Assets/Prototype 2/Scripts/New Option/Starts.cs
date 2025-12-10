using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class Starts : MonoBehaviour
{
    [Header("Temporizador")]
    public float tiempoTotalJuego = 60f;

    [Header("Luces de Advertencia")]
    public Light2D luzAdvertenciaJugador1;
    public Light2D luzAdvertenciaJugador2;
    public float tiempoAdvertencia = 10f;
    public Color colorAdvertencia = Color.red;

    [Header("Referencias")]
    public ScoreManager scoreManager;

    private bool juegoActivo = false;
    private float tiempoRestante;

    void Start()
    {
        Time.timeScale = 0f; // Pausar todo
        tiempoRestante = tiempoTotalJuego;

        // Apagar luces
        if (luzAdvertenciaJugador1 != null) luzAdvertenciaJugador1.enabled = false;
        if (luzAdvertenciaJugador2 != null) luzAdvertenciaJugador2.enabled = false;

        Debug.Log("Presiona cualquier tecla para comenzar...");
    }

    void Update()
    {
        // Esperar tecla para iniciar
        if (!juegoActivo && Input.anyKeyDown)
        {
            Time.timeScale = 1f;
            juegoActivo = true;
            Debug.Log("¡Juego iniciado!");
        }

        // Contar tiempo cuando está activo
        if (juegoActivo)
        {
            tiempoRestante -= Time.deltaTime;

            // Luces de advertencia
            if (tiempoRestante <= tiempoAdvertencia)
            {
                float intensidad = 1f - (tiempoRestante / tiempoAdvertencia);
                if (luzAdvertenciaJugador1 != null)
                {
                    luzAdvertenciaJugador1.enabled = true;
                    luzAdvertenciaJugador1.intensity = intensidad;
                    luzAdvertenciaJugador1.color = colorAdvertencia;
                }
                if (luzAdvertenciaJugador2 != null)
                {
                    luzAdvertenciaJugador2.enabled = true;
                    luzAdvertenciaJugador2.intensity = intensidad;
                    luzAdvertenciaJugador2.color = colorAdvertencia;
                }
            }

            // Fin del juego
            if (tiempoRestante <= 0f)
            {
                juegoActivo = false;
                if (luzAdvertenciaJugador1 != null) luzAdvertenciaJugador1.enabled = false;
                if (luzAdvertenciaJugador2 != null) luzAdvertenciaJugador2.enabled = false;

                if (scoreManager != null)
                {
                    scoreManager.gameEnded = true;
                    StartCoroutine(scoreManager.GameOverSequence());
                }

                Debug.Log("¡Tiempo terminado!");
            }
        }
    }

    // Métodos públicos útiles
    public float GetTiempoRestante() => tiempoRestante;
    public bool IsJuegoActivo() => juegoActivo;
}