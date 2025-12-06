using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class CastigFloor : MonoBehaviour
{
    [Header("Referencia Cámara")]
    [SerializeField] private Camera miCamara;

    [Header("Elementos de Amenaza (Orden: Arriba, Izquierda, Abajo)")]
    [SerializeField] private SpriteRenderer[] sprites = new SpriteRenderer[3];
    [SerializeField] private Light2D[] luces = new Light2D[3];

    [Header("Tiempos")]
    [SerializeField] private float tiempoFadeOut = 0.5f;
    [SerializeField] private float tiempoEspera = 2f;
    [SerializeField] private float tiempoFadeIn = 0.5f;
    [SerializeField] private float tiempoEsperaEntreCambios = 20f;

    private int estadoActual = 0;
    private float tiempoUltimoCambio = -100f;
    private bool estaTransicionando = false;

    void Start()
    {
        if (miCamara == null)
        {
            // Busca la cámara como hijo del jugador
            miCamara = GetComponentInChildren<Camera>();

            // Si no está como hijo, busca por tag
            if (miCamara == null)
            {
                string nombreCam = gameObject.CompareTag("Player 1") ? "MainCameraP1" : "MainCameraP2";
                GameObject camObj = GameObject.Find(nombreCam);
                if (camObj) miCamara = camObj.GetComponent<Camera>();
            }
        }

        InicializarEstado();
    }

    void InicializarEstado()
    {
        for (int i = 0; i < 3; i++)
        {
            SetAlpha(i, 0f);
            if (sprites[i]) sprites[i].enabled = (i == 0);
            if (luces[i]) luces[i].enabled = (i == 0);
        }

        StartCoroutine(Fade(0, 0f, 1f, tiempoFadeIn));
    }

    void Update()
    {
        if (miCamara == null) return;

        float altura = miCamara.orthographicSize;
        float ancho = altura * miCamara.aspect;
        Vector3 pos = miCamara.transform.position;
        Vector3[] posiciones = {
            new Vector3(pos.x, pos.y + altura, 0),
            new Vector3(pos.x - ancho, pos.y, 0),
            new Vector3(pos.x, pos.y - altura, 0)
        };

        for (int i = 0; i < 3; i++)
        {
            if (sprites[i]) sprites[i].transform.position = posiciones[i] + Vector3.forward * sprites[i].transform.position.z;
            if (luces[i]) luces[i].transform.position = posiciones[i] + Vector3.forward * luces[i].transform.position.z;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Horizontal") && !estaTransicionando &&
            Time.time - tiempoUltimoCambio > tiempoEsperaEntreCambios)
        {
            CambiarEstado((estadoActual + 1) % 3);
            tiempoUltimoCambio = Time.time;
        }
    }

    void CambiarEstado(int nuevoEstado)
    {
        StartCoroutine(TransicionEstado(nuevoEstado));
    }

    IEnumerator TransicionEstado(int nuevoEstado)
    {
        estaTransicionando = true;

        // Fade Out actual
        yield return Fade(estadoActual, 1f, 0f, tiempoFadeOut);
        if (sprites[estadoActual]) sprites[estadoActual].enabled = false;
        if (luces[estadoActual]) luces[estadoActual].enabled = false;

        // Espera
        yield return new WaitForSeconds(tiempoEspera);

        // Fade In nuevo
        if (sprites[nuevoEstado]) sprites[nuevoEstado].enabled = true;
        if (luces[nuevoEstado]) luces[nuevoEstado].enabled = true;
        yield return Fade(nuevoEstado, 0f, 1f, tiempoFadeIn);

        estadoActual = nuevoEstado;
        estaTransicionando = false;
    }

    IEnumerator Fade(int indice, float alphaInicio, float alphaFin, float duracion)
    {
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            SetAlpha(indice, Mathf.Lerp(alphaInicio, alphaFin, t / duracion));
            yield return null;
        }
        SetAlpha(indice, alphaFin);
    }

    void SetAlpha(int indice, float alpha)
    {
        if (sprites[indice])
        {
            Color c = sprites[indice].color;
            c.a = alpha;
            sprites[indice].color = c;
        }
        if (luces[indice])
        {
            Color c = luces[indice].color;
            c.a = alpha;
            luces[indice].color = c;
        }
    }

    // Métodos públicos para control manual
    public void CambiarAArriba() => CambiarEstado(0);
    public void CambiarAIzquierda() => CambiarEstado(1);
    public void CambiarAAbajo() => CambiarEstado(2);
}