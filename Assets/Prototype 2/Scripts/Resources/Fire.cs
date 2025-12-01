using UnityEngine;
using System.Collections;

public class Fire : MonoBehaviour
{
    [Header("Configuración")]
    public float tiempoEntreFrames = 0.1f;
    public float escalaInicial = 0.1f;
    public float escalaFinal = 1f;
    public float tiempoCrecimiento = 0.2f;

    private SpriteRenderer[] sprites;
    private int frameActual = 0;

    void Start()
    {
        // Obtener sprites de los hijos
        sprites = GetComponentsInChildren<SpriteRenderer>();

        // Ocultar todos al inicio
        foreach (var sprite in sprites)
            sprite.enabled = false;

        // Iniciar animación
        StartCoroutine(AnimacionFuego());
    }

    IEnumerator AnimacionFuego()
    {
        while (true)
        {
            // Ocultar frame anterior
            if (frameActual > 0)
                sprites[frameActual - 1].enabled = false;

            // Mostrar y animar frame actual
            var spriteActual = sprites[frameActual];
            spriteActual.enabled = true;
            StartCoroutine(EscalarSprite(spriteActual.transform));

            // Avanzar frame
            frameActual = (frameActual + 1) % sprites.Length;

            // Esperar para siguiente frame
            yield return new WaitForSeconds(tiempoEntreFrames);
        }
    }

    IEnumerator EscalarSprite(Transform spriteTransform)
    {
        float tiempo = 0;
        Vector3 escalaIni = Vector3.one * escalaInicial;
        Vector3 escalaFin = Vector3.one * escalaFinal;

        // Animación de escala
        while (tiempo < tiempoCrecimiento)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / tiempoCrecimiento;
            spriteTransform.localScale = Vector3.Lerp(escalaIni, escalaFin, t);
            yield return null;
        }

        spriteTransform.localScale = escalaFin;
    }
}