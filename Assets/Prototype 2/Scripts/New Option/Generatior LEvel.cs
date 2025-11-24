using UnityEngine;
using System.Collections.Generic;

public class GeneratiorLEvel : MonoBehaviour
{
    [Header("Configuración de Niveles")]
    [SerializeField] private float distanciaActivacion = 10f;
    [SerializeField] private int cantidadInicial = 3;
    [SerializeField] private int maxNivelesActivos = 3; // Nueva variable para limitar niveles
    [SerializeField] private Transform puntoFinal;

    [Header("Prefabs de Niveles")]
    [SerializeField] private List<GameObject> nivelesList = new List<GameObject>();

    private Transform jugador;
    private bool inicializado = false;
    private Queue<GameObject> nivelesActivos = new Queue<GameObject>(); // Cola para gestionar niveles activos

    // Propiedad para acceder a los niveles de forma segura
    private GameObject[] Niveles
    {
        get
        {
            if (nivelesList == null) return new GameObject[0];
            return nivelesList.ToArray();
        }
    }

    private void Start()
    {
        // Buscar jugador de forma segura
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player 1");
        if (playerObj != null)
        {
            jugador = playerObj.transform;
        }
        else
        {
            Debug.LogError("No se encontró objeto con tag 'Player 1'");
            return;
        }

        // Verificar que puntoFinal esté asignado
        if (puntoFinal == null)
        {
            Debug.LogError("puntoFinal no está asignado en el inspector");
            return;
        }

        // Validar que tenemos prefabs
        if (nivelesList == null || nivelesList.Count == 0)
        {
            Debug.LogError("La lista de niveles está vacía o no asignada");
            return;
        }

        // Limpiar nulls de la lista
        nivelesList.RemoveAll(item => item == null);

        if (nivelesList.Count == 0)
        {
            Debug.LogError("No hay prefabs válidos en la lista de niveles");
            return;
        }

        // Generar niveles iniciales
        for (int i = 0; i < cantidadInicial; i++)
        {
            GenerarNivel();
        }

        inicializado = true;
        Debug.Log($"Generador inicializado con {nivelesList.Count} prefabs de niveles");
    }

    private void Update()
    {
        // Verificar que todo esté inicializado antes de continuar
        if (!inicializado || jugador == null || puntoFinal == null) return;

        // Generar nuevo nivel cuando el jugador esté cerca del punto final
        if (Vector2.Distance(jugador.position, puntoFinal.position) < distanciaActivacion)
        {
            GenerarNivel();
        }
    }

    private void GenerarNivel()
    {
        if (nivelesList == null || nivelesList.Count == 0)
        {
            Debug.LogError("Lista de niveles está vacía");
            return;
        }

        // Filtrar nulls por si acaso
        List<GameObject> prefabsValidos = new List<GameObject>();
        foreach (var prefab in nivelesList)
        {
            if (prefab != null) prefabsValidos.Add(prefab);
        }

        if (prefabsValidos.Count == 0)
        {
            Debug.LogError("No hay prefabs válidos disponibles");
            return;
        }

        int indiceNivel = Random.Range(0, prefabsValidos.Count);
        GameObject prefabSeleccionado = prefabsValidos[indiceNivel];

        // El nuevo nivel se genera en la posición del punto final actual
        GameObject nuevoNivel = Instantiate(prefabSeleccionado, puntoFinal.position, Quaternion.identity);

        // Agregar el nuevo nivel a la cola de niveles activos
        nivelesActivos.Enqueue(nuevoNivel);

        // Buscar nuevo punto final en el nivel recién generado
        Transform nuevoPuntoFinal = BuscarPuntoFinal(nuevoNivel, "PuntoFinal");
        if (nuevoPuntoFinal != null)
        {
            puntoFinal = nuevoPuntoFinal;
            Debug.Log($"Nivel generado: {prefabSeleccionado.name}, nuevo punto final asignado");
        }
        else
        {
            Debug.LogError($"No se encontró PuntoFinal en el nivel generado: {nuevoNivel.name}");
        }

        // Eliminar niveles excedentes
        EliminarNivelesExcedentes();
    }

    private void EliminarNivelesExcedentes()
    {
        // Si tenemos más niveles de los permitidos, eliminar los más antiguos
        while (nivelesActivos.Count > maxNivelesActivos)
        {
            GameObject nivelAEliminar = nivelesActivos.Dequeue();
            if (nivelAEliminar != null)
            {
                Destroy(nivelAEliminar);
                Debug.Log($"Nivel eliminado: {nivelAEliminar.name}");
            }
        }
    }

    private Transform BuscarPuntoFinal(GameObject nivel, string etiqueta)
    {
        if (nivel == null) return null;

        foreach (Transform ubicacion in nivel.transform)
        {
            if (ubicacion.CompareTag(etiqueta))
            {
                return ubicacion;
            }
        }
        return null;
    }

    // Métodos públicos para gestionar la lista desde otros scripts si es necesario
    public void AgregarNivel(GameObject nuevoNivel)
    {
        if (nuevoNivel != null && !nivelesList.Contains(nuevoNivel))
        {
            nivelesList.Add(nuevoNivel);
        }
    }

    public void RemoverNivel(GameObject nivel)
    {
        if (nivel != null)
        {
            nivelesList.Remove(nivel);
        }
    }

    public void LimpiarLista()
    {
        nivelesList.Clear();
    }

    // Método para limpiar todos los niveles activos (útil al reiniciar el juego)
    public void LimpiarNivelesActivos()
    {
        while (nivelesActivos.Count > 0)
        {
            GameObject nivel = nivelesActivos.Dequeue();
            if (nivel != null)
            {
                Destroy(nivel);
            }
        }
    }
}