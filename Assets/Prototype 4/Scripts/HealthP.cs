using UnityEngine;
using TMPro;

public class HealthP : MonoBehaviour
{
    [Header("Configuración de Salud")]
    public int saludMaxima = 100;
    public int saludActual;
    public bool esChasis = false;

    [Header("Referencias 3D")]
    public TextMeshPro textoSalud3D;

    [Header("Debug")]
    public bool mostrarLogsDamage = true;

    private GameManager gameManager;

    void Start()
    {
        saludActual = saludMaxima;
        gameManager = FindObjectOfType<GameManager>();

        // Asignar tags automáticamente
        if (esChasis && !gameObject.CompareTag("Chasis"))
        {
            gameObject.tag = "Chasis";
        }
        else if (!esChasis && !gameObject.CompareTag("Modulo"))
        {
            gameObject.tag = "Modulo";
        }

        ConfigurarTextoSalud();
        ActualizarUI();

        Debug.Log($"HealthP inicializado: {gameObject.name} - Tag: {gameObject.tag} - Salud: {saludActual}");
    }

    private void ConfigurarTextoSalud()
    {
        if (textoSalud3D == null)
        {
            textoSalud3D = GetComponentInChildren<TextMeshPro>();
        }

        if (textoSalud3D == null)
        {
            CrearTextoSaludAutomatico();
        }
    }

    private void CrearTextoSaludAutomatico()
    {
        GameObject textoObj = new GameObject("TextoSalud");
        textoObj.transform.SetParent(transform);
        textoObj.transform.localPosition = new Vector3(0, 0.8f, 0);
        textoObj.transform.localRotation = Quaternion.identity;

        textoSalud3D = textoObj.AddComponent<TextMeshPro>();
        textoSalud3D.text = $"{saludActual}/{saludMaxima}";
        textoSalud3D.fontSize = 2;
        textoSalud3D.alignment = TextAlignmentOptions.Center;
        textoSalud3D.sortingOrder = 10;
    }

    public void RecibirDanio(int cantidad)
    {
        saludActual -= cantidad;

        if (mostrarLogsDamage)
        {
            Debug.Log($"{gameObject.name} recibió {cantidad} daño. Salud: {saludActual}/{saludMaxima}");
        }

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
            Debug.Log("CHASIS DESTRUIDO - GAME OVER");
            Time.timeScale = 0; // Pausar juego
        }
        else
        {
            Debug.Log($"Módulo {gameObject.name} destruido");
            // CRÍTICO: Llamar a GameManager ANTES de destruir
            if (gameManager != null)
            {
                gameManager.EliminarModulo(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void ActualizarUI()
    {
        if (textoSalud3D != null)
        {
            textoSalud3D.text = $"{saludActual}/{saludMaxima}";

            float porcentaje = (float)saludActual / saludMaxima;
            if (porcentaje < 0.3f)
                textoSalud3D.color = Color.red;
            else if (porcentaje < 0.6f)
                textoSalud3D.color = Color.yellow;
            else
                textoSalud3D.color = Color.green;
        }
    }

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