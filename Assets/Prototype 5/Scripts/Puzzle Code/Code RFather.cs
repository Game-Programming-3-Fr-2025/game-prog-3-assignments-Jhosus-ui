using UnityEngine;
using UnityEngine.UI;

public class CodeRFather : MonoBehaviour
{
    public string correctCode = "1234"; 
    public GameObject codeUI; 

    private string currentInput = "";
    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E)) // Interacción con E
        {
            ShowCodeUI();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Jugador cerca de la puerta
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            playerInRange = false;
            HideCodeUI(); // Ocultar interfaz al salir
        }
    }

    void ShowCodeUI()
    {
        if (codeUI != null)
        {
            codeUI.SetActive(true); // Mostrar panel de código
            currentInput = ""; 
        }
    }

    void HideCodeUI()
    {
        if (codeUI != null)
        {
            codeUI.SetActive(false); 
        }
    }
    public void AddDigit(int digit) // Llamado por botones numéricos
    {
        currentInput += digit.ToString(); 

        if (currentInput.Length >= 4) // Completaron 4 dígitos
        {
            CheckCode();
        }
    }

    void CheckCode()
    {
        if (currentInput == correctCode) 
        {
            gameObject.SetActive(false); // Ocultar puerta
            HideCodeUI(); 
        }
        else 
        {
            currentInput = ""; // Reiniciar entrada
        }
    }
}