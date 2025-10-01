using UnityEngine;

public class DoorSimple : MonoBehaviour
{
    public float reappearTime = 5f;
    private bool canOpen = true;
    private bool playerInRange;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && canOpen)
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
        gameObject.SetActive(false);
        canOpen = false;
        Invoke("Reappear", reappearTime);

        // Registrar para el Time Loop
        TimeLoopManager.Instance.RegisterCollectedItem(gameObject);
    }

    void Reappear()
    {
        gameObject.SetActive(true);
    }
}