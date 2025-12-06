using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public GameObject shopUI;
    private KeyCode interactKey = KeyCode.E;

    private bool isShopOpen = false;

    void Start()
    {
        shopUI?.SetActive(false);
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
        shopUI?.SetActive(isShopOpen);
    }

    void CloseShop()
    {
        isShopOpen = false;
        shopUI?.SetActive(false);
    }
}