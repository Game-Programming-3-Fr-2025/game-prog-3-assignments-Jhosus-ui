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
    public bool seguirJugadorHorizontal = false;

    private Camera cam;
    private Transform jugador;
    private float alturaInicial;
    private float posicionXInicial;
    private float posicionYFijaHorizontal;
    private float posicionYInicialArriba;
    private float posicionXFijaArriba;
    private float tiempoInicioHorizontal;
    private float tiempoInicioVerticalArriba;
    private float tiempoInicioVertical;
    private float mitadAlturaCamara;
    private float mitadAnchoCamara;

    void Start()
    {
        alturaInicial = transform.position.y;
        posicionXInicial = transform.position.x;
        tiempoInicioVertical = Time.time;

        cam = GetComponent<Camera>();
        if (cam != null)
        {
            mitadAlturaCamara = cam.orthographicSize;
            mitadAnchoCamara = cam.orthographicSize * cam.aspect;
        }
        else
        {
            mitadAlturaCamara = 5f;
            mitadAnchoCamara = 8f;
        }

        string playerTag = transform.parent != null && transform.parent.CompareTag("Player 2") ? "Player 2" : "Player 1";
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null) jugador = playerObj.transform;
    }

    void LateUpdate()
    {
        if (modoVerticalArriba)
            MoverCamaraVerticalArriba();
        else if (modoHorizontal)
            MoverCamaraHorizontal();
        else
            MoverCamaraVertical();
    }

    void MoverCamaraVertical()
    {
        float tiempoTranscurrido = Time.time - tiempoInicioVertical;
        float nuevaY = alturaInicial - (velocidadBajada * tiempoTranscurrido);
        transform.position = new Vector3(posicionXFija, nuevaY, transform.position.z);
    }

    void MoverCamaraHorizontal()
    {
        float nuevaX = seguirJugadorHorizontal && jugador != null
            ? Mathf.Lerp(transform.position.x, jugador.position.x, Time.deltaTime * velocidadDerecha)
            : posicionXInicial + (velocidadDerecha * (Time.time - tiempoInicioHorizontal));

        transform.position = new Vector3(nuevaX, posicionYFijaHorizontal, transform.position.z);
    }

    void MoverCamaraVerticalArriba()
    {
        float tiempoTranscurrido = Time.time - tiempoInicioVerticalArriba;
        float nuevaY = posicionYInicialArriba + (velocidadSubida * tiempoTranscurrido);
        transform.position = new Vector3(posicionXFijaArriba, nuevaY, transform.position.z);
    }

    public void CambiarAModoHorizontal(Vector3 posicionAjuste)
    {
        if (!modoHorizontal)
        {
            modoHorizontal = true;
            modoVerticalArriba = false;
            posicionXInicial = transform.position.x;
            tiempoInicioHorizontal = Time.time;
            posicionYFijaHorizontal = posicionAjuste.y + mitadAlturaCamara;
            transform.position = new Vector3(transform.position.x, posicionYFijaHorizontal, transform.position.z);
        }
    }

    public void CambiarAModoVerticalArriba(Vector3 puntoAjuste)
    {
        if (modoHorizontal && !modoVerticalArriba)
        {
            modoHorizontal = false;
            modoVerticalArriba = true;
            posicionYInicialArriba = transform.position.y;
            tiempoInicioVerticalArriba = Time.time;
            posicionXFijaArriba = puntoAjuste.x - mitadAnchoCamara;
            transform.position = new Vector3(posicionXFijaArriba, posicionYInicialArriba, transform.position.z);
        }
    }

    public void CambiarAModoVertical()
    {
        if (modoHorizontal || modoVerticalArriba)
        {
            modoHorizontal = false;
            modoVerticalArriba = false;
            alturaInicial = transform.position.y;
            tiempoInicioVertical = Time.time;
        }
    }

    public void SetSeguirJugador(bool seguir) => seguirJugadorHorizontal = seguir;
    public void SetVelocidadBajada(float nuevaVelocidad) => velocidadBajada = nuevaVelocidad;
    public void SetVelocidadDerecha(float nuevaVelocidad) => velocidadDerecha = nuevaVelocidad;
    public void SetVelocidadSubida(float nuevaVelocidad) => velocidadSubida = nuevaVelocidad;
}