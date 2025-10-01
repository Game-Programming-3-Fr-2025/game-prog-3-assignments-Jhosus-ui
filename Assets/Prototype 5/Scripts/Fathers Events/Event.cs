using UnityEngine;

public class Event : MonoBehaviour
{
    public GameObject objectToShow; // El objeto "Is Mother" que aparecerá
    private bool activated; // Control interno

    void Start()
    {
        if (objectToShow) objectToShow.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !activated && objectToShow)
        {
            objectToShow.SetActive(true);
            activated = true;
            GetComponent<Collider2D>().enabled = false; // Desactiva el trigger
        }
    }

    // Para resetear en el time loop
    public void ResetEvent()
    {
        activated = false;
        GetComponent<Collider2D>().enabled = true;
        if (objectToShow) objectToShow.SetActive(false);
    }
}