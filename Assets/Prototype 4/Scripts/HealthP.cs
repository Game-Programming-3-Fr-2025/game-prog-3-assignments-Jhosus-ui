using UnityEngine;
using TMPro;

public class HealthP : MonoBehaviour
{
    [Header("Configuración de Salud")]
    public int saludMaxima = 100;
    public int saludActual;
    public bool esChasis = false;

    [Header("Referencias 3D")]
    public TextMeshPro textoSalud3D; // TextMeshPro para objetos 3D

    private GameManager gameManager;

    void Start()
    {
        saludActual = saludMaxima;
        gameManager = FindObjectOfType<GameManager>();

        // Buscar TextMeshPro en hijos si no está asignado
        if (textoSalud3D == null)
        {
            textoSalud3D = GetComponentInChildren<TextMeshPro>();
        }

        // Si aún no existe, crear uno automáticamente
        if (textoSalud3D == null)
        {
            CrearTextoSaludAutomatico();
        }

        ActualizarUI();
    }

    private void CrearTextoSaludAutomatico()
    {
        GameObject textoObj = new GameObject("TextoSalud");
        textoObj.transform.SetParent(transform);
        textoObj.transform.localPosition = new Vector3(0, 0.8f, 0); // Encima del objeto
        textoObj.transform.localRotation = Quaternion.identity;

        textoSalud3D = textoObj.AddComponent<TextMeshPro>();
        textoSalud3D.text = $"{saludActual}/{saludMaxima}";
        textoSalud3D.fontSize = 2;
        textoSalud3D.alignment = TextAlignmentOptions.Center;
        textoSalud3D.sortingOrder = 10; // Para que se vea por encima de otros sprites
    }

    public void RecibirDanio(int cantidad)
    {
        saludActual -= cantidad;

        if (saludActual <= 0)
        {
            saludActual = 0;
            Morir();
        }

        ActualizarUI();
    }

    private void Morir()
    {
        if (esChasis)
        {
            // Game Over
            Debug.Log("CHASIS DESTRUIDO - GAME OVER");
            // Aquí puedes llamar a una función de GameManager para game over
        }
        else
        {
            // Destruir módulo y bajar los de arriba
            if (gameManager != null)
            {
                gameManager.EliminarModulo(this.gameObject);
            }
        }

        Destroy(gameObject);
    }

    private void ActualizarUI()
    {
        if (textoSalud3D != null)
        {
            textoSalud3D.text = $"{saludActual}/{saludMaxima}";

            // Cambiar color según la salud (opcional)
            if (saludActual < saludMaxima * 0.3f)
                textoSalud3D.color = Color.red;
            else if (saludActual < saludMaxima * 0.6f)
                textoSalud3D.color = Color.yellow;
            else
                textoSalud3D.color = Color.green;
        }
    }

    // Para debugging
    private void OnDrawGizmosSelected()
    {
        if (esChasis)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, new Vector3(1.5f, 1.5f, 0.1f));
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(1.2f, 1.2f, 0.1f));
        }
    }
}