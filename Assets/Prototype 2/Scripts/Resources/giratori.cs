using UnityEngine;

public class giratori : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    public float velocidadRotacion = 90f; // Grados por segundo
    public bool rotarDerecha = true; // True = derecha, False = izquierda

    // Start is called once before the execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Opcional: Mostrar en consola la dirección configurada
        string direccion = rotarDerecha ? "derecha" : "izquierda";
        Debug.Log($"Rotación configurada: {direccion} a {velocidadRotacion}°/segundo");
    }

    // Update is called once per frame
    void Update()
    {
        RotarObjeto();
    }

    void RotarObjeto()
    {
        // Determinar la dirección de rotación (1 para derecha, -1 para izquierda)
        float direccion = rotarDerecha ? -1f : 1f;

        // Rotar el objeto en el eje Z
        transform.Rotate(0, 0, velocidadRotacion * direccion * Time.deltaTime);
    }
}