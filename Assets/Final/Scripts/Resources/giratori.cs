using UnityEngine;

public class giratori : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    public float velocidadRotacion = 90f; 
    public bool rotarDerecha = true; 

    void Start()
    {
        string direccion = rotarDerecha ? "derecha" : "izquierda";
        Debug.Log($"Rotación configurada: {direccion} a {velocidadRotacion}°/segundo");
    }
    void Update()
    {
        RotarObjeto();
    }

    void RotarObjeto()
    {
        
        float direccion = rotarDerecha ? -1f : 1f; // Determinar la dirección de rotación (1 para derecha, -1 para izquierda)

        
        transform.Rotate(0, 0, velocidadRotacion * direccion * Time.deltaTime); // Rotar el objeto en el eje Z
    }
}