using UnityEngine;

public class Player5 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null) 
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; 
            rb.freezeRotation = true; 
        }
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized; 
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement * moveSpeed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Fathers")) 
        {
            GameOver();
        }
    }

    void GameOver()
    {
        TimeLoopManager.Instance.ForceLoopReset(); // Activar loop de escena
    }
}