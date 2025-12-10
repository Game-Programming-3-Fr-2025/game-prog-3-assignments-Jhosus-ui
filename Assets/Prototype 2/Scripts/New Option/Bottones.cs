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
        // Obtener el componente Button
        boton = GetComponent<Button>();

        if (boton == null)
        {
            Debug.LogError($"[{gameObject.name}] No se encontró componente Button");
            return;
        }

        // Validar que la escena exista
        if (string.IsNullOrEmpty(nombreEscena))
        {
            Debug.LogError($"[{gameObject.name}] No se ha asignado un nombre de escena");
            return;
        }

        // Verificar si la escena está en Build Settings
        if (!EscenaExisteEnBuild(nombreEscena))
        {
            Debug.LogError($"[{gameObject.name}] La escena '{nombreEscena}' no está en Build Settings");
            return;
        }
    }

    void OnEnable()
    {
        // Suscribirse al evento cuando el objeto se activa
        if (boton != null)
        {
            boton.onClick.RemoveListener(CargarEscena); // Prevenir duplicados
            boton.onClick.AddListener(CargarEscena);
        }
    }

    void OnDisable()
    {
        // Desuscribirse cuando el objeto se desactiva
        if (boton != null)
        {
            boton.onClick.RemoveListener(CargarEscena);
        }
    }

    void CargarEscena()
    {
        if (string.IsNullOrEmpty(nombreEscena))
        {
            Debug.LogError("No se puede cargar: nombre de escena vacío");
            return;
        }

        Debug.Log($"Cargando escena: {nombreEscena}");

        try
        {
            SceneManager.LoadScene(nombreEscena);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al cargar escena '{nombreEscena}': {e.Message}");
        }
    }

    // Método para cargar escena de forma asíncrona (mejor para escenas pesadas)
    public void CargarEscenaAsync()
    {
        if (string.IsNullOrEmpty(nombreEscena))
        {
            Debug.LogError("No se puede cargar: nombre de escena vacío");
            return;
        }

        StartCoroutine(CargarEscenaAsyncCoroutine());
    }

    private System.Collections.IEnumerator CargarEscenaAsyncCoroutine()
    {
        Debug.Log($"Cargando escena asíncrona: {nombreEscena}");

        AsyncOperation operacion = SceneManager.LoadSceneAsync(nombreEscena);

        // Opcionalmente puedes mostrar una barra de progreso aquí
        while (!operacion.isDone)
        {
            float progreso = Mathf.Clamp01(operacion.progress / 0.9f);
            Debug.Log($"Progreso: {progreso * 100}%");
            yield return null;
        }
    }

    // Verificar si la escena existe en Build Settings
    private bool EscenaExisteEnBuild(string nombre)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string rutaEscena = SceneUtility.GetScenePathByBuildIndex(i);
            string nombreEscenaEnBuild = System.IO.Path.GetFileNameWithoutExtension(rutaEscena);

            if (nombreEscenaEnBuild == nombre)
            {
                return true;
            }
        }
        return false;
    }

    // Método público para asignar escena desde otro script
    public void AsignarEscena(string nuevaEscena)
    {
        nombreEscena = nuevaEscena;

        if (!EscenaExisteEnBuild(nuevaEscena))
        {
            Debug.LogWarning($"La escena '{nuevaEscena}' no está en Build Settings");
        }
    }
}