using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Escaleras : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TextMeshPro textoInteraccion;
    [SerializeField] private Image fadeImage;
    [SerializeField] private Transform puntoDestino;

    [Header("Configuración")]
    [SerializeField] private float duracionFadeIn = 0.5f;
    [SerializeField] private float duracionOscuro = 1f;
    [SerializeField] private float duracionFadeOut = 1.5f;

    private bool jugadorEnRango;
    private bool enTransicion;
    private Transform jugador;

    void Start()
    {
        ConfigurarVisuales(0f);
    }

    void Update()
    {
        ActualizarTextoInteraccion();

        if (jugadorEnRango && !enTransicion && Input.GetKeyDown(KeyCode.E))
            StartCoroutine(RealizarTransicion());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = true;
            jugador = other.transform;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = false;
            jugador = null;
        }
    }

    private void ActualizarTextoInteraccion()
    {
        if (textoInteraccion != null)
            textoInteraccion.alpha = (jugadorEnRango && !enTransicion) ? 1f : 0f;
    }

    private IEnumerator RealizarTransicion()
    {
        if (puntoDestino == null || jugador == null) yield break;

        enTransicion = true;

        // Oscurecer
        yield return AnimarFade(0f, 1f, duracionFadeIn);

        // Mantener oscuro
        yield return new WaitForSeconds(duracionOscuro);

        // Teletransportar
        jugador.position = puntoDestino.position;

        // Aclarar
        yield return AnimarFade(1f, 0f, duracionFadeOut);

        // Resetear
        ConfigurarVisuales(0f);
        enTransicion = false;
        jugadorEnRango = false;
    }

    private IEnumerator AnimarFade(float inicio, float fin, float duracion)
    {
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            SetearAlphaFade(Mathf.Lerp(inicio, fin, tiempo / duracion));
            yield return null;
        }

        SetearAlphaFade(fin);
    }

    private void SetearAlphaFade(float alpha)
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }

    private void ConfigurarVisuales(float alpha)
    {
        if (textoInteraccion != null)
            textoInteraccion.alpha = alpha;

        SetearAlphaFade(alpha);
    }
}