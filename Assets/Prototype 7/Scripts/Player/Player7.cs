using UnityEngine;

public class Player7 : MonoBehaviour
{
    public float moveSpeed = 5f; 

    private Rigidbody2D rb; 
    private Vector2 movement;
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        HandleSpriteFlip();
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    void HandleSpriteFlip()
    {
        if (movement.x != 0)
        {
            spriteRenderer.flipX = movement.x < 0;
        }
    }
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}