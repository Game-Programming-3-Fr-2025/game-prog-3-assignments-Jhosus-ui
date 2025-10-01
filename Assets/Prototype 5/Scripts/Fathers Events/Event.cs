using UnityEngine;

public class Event : MonoBehaviour
{
    public GameObject objectToShow;
    private bool activated;

    void Start()
    {
        if (objectToShow) objectToShow.SetActive(false);

        // Suscribirse al evento de reset del Time Loop
        TimeLoopManager.Instance.OnLoopReset += ResetEvent;
    }

    void OnDestroy()
    {
        // Desuscribirse para evitar errores
        if (TimeLoopManager.Instance != null)
            TimeLoopManager.Instance.OnLoopReset -= ResetEvent;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !activated && objectToShow)
        {
            objectToShow.SetActive(true);
            activated = true;
            GetComponent<Collider2D>().enabled = false;

            // Registrar que este evento fue activado
            TimeLoopManager.Instance.RegisterObjectState(gameObject.name + "_activated", true);
        }
    }

    public void ResetEvent()
    {
        activated = false;
        GetComponent<Collider2D>().enabled = true;
        if (objectToShow) objectToShow.SetActive(false);

        Debug.Log($"Event {gameObject.name} reseteado");
    }
}