using UnityEngine;

public class KeyROffice : MonoBehaviour
{
    public GameObject keyImageUI; // Image UI que muestra que tienes la llave
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
        }
    }

    void TryOpenDoor()
    {
        // Verificar si el Image UI de la llave está activo (significa que tienes la llave)
        if (keyImageUI != null && keyImageUI.activeInHierarchy)
        {
            // Abrir puerta - ocultarla
            gameObject.SetActive(false);
            Debug.Log("¡Puerta abierta con llave!");
        }
        else
        {
            Debug.Log("Necesitas la llave para abrir esta puerta");
        }
    }
}