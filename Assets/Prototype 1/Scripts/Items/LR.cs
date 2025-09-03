using UnityEngine;

public class LR : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    public float rotationSpeed = 90f;    
    public bool clockwise = true;        

    void Update()
    {
        // Calcular la dirección de rotación
        float direction = clockwise ? 1f : -1f;

        // Rotar el objeto sobre su eje Z
        transform.Rotate(0f, 0f, direction * rotationSpeed * Time.deltaTime);
    }
}