using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BotonCargarEscena : MonoBehaviour
{
    [Header("Configuración de Escena")]
    public Object escenaParaCargar; // Arrastra el archivo .unity desde el Project

    private Button boton;

    void Start()
    {
        // Obtener el componente Button
        boton = GetComponent<Button>();

        // Configurar el evento onClick
        if (boton != null && escenaParaCargar != null)
        {
            boton.onClick.AddListener(CargarEscena);
        }
        else
        {
            Debug.LogWarning("Falta asignar el botón o la escena");
        }
    }

    void CargarEscena()
    {
        if (escenaParaCargar != null)
        {
            // Obtener el nombre de la escena desde el objeto arrastrado
            string nombreEscena = escenaParaCargar.name;

            // Cargar la escena
            SceneManager.LoadScene(nombreEscena);

            Debug.Log("Cargando escena: " + nombreEscena);
        }
        else
        {
            Debug.LogError("No se ha asignado una escena para cargar");
        }
    }

    // Método público para asignar escena desde otro script si es necesario
    public void AsignarEscena(Object nuevaEscena)
    {
        escenaParaCargar = nuevaEscena;
    }
}