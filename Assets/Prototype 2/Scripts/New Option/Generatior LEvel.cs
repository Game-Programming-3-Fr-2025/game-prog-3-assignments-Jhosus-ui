using UnityEngine;
using System.Collections.Generic;

public class GeneratiorLEvel : MonoBehaviour
{
    [Header("Configuración de Niveles")]
    [SerializeField] private float distanciaActivacion = 10f;
    [SerializeField] private int cantidadInicial = 3;
    [SerializeField] private int maxNivelesActivos = 3;
    [SerializeField] private Transform puntoFinal;

    [Header("Transición a Horizontal")]
    [SerializeField] private int nivelesAntesDeTransicion = 5;
    [SerializeField] private GameObject prefabTransicion;

    [Header("Prefabs de Niveles")]
    [SerializeField] private List<GameObject> nivelesVerticales = new List<GameObject>();
    [SerializeField] private List<GameObject> nivelesHorizontales = new List<GameObject>();

    private Transform jugador;
    private bool inicializado = false;
    private bool transicionRealizada = false;
    private bool usarHorizontal = false;
    private Queue<GameObject> nivelesActivos = new Queue<GameObject>();
    private List<GameObject> nivelesListActual;
    private int contadorNivelesGenerados = 0;

    private void Start()
    {
        nivelesListActual = new List<GameObject>(nivelesVerticales);
        InicializarGenerador();
    }

    private void InicializarGenerador()
    {
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

        if (puntoFinal == null)
        {
            Debug.LogError("puntoFinal no está asignado en el inspector");
            return;
        }

        nivelesListActual.RemoveAll(item => item == null);
        if (nivelesListActual.Count == 0)
        {
            Debug.LogError("No hay prefabs válidos en la lista de niveles");
            return;
        }

        for (int i = 0; i < cantidadInicial; i++)
        {
            GenerarNivel();
        }

        inicializado = true;
        Debug.Log($"Generador inicializado. Niveles verticales: {nivelesVerticales.Count}, horizontales: {nivelesHorizontales.Count}");
    }

    private void Update()
    {
        if (!inicializado || jugador == null || puntoFinal == null) return;

        if (Vector2.Distance(jugador.position, puntoFinal.position) < distanciaActivacion)
        {
            GenerarNivel();
        }
    }

    private void GenerarNivel()
    {
        if (!transicionRealizada && contadorNivelesGenerados >= nivelesAntesDeTransicion && prefabTransicion != null)
        {
            GenerarTransicion();
            return;
        }

        if (nivelesListActual == null || nivelesListActual.Count == 0)
        {
            Debug.LogError("Lista de niveles está vacía");
            return;
        }

        List<GameObject> prefabsValidos = new List<GameObject>();
        foreach (var prefab in nivelesListActual)
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

        // Los prefabs horizontales ya están diseñados correctamente, sin rotación necesaria
        Quaternion rotacionGeneracion = Quaternion.identity;
        GameObject nuevoNivel = Instantiate(prefabSeleccionado, puntoFinal.position, rotacionGeneracion);

        nivelesActivos.Enqueue(nuevoNivel);
        contadorNivelesGenerados++;

        Transform nuevoPuntoFinal = BuscarPuntoFinal(nuevoNivel, "PuntoFinal");
        if (nuevoPuntoFinal != null)
        {
            puntoFinal = nuevoPuntoFinal;
            Debug.Log($"Nivel generado: {prefabSeleccionado.name} ({contadorNivelesGenerados}/{(transicionRealizada ? "Horizontal" : "Vertical")})");
        }
        else
        {
            Debug.LogError($"No se encontró PuntoFinal en el nivel generado: {nuevoNivel.name}");
        }

        EliminarNivelesExcedentes();
    }

    private void GenerarTransicion()
    {
        Debug.Log("Generando nivel de transición a horizontal");

        GameObject transicion = Instantiate(prefabTransicion, puntoFinal.position, Quaternion.identity);
        nivelesActivos.Enqueue(transicion);
        contadorNivelesGenerados++;

        Transform nuevoPuntoFinal = BuscarPuntoFinal(transicion, "PuntoFinal");
        if (nuevoPuntoFinal != null)
        {
            puntoFinal = nuevoPuntoFinal;
            usarHorizontal = true;
            transicionRealizada = true;
            nivelesListActual = new List<GameObject>(nivelesHorizontales);
            nivelesListActual.RemoveAll(item => item == null);

            Debug.Log($"Transición completada. Ahora generando niveles horizontales");
        }
        else
        {
            Debug.LogError("No se encontró PuntoFinal en el nivel de transición");
        }

        EliminarNivelesExcedentes();
    }

    private void EliminarNivelesExcedentes()
    {
        while (nivelesActivos.Count > maxNivelesActivos)
        {
            GameObject nivelAEliminar = nivelesActivos.Dequeue();
            if (nivelAEliminar != null)
            {
                Destroy(nivelAEliminar);
            }
        }
    }

    private Transform BuscarPuntoFinal(GameObject nivel, string etiqueta)
    {
        if (nivel == null) return null;
        return BuscarPuntoFinalRecursivo(nivel.transform, etiqueta);
    }

    private Transform BuscarPuntoFinalRecursivo(Transform parent, string etiqueta)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(etiqueta))
            {
                return child;
            }

            Transform resultado = BuscarPuntoFinalRecursivo(child, etiqueta);
            if (resultado != null)
            {
                return resultado;
            }
        }
        return null;
    }

    public void AgregarNivelVertical(GameObject nuevoNivel)
    {
        if (nuevoNivel != null && !nivelesVerticales.Contains(nuevoNivel))
        {
            nivelesVerticales.Add(nuevoNivel);
            if (!transicionRealizada)
            {
                nivelesListActual.Add(nuevoNivel);
            }
        }
    }

    public void AgregarNivelHorizontal(GameObject nuevoNivel)
    {
        if (nuevoNivel != null && !nivelesHorizontales.Contains(nuevoNivel))
        {
            nivelesHorizontales.Add(nuevoNivel);
            if (transicionRealizada)
            {
                nivelesListActual.Add(nuevoNivel);
            }
        }
    }

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

        transicionRealizada = false;
        usarHorizontal = false;
        contadorNivelesGenerados = 0;
        nivelesListActual = new List<GameObject>(nivelesVerticales);

        for (int i = 0; i < cantidadInicial; i++)
        {
            GenerarNivel();
        }
    }
}