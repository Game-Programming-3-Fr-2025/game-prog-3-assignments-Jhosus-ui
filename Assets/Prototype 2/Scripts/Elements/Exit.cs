using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image fadeImage;

    [Header("Configuración de Escena")]
    [SerializeField] private string nombreSiguienteEscena;
    [SerializeField] private string exitID; // Identificador único de esta salida

    [Header("Configuración de Fade")]
    [SerializeField] private float duracionFadeIn = 0.5f;
    [SerializeField] private float duracionOscuro = 1f;

    private bool jugadorEnRango;
    private bool enTransicion;

    void Start()
    {
        ConfigurarVisuales(0f);
    }

    void Update()
    {
        bool inputInteraccion = Input.GetKeyDown(KeyCode.E) ||
                              (Gamepad.current != null && Gamepad.current.yButton.wasPressedThisFrame);

        if (jugadorEnRango && !enTransicion && inputInteraccion)
            StartCoroutine(RealizarTransicion());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = false;
        }
    }

    private IEnumerator RealizarTransicion()
    {
        if (string.IsNullOrEmpty(nombreSiguienteEscena)) yield break;

        enTransicion = true;

        // Guardar información del exit antes de cambiar de escena
        PlayerPrefs.SetString("LastExitID", exitID);
        PlayerPrefs.Save();

        // Oscurecer
        yield return AnimarFade(0f, 1f, duracionFadeIn);

        // Mantener oscuro
        yield return new WaitForSeconds(duracionOscuro);

        // Cambiar de escena
        SceneManager.LoadScene(nombreSiguienteEscena);
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
        SetearAlphaFade(alpha);
    }
}