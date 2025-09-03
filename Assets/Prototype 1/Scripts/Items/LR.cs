using UnityEngine;

public class LR : MonoBehaviour
{
    [Header("Configuraci�n de Rotaci�n")]
    public float rotationSpeed = 90f;    
    public bool clockwise = true;        

    void Update()
    {
        // Calcular la direcci�n de rotaci�n
        float direction = clockwise ? 1f : -1f;

        // Rotar el objeto sobre su eje Z
        transform.Rotate(0f, 0f, direction * rotationSpeed * Time.deltaTime);
    }
}