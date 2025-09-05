using UnityEngine;

public class LR : MonoBehaviour
{
    [Header("Configuraci�n de Rotaci�n")]
    public float rotationSpeed = 90f;    
    public bool clockwise = true;        

    void Update()
    {
        
        float direction = clockwise ? 1f : -1f;

        
        transform.Rotate(0f, 0f, direction * rotationSpeed * Time.deltaTime);
    }
}