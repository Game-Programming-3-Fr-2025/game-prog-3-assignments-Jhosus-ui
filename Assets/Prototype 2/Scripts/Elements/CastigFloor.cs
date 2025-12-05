using UnityEngine;
using System.Collections;

public class CastigFloor : MonoBehaviour
{
    [Header("Referencia Cámara")]
    [SerializeField] private Camera miCamara;

    [Header("Sprites de Amenaza")]
    [SerializeField] private SpriteRenderer arriba;
    [SerializeField] private SpriteRenderer izquierda;
    [SerializeField] private SpriteRenderer abajo;

    [Header("Transiciones")]
    [SerializeField] private float tiempoFadeOut = 0.5f;
    [SerializeField] private float tiempoEspera = 2f; // Tiempo entre desaparecer y aparecer
    [SerializeField] private float tiempoFadeIn = 0.5f;
    [SerializeField] private float tiempoEsperaEntreCambios = 20f;

    private int estado = 0;
    private float tiempoUltimoCambio = -100f;
    private bool estaTransicionando = false;

    void Start()
    {
        if (miCamara == null)
        {
            bool esPlayer1 = gameObject.CompareTag("Player 1");
            string nombreCam = esPlayer1 ? "MainCameraP1" : "MainCameraP2";
            GameObject camObj = GameObject.Find(nombreCam);
            if (camObj) miCamara = camObj.GetComponent<Camera>();
        }

        IniciarConSpriteArriba();
    }

    void IniciarConSpriteArriba()
    {
        SetAlpha(arriba, 0f);
        SetAlpha(izquierda, 0f);
        SetAlpha(abajo, 0f);

        if (arriba)
        {
            arriba.enabled = true;
            StartCoroutine(FadeIn(arriba, tiempoFadeIn));
        }

        estado = 0;
    }

    void Update()
    {
        if (miCamara == null) return;
        ActualizarPosiciones();
    }

    void ActualizarPosiciones()
    {
        Vector3 pos = miCamara.transform.position;
        float altura = miCamara.orthographicSize;
        float ancho = altura * miCamara.aspect;

        if (arriba) arriba.transform.position = new Vector3(pos.x, pos.y + altura, arriba.transform.position.z);
        if (izquierda) izquierda.transform.position = new Vector3(pos.x - ancho, pos.y, izquierda.transform.position.z);
        if (abajo) abajo.transform.position = new Vector3(pos.x, pos.y - altura, abajo.transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Horizontal") && !estaTransicionando)
        {
            if (Time.time - tiempoUltimoCambio > tiempoEsperaEntreCambios)
            {
                int siguienteEstado = (estado + 1) % 3;
                StartCoroutine(TransicionCompleta(siguienteEstado));
                tiempoUltimoCambio = Time.time;
            }
        }
    }

    IEnumerator TransicionCompleta(int nuevoEstado)
    {
        estaTransicionando = true;

        // 1. Ocultar sprite actual (Fade Out)
        SpriteRenderer spriteActual = GetSpriteActual();
        if (spriteActual != null)
        {
            yield return StartCoroutine(FadeOut(spriteActual, tiempoFadeOut));
            spriteActual.enabled = false;
        }

        // 2. ESPERAR X TIEMPO ANTES DE APARECER EL NUEVO
        yield return new WaitForSeconds(tiempoEspera);

        // 3. Mostrar nuevo sprite (Fade In)
        SpriteRenderer nuevoSprite = GetSpritePorEstado(nuevoEstado);
        if (nuevoSprite != null)
        {
            nuevoSprite.enabled = true;
            yield return StartCoroutine(FadeIn(nuevoSprite, tiempoFadeIn));
        }

        // 4. Actualizar estado
        estado = nuevoEstado;
        estaTransicionando = false;

        Debug.Log($"{gameObject.name}: Cambió a {GetNombreEstado(nuevoEstado)}");
    }

    IEnumerator FadeOut(SpriteRenderer sprite, float duracion)
    {
        if (sprite == null) yield break;

        float tiempo = 0f;
        Color colorOriginal = sprite.color;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;
            float alpha = Mathf.Lerp(colorOriginal.a, 0f, t);

            SetAlpha(sprite, alpha);
            yield return null;
        }

        SetAlpha(sprite, 0f);
    }

    IEnumerator FadeIn(SpriteRenderer sprite, float duracion)
    {
        if (sprite == null) yield break;

        float tiempo = 0f;
        SetAlpha(sprite, 0f); // Empezar invisible

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;
            float alpha = Mathf.Lerp(0f, 1f, t);

            SetAlpha(sprite, alpha);
            yield return null;
        }

        SetAlpha(sprite, 1f);
    }

    SpriteRenderer GetSpriteActual()
    {
        switch (estado)
        {
            case 0: return arriba;
            case 1: return izquierda;
            case 2: return abajo;
            default: return null;
        }
    }

    SpriteRenderer GetSpritePorEstado(int estadoIndex)
    {
        switch (estadoIndex)
        {
            case 0: return arriba;
            case 1: return izquierda;
            case 2: return abajo;
            default: return null;
        }
    }

    string GetNombreEstado(int estadoIndex)
    {
        return estadoIndex switch
        {
            0 => "ARRIBA",
            1 => "IZQUIERDA",
            2 => "ABAJO",
            _ => "DESCONOCIDO"
        };
    }

    void SetAlpha(SpriteRenderer sprite, float alpha)
    {
        if (sprite == null) return;

        Color color = sprite.color;
        color.a = alpha;
        sprite.color = color;
    }

    // Métodos para control manual
    public void CambiarAArriba() => StartCoroutine(TransicionCompleta(0));
    public void CambiarAIzquierda() => StartCoroutine(TransicionCompleta(1));
    public void CambiarAAbajo() => StartCoroutine(TransicionCompleta(2));
}