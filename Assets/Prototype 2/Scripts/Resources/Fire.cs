using UnityEngine;
using System.Collections;

public class Fire : MonoBehaviour
{
    [Header("Configuración")]
    public float tiempoEntreFrames = 0.1f;
    public float escalaInicial = 0.1f;
    public float escalaFinal = 1f;
    public float tiempoCrecimiento = 0.2f;

    [Header("Daño")]
    public Vector2 areaDanoSize = Vector2.one; // Tamaño del área de daño para cada sprite
    public Vector2 areaDanoOffset = Vector2.zero; // Ajuste de posición del área

    private SpriteRenderer[] sprites;
    private DamageSource[] damageSources; // Array para múltiples fuentes de daño
    private int frameActual = 0;

    void Start()
    {
        // Obtener sprites de los hijos
        sprites = GetComponentsInChildren<SpriteRenderer>();

        // Inicializar array de DamageSources
        damageSources = new DamageSource[sprites.Length];

        // Crear un DamageSource para cada sprite hijo
        for (int i = 0; i < sprites.Length; i++)
        {
            // Añadir DamageSource al sprite hijo (no al padre)
            damageSources[i] = sprites[i].gameObject.AddComponent<DamageSource>();

            // Ocultar todos al inicio
            sprites[i].enabled = false;

            // Inicialmente desactivar el daño hasta que el sprite esté visible
            damageSources[i].enabled = false;
        }

        // Iniciar animación
        StartCoroutine(AnimacionFuego());
    }

    IEnumerator AnimacionFuego()
    {
        while (true)
        {
            // Ocultar frame anterior y desactivar su daño
            if (frameActual > 0)
            {
                int prevFrame = frameActual - 1;
                sprites[prevFrame].enabled = false;
                damageSources[prevFrame].enabled = false;
            }

            // Mostrar y animar frame actual
            var spriteActual = sprites[frameActual];
            var transformActual = spriteActual.transform;
            spriteActual.enabled = true;

            // Activar el daño para este sprite
            damageSources[frameActual].enabled = true;

            // Aplicar configuración de daño específica para fuego
            var damageSource = damageSources[frameActual];
            damageSource.damageAreaSize = areaDanoSize;

            // Ajustar posición del área de daño si es necesario
            // (El DamageSource ya está en el sprite hijo, así que usará su posición)

            StartCoroutine(EscalarSprite(transformActual));

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