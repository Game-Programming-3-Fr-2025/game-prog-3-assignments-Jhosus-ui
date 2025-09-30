using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeLoopManager : MonoBehaviour
{
    // Singleton para fácil acceso
    public static TimeLoopManager Instance;

    // Duración del bucle en segundos
    public float loopDuration = 60f; // Ejemplo: 1 minuto

    // Tiempo actual dentro del bucle
    private float currentLoopTime;

    // Estado del bucle
    private bool isLoopActive = false;

    // Eventos para notificar a otros scripts (Muy importante para desacoplar el código)
    public System.Action OnLoopStart;
    public System.Action OnLoopEnd;
    public System.Action OnLoopReset;

    void Awake()
    {
        // Configurar el Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional: si quieres que persista entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartTimeLoop();
    }

    void Update()
    {
        if (!isLoopActive) return;

        // Actualizar el temporizador
        currentLoopTime += Time.deltaTime;

        // Comprobar si el bucle debe terminar
        if (currentLoopTime >= loopDuration)
        {
            EndTimeLoop();
        }
    }

    public void StartTimeLoop()
    {
        isLoopActive = true;
        currentLoopTime = 0f;
        Debug.Log("¡Bucle Temporal Iniciado!");

        // Notificar a todos los suscriptores que el bucle ha comenzado
        OnLoopStart?.Invoke();
    }

    private void EndTimeLoop()
    {
        isLoopActive = false;
        Debug.Log("¡Bucle Temporal Terminado! Reiniciando...");

        // Notificar que el bucle ha terminado (útil para efectos de transición)
        OnLoopEnd?.Invoke();

        // Iniciar el reinicio después de un pequeño delay (opcional)
        StartCoroutine(ResetLoopAfterDelay(1f));
    }

    private IEnumerator ResetLoopAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetTimeLoop();
    }

    public void ResetTimeLoop()
    {
        // Notificar a todos los objetos para que se reinicien
        OnLoopReset?.Invoke();

        // Reiniciar el tiempo y reactivar el bucle
        currentLoopTime = 0f;
        isLoopActive = true;
        Debug.Log("Mundo Reiniciado.");
    }

    // Método para que otros objetos sepan cuánto tiempo queda
    public float GetRemainingTime()
    {
        return Mathf.Max(loopDuration - currentLoopTime, 0f);
    }

    // Para activar el reinicio manualmente (útil para debug o si el jugador muere)
    public void ForceLoopReset()
    {
        ResetTimeLoop();
    }
}