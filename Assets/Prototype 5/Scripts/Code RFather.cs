using UnityEngine;
using UnityEngine.UI;

public class CodeRFather : MonoBehaviour
{
    public string correctCode = "1234"; // C�digo para esta puerta
    public GameObject codeUI; // Panel con los botones num�ricos

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
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideCodeUI();
        }
    }

    void ShowCodeUI()
    {
        if (codeUI != null)
        {
            codeUI.SetActive(true);
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

    // Llamado por los botones num�ricos del UI
    public void AddDigit(int digit)
    {
        currentInput += digit.ToString();

        if (currentInput.Length >= 4)
        {
            CheckCode();
        }
    }

    void CheckCode()
    {
        if (currentInput == correctCode)
        {
            // C�digo correcto - ocultar puerta
            gameObject.SetActive(false);
            HideCodeUI();
            Debug.Log("�Puerta desbloqueada!");
        }
        else
        {
            // C�digo incorrecto - resetear
            currentInput = "";
            Debug.Log("C�digo incorrecto");
        }
    }
}