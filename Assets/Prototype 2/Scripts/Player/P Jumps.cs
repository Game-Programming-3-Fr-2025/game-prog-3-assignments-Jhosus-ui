using UnityEngine;

public class PJumps : MonoBehaviour
{
    [Header("Jump Settings")]
    public bool isDoubleJumpUnlocked = false;
    public float doubleJumpForce = 14f;
    public GameObject doubleJumpActivatorObject;

    [Header("Point Jump")]
    public float pointJumpForce = 18f;

    private Rigidbody2D rb;
    private Player2 player;
    private PDash dashComponent;

    private bool hasUsedAirJump = false;
    private bool wasGrounded = true;
    private bool inPointTrigger = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player2>();
        dashComponent = GetComponent<PDash>();
    }

    void Update()
    {
        if (!isDoubleJumpUnlocked) return;

        HandleAirJump();
        UpdateJumpReset();
    }

    void HandleAirJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (inPointTrigger)
            {
                PerformPointJump();
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
    }

    void PerformPointJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, pointJumpForce);
        if (isDoubleJumpUnlocked) hasUsedAirJump = false;
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
        }
    }

    public bool HasUsedAirJump() => hasUsedAirJump;
}