using UnityEngine;

public class CodeOPhone : MonoBehaviour
{
    public string correctCode = "1234";
    public GameObject codeUI;
    private string currentInput = "";
    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ShowCodeUI();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true; 
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideCodeUI(); // Ocultar UI al alejarse
        }
    }

    void ShowCodeUI()
    {
        if (codeUI != null)
        {
            codeUI.SetActive(true); // Mostrar teclado
            currentInput = ""; 
        }
    }

    void HideCodeUI()
    {
        if (codeUI != null) codeUI.SetActive(false); // Ocultar teclado
    }
    public void AddDigit(int digit)
    {
        currentInput += digit.ToString(); 
        if (currentInput.Length >= 4) CheckCode(); // Verificar si completo
    }

    void CheckCode()
    {
        if (currentInput == correctCode)
        {
            gameObject.SetActive(false); // Desbloquear objeto
            HideCodeUI();
        }
        else
        {
            currentInput = ""; // Resetear si incorrecto
        }
    }
}