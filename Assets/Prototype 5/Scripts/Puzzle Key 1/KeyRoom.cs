using UnityEngine;

public class KeyRoom : MonoBehaviour
{
    public GameObject keyImageUI;
    public bool isKeySpecial = false;

    private bool isActive = true;

    void Start()
    {
        // Llaves especiales empiezan ocultas
        if (isKeySpecial)
        {
            gameObject.SetActive(false);
            isActive = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isActive)
        {
            if (keyImageUI != null) keyImageUI.SetActive(true);
            gameObject.SetActive(false);
            isActive = false;
        }
    }
}