using UnityEngine;

public class Recollect : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string playerLayer = "Players"; // Layer del jugador

    [Header("Efectos")]
    [SerializeField] private ParticleSystem particleEffect; // Referencia directa al ParticleSystem

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si es un jugador
        if (other.gameObject.layer == LayerMask.NameToLayer(playerLayer))
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        // Reproducir partículas si existe la referencia
        if (particleEffect != null)
        {
            particleEffect.Play();
        }

        // Reproducir sonido si hay AudioSource
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            audio.Play();
        }

        // Desactivar renderer para hacerlo invisible
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.enabled = false;
        }

        // Desactivar collider para que no pueda ser recolectado de nuevo
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Destruir el objeto después de un pequeño delay
        Destroy(gameObject, 0.9f);
    }
}