using UnityEngine;
using TMPro;

public class KeyROffice : MonoBehaviour
{
    public GameObject keyImageUI;
    public TextMeshProUGUI noKeyText;    // Texto cuando no tiene llave
    public TextMeshProUGUI unlockText;   // Texto cuando desbloquea normal
    public TextMeshProUGUI finalText;    // Texto cuando es final
    public bool isFinal = false;

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryOpenDoor();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    void TryOpenDoor()
    {
        bool hasKey = keyImageUI != null && keyImageUI.activeInHierarchy;

        if (hasKey)
        {
            gameObject.SetActive(false); // Puerta desaparece

            if (isFinal && finalText != null)
            {
                finalText.gameObject.SetActive(true);
                Invoke("HideAllMessages", 3f);
            }
            else if (unlockText != null)
            {
                unlockText.gameObject.SetActive(true);
                Invoke("HideAllMessages", 3f);
            }
        }
        else if (noKeyText != null)
        {
            noKeyText.gameObject.SetActive(true);
            Invoke("HideAllMessages", 3f);
        }
    }

    void HideAllMessages()
    {
        if (noKeyText != null) noKeyText.gameObject.SetActive(false);
        if (unlockText != null) unlockText.gameObject.SetActive(false);
        if (finalText != null) finalText.gameObject.SetActive(false);
    }
}