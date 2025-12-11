using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BotonCargarEscena : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Nombre exacto de la escena (debe estar en Build Settings)")]
    public string nombreEscena;

    private Button boton;

    void Awake()
    {
        boton = GetComponent<Button>();

        if (boton == null)
        {
            Debug.LogError($"No se encontró componente Button");
            return;
        }

        if (string.IsNullOrEmpty(nombreEscena))
        {
            Debug.LogError($"No se ha asignado un nombre de escena");
            return;
        }

        if (!EscenaExisteEnBuild(nombreEscena))
        {
            Debug.LogError($"La escena no está en Build Settings");
            return;
        }
    } 

    void OnEnable()
    {
        if (boton != null)
        {
            boton.onClick.RemoveListener(CargarEscena);
            boton.onClick.AddListener(CargarEscena);
        }
    }

    void OnDisable()
    {
        if (boton != null)
            boton.onClick.RemoveListener(CargarEscena); // Limpiar listeners al desactivar
    }

    void CargarEscena()
    {
        if (string.IsNullOrEmpty(nombreEscena)) // Validación adicional por seguridad
        {
            Debug.LogError("No se puede cargar: nombre de escena vacío");
            return;
        }

        try
        {
            SceneManager.LoadScene(nombreEscena);
        }
        catch (System.Exception e) 
        {
            Debug.LogError($"Error al cargar escena");
        }
    }

    public void CargarEscenaAsync()
    {
        if (string.IsNullOrEmpty(nombreEscena))
        {
            Debug.LogError("No se puede cargar: nombre de escena vacío");
            return;
        }

        StartCoroutine(CargarEscenaAsyncCoroutine());
    }

    private System.Collections.IEnumerator CargarEscenaAsyncCoroutine() //Carga asíncrona de escena
    {
        AsyncOperation operacion = SceneManager.LoadSceneAsync(nombreEscena); 

        while (!operacion.isDone)
        {
            float progreso = Mathf.Clamp01(operacion.progress / 0.9f); // Normalizar progreso
            yield return null;
        }
    }

    private bool EscenaExisteEnBuild(string nombre)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) 
        {
            string rutaEscena = SceneUtility.GetScenePathByBuildIndex(i);
            string nombreEscenaEnBuild = System.IO.Path.GetFileNameWithoutExtension(rutaEscena); // Obtener nombre sin extensión

            if (nombreEscenaEnBuild == nombre)
                return true;
        }
        return false;
    }

    public void AsignarEscena(string nuevaEscena)
    {
        nombreEscena = nuevaEscena; //Permitir asignar escena desde código
    }
}