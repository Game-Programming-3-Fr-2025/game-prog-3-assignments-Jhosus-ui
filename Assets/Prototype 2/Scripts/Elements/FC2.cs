using UnityEngine;

public class FC2 : MonoBehaviour
{
    [Header("Configuración Cámara")]
    public float velocidadBajada = 2f;
    public float posicionXFija = 0f;

    private float alturaInicial;

    void Start()
    {
        // Guardar la altura inicial de la cámara
        alturaInicial = transform.position.y;
    }

    void Update()
    {
        // Calcular la nueva posición Y (bajada constante desde altura inicial)
        float nuevaY = alturaInicial - (velocidadBajada * Time.time);

        // Forzar posición fija en X e Y independiente del jugador
        Vector3 posicion = new Vector3(
            posicionXFija,    // X fija
            nuevaY,           // Y con bajada constante
            transform.position.z
        );

        transform.position = posicion;
    }

    // Método para cambiar velocidad
    public void SetVelocidadBajada(float nuevaVelocidad)
    {
        velocidadBajada = nuevaVelocidad;
    }

    // Método para cambiar posición X fija
    public void SetPosicionXFija(float nuevaPosicionX)
    {
        posicionXFija = nuevaPosicionX;
    }
}