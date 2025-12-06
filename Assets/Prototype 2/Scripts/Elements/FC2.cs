using UnityEngine;

public class FC2 : MonoBehaviour
{
    [Header("Configuración Cámara")]
    public float velocidadBajada = 2f;
    public float velocidadDerecha = 2f;
    public float velocidadSubida = 2f;
    public float posicionXFija = 0f;

    [Header("Estado Cámara")]
    public bool modoHorizontal = false;
    public bool modoVerticalArriba = false;

    private float alturaInicial;
    private float posicionXInicial;
    private Transform jugador;
    private bool seguirJugadorHorizontal = false;
    private float tiempoInicioHorizontal;
    private float tiempoInicioVerticalArriba;
    private Camera camaraComponent;
    private float mitadAlturaCamara;
    private float mitadAnchoCamara;
    private float posicionYFijaHorizontal;
    private float posicionYInicialArriba;
    private float posicionXFijaArriba;

    void Start()
    {
        alturaInicial = transform.position.y;
        posicionXInicial = transform.position.x;

        camaraComponent = GetComponent<Camera>();
        if (camaraComponent != null)
        {
            mitadAlturaCamara = camaraComponent.orthographicSize;
            mitadAnchoCamara = camaraComponent.orthographicSize * camaraComponent.aspect;
        }
        else
        {
            mitadAlturaCamara = 5f;
            mitadAnchoCamara = 8f;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player 1");
        if (playerObj != null)
        {
            jugador = playerObj.transform;
        }
    }

    void Update()
    {
        if (modoVerticalArriba)
        {
            MoverCamaraVerticalArriba();
        }
        else if (modoHorizontal)
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
        float nuevaY = posicionYFijaHorizontal;

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

    void MoverCamaraVerticalArriba()
    {
        // CORREGIDO: Usar Time.time - tiempoInicio, igual que en MoverCamaraHorizontal
        float tiempoVertical = Time.time - tiempoInicioVerticalArriba;
        float nuevaY = posicionYInicialArriba + (velocidadSubida * tiempoVertical);

        Vector3 posicion = new Vector3(
            posicionXFijaArriba,
            nuevaY,
            transform.position.z
        );

        transform.position = posicion;
    }

    public void CambiarAModoHorizontal(Vector3 posicionAjuste)
    {
        if (!modoHorizontal)
        {
            modoHorizontal = true;
            modoVerticalArriba = false;
            posicionXInicial = transform.position.x;
            tiempoInicioHorizontal = Time.time;

            // Ajustar Y para que el BORDE INFERIOR esté en posicionAjuste.y
            posicionYFijaHorizontal = posicionAjuste.y + mitadAlturaCamara;

            Vector3 nuevaPosicion = new Vector3(transform.position.x, posicionYFijaHorizontal, transform.position.z);
            transform.position = nuevaPosicion;

            Debug.Log($"Horizontal: Ajustado a Y={posicionYFijaHorizontal} (borde inferior en {posicionAjuste.y})");
        }
    }

    public void CambiarAModoVerticalArriba(Vector3 puntoAjuste)
    {
        if (modoHorizontal && !modoVerticalArriba)
        {
            modoHorizontal = false;
            modoVerticalArriba = true;

            // Guardar la posición Y ACTUAL como punto de inicio
            posicionYInicialArriba = transform.position.y;

            // Inicializar el tiempo de inicio
            tiempoInicioVerticalArriba = Time.time;

            
            // El centro de la cámara debe estar a mitadAnchoCamara a la IZQUIERDA del borde derecho
            posicionXFijaArriba = puntoAjuste.x - mitadAnchoCamara;

            // Aplicar la posición inmediatamente
            Vector3 nuevaPosicion = new Vector3(
                posicionXFijaArriba,
                posicionYInicialArriba,
                transform.position.z
            );
            transform.position = nuevaPosicion;

            Debug.Log($"Vertical Arriba: Y={posicionYInicialArriba}, Centro X={posicionXFijaArriba}, Borde derecho en {puntoAjuste.x} (mitad ancho={mitadAnchoCamara})");
        }
    }

    public void CambiarAModoVertical()
    {
        if (modoHorizontal || modoVerticalArriba)
        {
            modoHorizontal = false;
            modoVerticalArriba = false;
            alturaInicial = transform.position.y;
            Debug.Log("Cámara cambió a modo vertical");
        }
    }

    // Métodos originales
    public void SetSeguirJugador(bool seguir) => seguirJugadorHorizontal = seguir;
    public void SetVelocidadBajada(float nuevaVelocidad) => velocidadBajada = nuevaVelocidad;
    public void SetVelocidadDerecha(float nuevaVelocidad) => velocidadDerecha = nuevaVelocidad;
    public void SetVelocidadSubida(float nuevaVelocidad) => velocidadSubida = nuevaVelocidad;
}