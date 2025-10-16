using UnityEngine;
using TMPro;

public class CodePRepair : MonoBehaviour
{
    public TextMeshProUGUI repairMessage;
    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            RepairPhone();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true; // Player cerca
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false; // Player lejos
    }
    void RepairPhone()
    {
        CodeRSecret phone = FindObjectOfType<CodeRSecret>();
        if (phone != null)
        {
            phone.RepairPhone(); // Reparar tel�fono

            if (repairMessage != null)
            {
                repairMessage.gameObject.SetActive(true); 
                Invoke("HideMessage", 3f); 
            }
        }
    }

    void HideMessage()
    {
        if (repairMessage != null) repairMessage.gameObject.SetActive(false); // Ocultar UI
    }
}