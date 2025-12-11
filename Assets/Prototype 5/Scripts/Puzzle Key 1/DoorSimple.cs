using UnityEngine;

public class DoorSimple : MonoBehaviour
{
    public float reappearTime = 5f;
    private bool canOpen = true;
    private bool playerInRange;
    private bool wasOpenedThisLoop = false; 

    void Start()
    {
        TimeLoopManager.Instance.OnLoopReset += ResetDoorState; // Suscribir al reset
    }

    void OnDestroy()
    {
        if (TimeLoopManager.Instance != null)
            TimeLoopManager.Instance.OnLoopReset -= ResetDoorState; // Limpiar suscripción
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && canOpen && !wasOpenedThisLoop)
        {
            OpenDoor();
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

    void OpenDoor()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.doorOpen);
        gameObject.SetActive(false);
        canOpen = false;
        wasOpenedThisLoop = true; 
        Invoke("Reappear", reappearTime);

        TimeLoopManager.Instance.RegisterCollectedItem(gameObject); 
    }

    void Reappear()
    {
        gameObject.SetActive(true);
        canOpen = true; // Permitir abrir nuevamente (pero wasOpenedThisLoop lo bloquea)
    }

    void ResetDoorState()
    {
        wasOpenedThisLoop = false; // Resetear estado al nuevo loop
        canOpen = true; 
        gameObject.SetActive(true);
        CancelInvoke("Reappear");
    }
}