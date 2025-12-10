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
        Time.timeScale = 0f;
        tiempoRestante = tiempoTotalJuego;

        if (luzAdvertenciaJugador1 != null) luzAdvertenciaJugador1.enabled = false;
        if (luzAdvertenciaJugador2 != null) luzAdvertenciaJugador2.enabled = false;
    }

    void Update()
    {
        if (!juegoActivo && Input.anyKeyDown)
        {
            Time.timeScale = 1f;
            juegoActivo = true;
        }

        if (juegoActivo)
        {
            tiempoRestante -= Time.deltaTime;

            if (tiempoRestante <= tiempoAdvertencia)
            {
                float intensidad = 1f - (tiempoRestante / tiempoAdvertencia);
                ActualizarLuzAdvertencia(luzAdvertenciaJugador1, intensidad);
                ActualizarLuzAdvertencia(luzAdvertenciaJugador2, intensidad);
            }

            if (tiempoRestante <= 0f)
                FinalizarJuego();
        }
    }

    void ActualizarLuzAdvertencia(Light2D luz, float intensidad)
    {
        if (luz != null)
        {
            luz.enabled = true;
            luz.intensity = intensidad;
            luz.color = colorAdvertencia;
        }
    }

    void FinalizarJuego()
    {
        juegoActivo = false;
        if (luzAdvertenciaJugador1 != null) luzAdvertenciaJugador1.enabled = false;
        if (luzAdvertenciaJugador2 != null) luzAdvertenciaJugador2.enabled = false;

        if (scoreManager != null)
        {
            scoreManager.gameEnded = true;
            StartCoroutine(scoreManager.GameOverSequence());
        }
    }

    public float GetTiempoRestante() => tiempoRestante;
    public bool IsJuegoActivo() => juegoActivo;
}