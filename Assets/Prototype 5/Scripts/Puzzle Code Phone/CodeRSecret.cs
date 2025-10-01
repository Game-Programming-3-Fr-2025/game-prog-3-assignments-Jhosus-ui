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
        // Verificar si el temporizador expir�
        if (timerActive && !codeExpired && Time.time - timerStartTime >= timeToShowCode)
        {
            codeExpired = true;
            timerActive = false;
            Debug.Log("C�digo expirado");
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ShowMessage();
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

    public void RepairPhone()
    {
        isRepaired = true;
        codeExpired = false;
        timerActive = false;
        Debug.Log("Tel�fono reparado - esperando activaci�n del timer");
    }

    // Llamado por el objeto trigger externo
    public void StartCodeTimer()
    {
        if (isRepaired && !timerActive)
        {
            timerStartTime = Time.time;
            timerActive = true;
            codeExpired = false;
            Debug.Log("Timer activado - c�digo disponible por " + timeToShowCode + " segundos");
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