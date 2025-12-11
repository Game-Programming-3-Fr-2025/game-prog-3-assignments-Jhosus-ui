using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PJumpsGP2 : MonoBehaviour
{
    [Header("Jump Settings")]
    public bool isDoubleJumpUnlocked = false;
    public float doubleJumpForce = 14f;
    public GameObject doubleJumpActivatorObject;

    [Header("Point Jump")]
    public float pointJumpForce = 18f;
    private float pointBufferTime = 0.2f;
    private float pointBufferTimer = 0f;

    [Header("Jump Sounds")]
    public AudioClip airJumpSound;
    public AudioClip pointJumpSound;

    private Rigidbody2D rb;
    private Player2GP2 player;
    private PDash dashComponent;
    private AudioSource audioSource;

    private bool hasUsedAirJump = false;
    private bool wasGrounded = true;
    private bool inPointTrigger = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player2GP2>();
        dashComponent = GetComponent<PDash>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (!isDoubleJumpUnlocked) return;

        HandleAirJump();
        UpdateJumpReset();

        if (pointBufferTimer > 0f)
        {
            pointBufferTimer -= Time.deltaTime;
        }
    }

    void HandleAirJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (inPointTrigger || pointBufferTimer > 0f)
            {
                PerformPointJump();
                pointBufferTimer = 0f;
            }
            else if (CanAirJump())
            {
                PerformAirJump();
            }
        }
    }

    bool CanAirJump()
    {
        return !player.isGrounded && !hasUsedAirJump;
    }

    void PerformAirJump()
    {
        hasUsedAirJump = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);

        if (airJumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(airJumpSound);
        }
    }

    void PerformPointJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, pointJumpForce);
        if (isDoubleJumpUnlocked) hasUsedAirJump = false;

        if (pointJumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pointJumpSound);
        }
    }

    void UpdateJumpReset()
    {
        if (player.isGrounded && !wasGrounded)
        {
            hasUsedAirJump = false;
        }
        wasGrounded = player.isGrounded;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!isDoubleJumpUnlocked && other.gameObject == doubleJumpActivatorObject)
        {
            isDoubleJumpUnlocked = true;
            doubleJumpActivatorObject.SetActive(false);
        }

        if (other.CompareTag("Points"))
        {
            inPointTrigger = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Points"))
        {
            inPointTrigger = false;
            pointBufferTimer = pointBufferTime;
        }
    }

    public bool HasUsedAirJump() => hasUsedAirJump;
}