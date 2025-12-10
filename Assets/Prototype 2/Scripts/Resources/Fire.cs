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
    public Vector2 areaDanoSize = Vector2.one;
    public Vector2 areaDanoOffset = Vector2.zero;

    private SpriteRenderer[] sprites;
    private DamageSource[] damageSources;
    private int frameActual = 0;

    void Start()
    {
        sprites = GetComponentsInChildren<SpriteRenderer>();
        damageSources = new DamageSource[sprites.Length];

        for (int i = 0; i < sprites.Length; i++)
        {
            damageSources[i] = sprites[i].gameObject.AddComponent<DamageSource>();
            sprites[i].enabled = false;
            damageSources[i].enabled = false;
        }

        StartCoroutine(AnimacionFuego());
    }

    IEnumerator AnimacionFuego()
    {
        while (true)
        {
            if (frameActual > 0)
            {
                int prevFrame = frameActual - 1;
                sprites[prevFrame].enabled = false;
                damageSources[prevFrame].enabled = false;
            }

            var spriteActual = sprites[frameActual];
            var transformActual = spriteActual.transform;
            spriteActual.enabled = true;

            damageSources[frameActual].enabled = true;
            damageSources[frameActual].damageAreaSize = areaDanoSize;

            StartCoroutine(EscalarSprite(transformActual));

            frameActual = (frameActual + 1) % sprites.Length;

            yield return new WaitForSeconds(tiempoEntreFrames);
        }
    }

    IEnumerator EscalarSprite(Transform spriteTransform)
    {
        float tiempo = 0;
        Vector3 escalaIni = Vector3.one * escalaInicial;
        Vector3 escalaFin = Vector3.one * escalaFinal;

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