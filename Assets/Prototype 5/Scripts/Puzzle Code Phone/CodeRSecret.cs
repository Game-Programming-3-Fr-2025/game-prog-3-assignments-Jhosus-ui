using UnityEngine;
using TMPro;

public class CodeRSecret : MonoBehaviour
{
    public TextMeshProUGUI codeText;
    public TextMeshProUGUI noCodeText;
    public TextMeshProUGUI repairText;
    public float timeToShowCode = 10f;

    private bool isRepaired = false;
    private bool playerInRange = false;
    private float timerStartTime;
    private bool timerActive = false;
    private bool codeExpired = false;

    void Update()
    {
        if (timerActive && !codeExpired && Time.time - timerStartTime >= timeToShowCode) // Timer expirado
        {
            codeExpired = true;
            timerActive = false;
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.E)) ShowMessage();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    public void RepairPhone()
    {
        isRepaired = true;
        codeExpired = false;
        timerActive = false;
    }

    public void StartCodeTimer() // Llamado por trigger externo
    {
        if (isRepaired && !timerActive)
        {
            timerStartTime = Time.time;
            timerActive = true;
            codeExpired = false;
        }
    }

    void ShowMessage()
    {
        HideAllMessages();

        if (!isRepaired)
        {
            if (repairText != null)
            {
                repairText.gameObject.SetActive(true);
                Invoke("HideAllMessages", 3f);
            }
        }
        else if (codeExpired)
        {
            if (noCodeText != null)
            {
                noCodeText.gameObject.SetActive(true);
                Invoke("HideAllMessages", 3f);
            }
        }
        else
        {
            if (codeText != null)
            {
                codeText.gameObject.SetActive(true);
                Invoke("HideAllMessages", 3f);
            }
        }
    }

    void HideAllMessages()
    {
        if (codeText != null) codeText.gameObject.SetActive(false);
        if (noCodeText != null) noCodeText.gameObject.SetActive(false);
        if (repairText != null) repairText.gameObject.SetActive(false);
    }
}