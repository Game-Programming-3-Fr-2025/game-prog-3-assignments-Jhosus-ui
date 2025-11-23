using UnityEngine;

public class Blocks : MonoBehaviour
{
    [Header("Block Settings")]
    public Sprite normalFace;
    public Sprite crackedFace;
    public float breakTime = 0.5f;
    public float respawnTime = 3f;
    public float shakeIntensity = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Collider2D blockCollider;
    private Vector3 originalPosition;
    private bool isBreaking;
    private bool isBroken;

    [SerializeField] private ParticleSystem efecto;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        blockCollider = GetComponent<Collider2D>();
        originalPosition = transform.position;

        ResetBlock();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isBreaking && !isBroken)
        {
            string collisionTag = collision.gameObject.tag;
            if (collisionTag == "Player 1" || collisionTag == "Player 2")
            {
                StartBreaking();
            }
        }
    }

    void StartBreaking()
    {
        isBreaking = true;
        spriteRenderer.sprite = crackedFace;
        Invoke("BreakBlock", breakTime);
    }

    void BreakBlock()
    {
        if (efecto != null)
            efecto.Play();

        isBroken = true;
        isBreaking = false;
        spriteRenderer.enabled = false;
        blockCollider.enabled = false;
        Invoke("RespawnBlock", respawnTime);
    }

    void RespawnBlock()
    {
        transform.position = originalPosition;
        spriteRenderer.enabled = true;
        blockCollider.enabled = true;
        spriteRenderer.sprite = normalFace;
        isBroken = false;
    }

    void ResetBlock()
    {
        spriteRenderer.sprite = normalFace;
        spriteRenderer.enabled = true;
        blockCollider.enabled = true;
        isBreaking = false;
        isBroken = false;
    }

    void Update()
    {
        if (isBreaking)
        {
            // Efecto de temblor simple
            transform.position = originalPosition + Random.insideUnitSphere * shakeIntensity;
        }
    }
}