using UnityEngine;
using TMPro;

public class KeyROffice : MonoBehaviour
{
    public GameObject keyImageUI;
    public TextMeshProUGUI noKeyText, unlockText, finalText;
    public bool isFinal = false;

    private bool playerInRange = false;

    void Update()// Comprobar entrada del jugador
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

    void TryOpenDoor()  // Intentar abrir la puerta
    {
        bool hasKey = keyImageUI != null && keyImageUI.activeInHierarchy;

        if (hasKey)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.doorOpen);
            gameObject.SetActive(false); // Abrir puerta
            ShowMessage(isFinal ? finalText : unlockText);
        }
        else
        {
            ShowMessage(noKeyText);
        }
    }

    void ShowMessage(TextMeshProUGUI textElement) // Mostrar mensaje temporalmente
    {
        if (textElement != null)
        {
            textElement.gameObject.SetActive(true);
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