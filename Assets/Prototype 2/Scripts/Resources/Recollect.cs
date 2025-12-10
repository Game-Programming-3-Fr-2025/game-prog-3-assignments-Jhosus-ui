using UnityEngine;

public class Recollect : MonoBehaviour
{
    public enum TipoItem
    {
        Coin,
        PDash,
        PJump
    }

    [Header("Tipo de Item")]
    [SerializeField] private TipoItem tipoItem = TipoItem.Coin;

    [Header("Configuracin")]
    [SerializeField] private string playerLayer = "Players";

    [Header("Efectos")]
    [SerializeField] private ParticleSystem particleEffect;

    [Header("Sonido")]
    [SerializeField] private AudioClip collectSound; // Asigna el sonido desde el Inspector
    [SerializeField] private float volume = 1f; // Volumen del sonido

    [Header("Levitacin")]
    [SerializeField] private float levitationSpeed = 1f;
    [SerializeField] private float levitationHeight = 0.2f;

    private Vector3 startPosition;
    private float timeCounter = 0f;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        timeCounter += Time.deltaTime * levitationSpeed;
        float newY = startPosition.y + Mathf.Sin(timeCounter) * levitationHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(playerLayer))
        {
            RecolectarItem(other.gameObject);
        }
    }

    private void RecolectarItem(GameObject jugador)
    {
        enabled = false;

        // Reproducir sonido de recolección
        PlayCollectSound();

        // Activar habilidad seg˙n el tipo
        ActivarHabilidad(jugador);

        // Efectos visuales
        if (particleEffect != null) particleEffect.Play();

        // Ocultar item
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) sprite.enabled = false;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        Destroy(gameObject, 0.9f);
    }

    private void PlayCollectSound()
    {
        if (collectSound != null)
        {
            // Método simple: AudioSource.PlayClipAtPoint
            AudioSource.PlayClipAtPoint(collectSound, transform.position, volume);
        }
        else
        {
            Debug.LogWarning("No hay AudioClip asignado para el sonido de recolección");
        }
    }

    private void ActivarHabilidad(GameObject jugador)
    {
        switch (tipoItem)
        {
            case TipoItem.PDash:
                PDash dashComponent = jugador.GetComponent<PDash>();
                if (dashComponent != null)
                {
                    dashComponent.isDashUnlocked = true;
                    Debug.Log($"Dash desbloqueado para {jugador.name}");
                }
                break;

            case TipoItem.PJump:
                PJumps jumpComponent = jugador.GetComponent<PJumps>();
                if (jumpComponent != null)
                {
                    jumpComponent.isDoubleJumpUnlocked = true;
                    Debug.Log($"Doble salto desbloqueado para {jugador.name}");
                }
                break;

            case TipoItem.Coin:
                PlayerScore playerScore = jugador.GetComponent<PlayerScore>();
                if (playerScore != null)
                {
                    Destroy(gameObject);
                }
                break;
        }
    }
}