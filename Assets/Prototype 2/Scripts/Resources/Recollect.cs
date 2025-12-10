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

    [Header("Configuración")]
    [SerializeField] private string playerLayer = "Players";

    [Header("Efectos")]
    [SerializeField] private ParticleSystem particleEffect;

    [Header("Sonido")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float volume = 1f;

    [Header("Levitación")]
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
            RecolectarItem(other.gameObject);
    }

    private void RecolectarItem(GameObject jugador)
    {
        enabled = false;

        PlayCollectSound();
        ActivarHabilidad(jugador);

        if (particleEffect != null) particleEffect.Play();

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) sprite.enabled = false;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        Destroy(gameObject, 0.9f);
    }

    private void PlayCollectSound()
    {
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position, volume);
    }

    private void ActivarHabilidad(GameObject jugador)
    {
        switch (tipoItem)
        {
            case TipoItem.PDash:
                PDash dashComponent = jugador.GetComponent<PDash>();
                if (dashComponent != null)
                    dashComponent.isDashUnlocked = true;
                break;

            case TipoItem.PJump:
                PJumps jumpComponent = jugador.GetComponent<PJumps>();
                if (jumpComponent != null)
                    jumpComponent.isDoubleJumpUnlocked = true;
                break;

            case TipoItem.Coin:
                PlayerScore playerScore = jugador.GetComponent<PlayerScore>();
                if (playerScore != null)
                    Destroy(gameObject);
                break;
        }
    }
}