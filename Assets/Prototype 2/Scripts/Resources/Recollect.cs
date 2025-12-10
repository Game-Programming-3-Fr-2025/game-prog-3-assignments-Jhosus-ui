using UnityEngine;

public class Recollect : MonoBehaviour
{
    public enum TipoItem { Coin, PDash, PJump }

    [SerializeField] private TipoItem tipoItem = TipoItem.Coin;
    [SerializeField] private string playerLayer = "Players";
    [SerializeField] private GameObject collectParticlePrefab;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float volume = 1f;
    [SerializeField] private float levitationSpeed = 1f;
    [SerializeField] private float levitationHeight = 0.2f;

    private Vector3 startPosition;
    private float timeCounter = 0f;
    private bool collected = false;

    void Start() => startPosition = transform.position;

    void Update()
    {
        if (collected) return;
        timeCounter += Time.deltaTime * levitationSpeed;
        float newY = startPosition.y + Mathf.Sin(timeCounter) * levitationHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (other.gameObject.layer == LayerMask.NameToLayer(playerLayer))
            RecolectarItem(other.gameObject);
    }

    private void RecolectarItem(GameObject jugador)
    {
        collected = true;
        PlayCollectSound(jugador); // ← Cambiado: pasar el jugador
        ActivarHabilidad(jugador);
        SpawnCollectParticles();
        HideCollectible();
        Destroy(gameObject, 2f);
    }

    private void PlayCollectSound(GameObject jugador)
    {
        if (collectSound == null) return;

        // Reproducir en el AudioSource del jugador que recolectó
        AudioSource playerAudio = jugador.GetComponent<AudioSource>();
        if (playerAudio != null)
        {
            playerAudio.PlayOneShot(collectSound, volume);
        }
        else
        {
            Debug.LogWarning($"El jugador {jugador.name} no tiene AudioSource!");
        }
    }

    private void SpawnCollectParticles()
    {
        if (collectParticlePrefab == null) return;
        GameObject particles = Instantiate(collectParticlePrefab, transform.position, Quaternion.identity);
        Destroy(particles, 3f);
    }

    private void HideCollectible()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) sprite.enabled = false;
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;
    }

    private void ActivarHabilidad(GameObject jugador)
    {
        switch (tipoItem)
        {
            case TipoItem.PDash:
                PDash dash = jugador.GetComponent<PDash>();
                if (dash != null) dash.isDashUnlocked = true;
                break;
            case TipoItem.PJump:
                PJumps jump = jugador.GetComponent<PJumps>();
                if (jump != null) jump.isDoubleJumpUnlocked = true;
                break;
            case TipoItem.Coin:
                PlayerScore score = jugador.GetComponent<PlayerScore>();
                if (score != null) Destroy(gameObject);
                break;
        }
    }
}