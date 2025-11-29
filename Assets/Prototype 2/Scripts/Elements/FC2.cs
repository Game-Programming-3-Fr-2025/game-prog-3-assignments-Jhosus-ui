using UnityEngine;

public class FC2 : MonoBehaviour
{
    [Header("Configuración Cámara")]
    public float velocidadBajada = 2f;
    public float velocidadDerecha = 2f;
    public float posicionXFija = 0f;

    [Header("Estado Cámara")]
    public bool modoHorizontal = false;

    private float alturaInicial;
    private float posicionXInicial;
    private Transform jugador;
    private bool seguirJugadorHorizontal = false;
    private float tiempoInicioHorizontal;
    private Camera camaraComponent;
    private float mitadAlturaCamara; // Mitad de la altura de la cámara
    private float posicionYFijaHorizontal;

    void Start()
    {
        alturaInicial = transform.position.y;
        posicionXInicial = transform.position.x;

        camaraComponent = GetComponent<Camera>();
        if (camaraComponent != null)
        {
            // Calcular la mitad de la altura de la cámara
            mitadAlturaCamara = camaraComponent.orthographicSize;
        }
        else
        {
            mitadAlturaCamara = 5f; // Valor por defecto
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player 1");
        if (playerObj != null)
        {
            jugador = playerObj.transform;
        }
    }

    void Update()
    {
        if (modoHorizontal)
        {
            MoverCamaraHorizontal();
        }
        else
        {
            MoverCamaraVertical();
        }
    }

    void MoverCamaraVertical()
    {
        float nuevaY = alturaInicial - (velocidadBajada * Time.time);

        Vector3 posicion = new Vector3(
            posicionXFija,
            nuevaY,
            transform.position.z
        );

        transform.position = posicion;
    }

    void MoverCamaraHorizontal()
    {
        float nuevaX;
        float nuevaY = posicionYFijaHorizontal; // Siempre usar Y fija

        if (seguirJugadorHorizontal && jugador != null)
        {
            nuevaX = Mathf.Lerp(transform.position.x, jugador.position.x, Time.deltaTime * velocidadDerecha);
        }
        else
        {
            float tiempoHorizontal = Time.time - tiempoInicioHorizontal;
            nuevaX = posicionXInicial + (velocidadDerecha * tiempoHorizontal);
        }

        Vector3 posicion = new Vector3(nuevaX, nuevaY, transform.position.z);
        transform.position = posicion;
    }

    public void CambiarAModoHorizontal(Vector3 posicionAjuste)
    {
        if (!modoHorizontal)
        {
            modoHorizontal = true;
            posicionXInicial = transform.position.x;
            tiempoInicioHorizontal = Time.time;

            // Calcular la posición Y para que el BORDE INFERIOR de la cámara esté en posicionAjuste.y
            // Centro de la cámara = posición del borde inferior + mitad de la altura
            posicionYFijaHorizontal = posicionAjuste.y + mitadAlturaCamara;

            // Aplicar la nueva posición inmediatamente
            Vector3 nuevaPosicion = new Vector3(transform.position.x, posicionYFijaHorizontal, transform.position.z);
            transform.position = nuevaPosicion;

            Debug.Log($"Borde inferior de cámara posicionado en Y: {posicionAjuste.y}");
        }
    }

    public void CambiarAModoHorizontal()
    {
        CambiarAModoHorizontal(transform.position);
    }

    public void CambiarAModoVertical()
    {
        if (modoHorizontal)
        {
            modoHorizontal = false;
            alturaInicial = transform.position.y;
            Debug.Log("Cámara cambió a modo vertical");
        }
    }

    public void SetSeguirJugador(bool seguir)
    {
        seguirJugadorHorizontal = seguir;
    }

    public void SetVelocidadBajada(float nuevaVelocidad)
    {
        velocidadBajada = nuevaVelocidad;
    }

    public void SetVelocidadDerecha(float nuevaVelocidad)
    {
        velocidadDerecha = nuevaVelocidad;
    }
}