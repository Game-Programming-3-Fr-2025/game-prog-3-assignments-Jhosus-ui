using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal; // Para Light2D

public class Starts : MonoBehaviour
{
    [Header("Configuración Inicio")]
    public float tiempoEspera = 0.3f; // Tiempo mínimo antes de permitir comenzar

    [Header("Temporizador Juego")]
    public float tiempoTotalJuego = 60f; // Tiempo total en segundos
    public bool usarTemporizador = true; // Activar/desactivar temporizador

    [Header("Luces de Advertencia")]
    public Light2D luzAdvertenciaJugador1; // Luz para jugador 1
    public Light2D luzAdvertenciaJugador2; // Luz para jugador 2
    public float tiempoAdvertencia = 10f; // Cuántos segundos antes mostrar luz
    public float intensidadMaxima = 1f; // Intensidad máxima de la luz
    public Color colorAdvertencia = Color.red; // Color de advertencia

    [Header("Referencias")]
    public ScoreManager scoreManager; // Referencia al ScoreManager

    private bool puedeComenzar = false;
    private bool juegoActivo = false;
    private float tiempoRestante;
    private float tiempoTranscurrido = 0f;

    void Start()
    {
        // Pausar todo inmediatamente
        Time.timeScale = 0f;

        // Inicializar temporizador
        tiempoRestante = tiempoTotalJuego;

        // Configurar luces
        ConfigurarLuces();

        // Esperar un momento (para que cargue todo)
        StartCoroutine(PrepararInicio());
    }

    void ConfigurarLuces()
    {
        if (luzAdvertenciaJugador1 != null)
        {
            luzAdvertenciaJugador1.enabled = false;
            luzAdvertenciaJugador1.color = colorAdvertencia;
            luzAdvertenciaJugador1.intensity = 0f;
        }

        if (luzAdvertenciaJugador2 != null)
        {
            luzAdvertenciaJugador2.enabled = false;
            luzAdvertenciaJugador2.color = colorAdvertencia;
            luzAdvertenciaJugador2.intensity = 0f;
        }
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
        // Lógica de inicio del juego
        if (!juegoActivo && puedeComenzar && Input.anyKeyDown)
        {
            IniciarJuego();
        }

        // Lógica del temporizador durante el juego
        if (juegoActivo && usarTemporizador)
        {
            ActualizarTemporizador();
            ControlarLucesAdvertencia();
        }
    }

    void IniciarJuego()
    {
        Time.timeScale = 1f;
        puedeComenzar = false;
        juegoActivo = true;
        tiempoTranscurrido = 0f;

        Debug.Log("¡Juego comenzado! Tiempo: " + tiempoTotalJuego + " segundos");

        // No desactivamos el script completamente porque necesitamos el temporizador
    }

    void ActualizarTemporizador()
    {
        tiempoTranscurrido += Time.deltaTime;
        tiempoRestante = tiempoTotalJuego - tiempoTranscurrido;

        // Verificar si se acabó el tiempo
        if (tiempoRestante <= 0f)
        {
            FinalizarJuego();
        }
    }

    void ControlarLucesAdvertencia()
    {
        // Si queda poco tiempo, activar luces gradualmente
        if (tiempoRestante <= tiempoAdvertencia)
        {
            float progresoLuz = 1f - (tiempoRestante / tiempoAdvertencia);
            float intensidadActual = Mathf.Lerp(0f, intensidadMaxima, progresoLuz);

            if (luzAdvertenciaJugador1 != null)
            {
                luzAdvertenciaJugador1.enabled = true;
                luzAdvertenciaJugador1.intensity = intensidadActual;
            }

            if (luzAdvertenciaJugador2 != null)
            {
                luzAdvertenciaJugador2.enabled = true;
                luzAdvertenciaJugador2.intensity = intensidadActual;
            }
        }
    }

    void FinalizarJuego()
    {
        juegoActivo = false;

        // Apagar luces de advertencia
        if (luzAdvertenciaJugador1 != null)
            luzAdvertenciaJugador1.enabled = false;

        if (luzAdvertenciaJugador2 != null)
            luzAdvertenciaJugador2.enabled = false;

        // Llamar al ScoreManager para mostrar resultados
        if (scoreManager != null)
        {
            // Forzar que ambos jugadores "mueran" para activar los resultados
            // O llamar directamente al método de resultados
            scoreManager.gameEnded = true;
            scoreManager.StartCoroutine(scoreManager.GameOverSequence());
        }
        else
        {
            Debug.LogWarning("ScoreManager no asignado. Buscando automáticamente...");
            scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.gameEnded = true;
                scoreManager.StartCoroutine(scoreManager.GameOverSequence());
            }
        }

        Debug.Log("¡Tiempo terminado! Mostrando resultados...");

        // Pausar el juego (opcional)
        // Time.timeScale = 0f;
    }

    // Métodos públicos para acceder al estado del juego
    public float GetTiempoRestante() => tiempoRestante;
    public bool IsJuegoActivo() => juegoActivo;
    public float GetTiempoTranscurrido() => tiempoTranscurrido;

    // Método para añadir tiempo extra (por power-ups, etc.)
    public void AñadirTiempo(float tiempoExtra)
    {
        tiempoRestante += tiempoExtra;
        Debug.Log("Tiempo añadido: +" + tiempoExtra + " segundos");
    }

    // Método para reiniciar el temporizador
    public void ReiniciarTemporizador()
    {
        tiempoTranscurrido = 0f;
        tiempoRestante = tiempoTotalJuego;

        // Apagar luces
        ConfigurarLuces();
    }
}