using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public GameObject shopUI; // Arrastra el Canvas de la tienda aquí
    public KeyCode interactKey = KeyCode.E;

    private bool isShopOpen = false;

    void Start()
    {
        if (shopUI != null)
            shopUI.SetActive(false);
    }

    void Update()
    {
        // Solo verificar input si el jugador está en el trigger
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(interactKey))
        {
            ToggleShop();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CloseShop();
        }
    }

    void ToggleShop()
    {
        isShopOpen = !isShopOpen;

        if (shopUI != null)
            shopUI.SetActive(isShopOpen);
    }

    void CloseShop()
    {
        isShopOpen = false;
        if (shopUI != null)
            shopUI.SetActive(false);
    }
}