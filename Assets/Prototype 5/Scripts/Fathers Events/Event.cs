using UnityEngine;

public class Event : MonoBehaviour
{
    public GameObject objectToShow;
    private bool activated;

    void Start()
    {
        if (objectToShow) objectToShow.SetActive(false); // Ocultar objeto al inicio

        TimeLoopManager.Instance.OnLoopReset += ResetEvent; // Suscribir al reset del loop
    }

    void OnDestroy()
    {
        if (TimeLoopManager.Instance != null)
            TimeLoopManager.Instance.OnLoopReset -= ResetEvent; // Limpiar suscripción
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !activated && objectToShow)
        {
            objectToShow.SetActive(true); // Mostrar objeto vinculado
            activated = true;
            GetComponent<Collider2D>().enabled = false;

            TimeLoopManager.Instance.RegisterObjectState(gameObject.name + "_activated", true); // Registrar estado
        }
    }

    public void ResetEvent()
    {
        activated = false;
        GetComponent<Collider2D>().enabled = true; // Reactivar collider
        if (objectToShow) objectToShow.SetActive(false); 
    }
}