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
        if (codeUI != null) codeUI.SetActive(false);
    }

    public void AddDigit(int digit)
    {
        currentInput += digit.ToString();
        if (currentInput.Length >= 4) CheckCode();
    }

    void CheckCode()
    {
        if (currentInput == correctCode)
        {
            gameObject.SetActive(false);
            HideCodeUI();
        }
        else
        {
            currentInput = "";
        }
    }
}