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

    [Header("Nueva: Transición a Arriba")]
    [SerializeField] private int nivelesHorizontalesAntesDeArriba = 5;
    [SerializeField] private GameObject prefabTransicionArriba;

    [Header("Prefabs de Niveles")]
    [SerializeField] private List<GameObject> nivelesVerticales = new List<GameObject>();
    [SerializeField] private List<GameObject> nivelesHorizontales = new List<GameObject>();
    [SerializeField] private List<GameObject> nivelesVerticalesArriba = new List<GameObject>();

    private Transform jugador;
    private bool inicializado = false;
    private bool transicionRealizada = false;
    private bool transicionArribaRealizada = false;
    private bool usarHorizontal = false;
    private bool usarArriba = false;
    private Queue<GameObject> nivelesActivos = new Queue<GameObject>();
    private List<GameObject> nivelesListActual;
    private int contadorNivelesGenerados = 0;
    private Vector3 puntoBaseArriba; // Para calcular desde arriba

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
        Debug.Log($"Generador inicializado. Niveles verticales: {nivelesVerticales.Count}, horizontales: {nivelesHorizontales.Count}, arriba: {nivelesVerticalesArriba.Count}");
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
        // Primera transición: Vertical → Horizontal
        if (!transicionRealizada && contadorNivelesGenerados >= nivelesAntesDeTransicion && prefabTransicion != null)
        {
            GenerarTransicionHorizontal();
            return;
        }

        // Segunda transición: Horizontal → Vertical Arriba
        if (transicionRealizada && !transicionArribaRealizada &&
            contadorNivelesGenerados >= nivelesAntesDeTransicion + nivelesHorizontalesAntesDeArriba &&
            prefabTransicionArriba != null)
        {
            GenerarTransicionArriba();
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

        // Calcular posición según la etapa
        Vector3 posicionGeneracion = puntoFinal.position;
        Quaternion rotacionGeneracion = Quaternion.identity;

        // Si estamos en la etapa "arriba", generar más arriba
        if (usarArriba)
        {
            // Guardar punto base para calcular desde arriba
            if (puntoBaseArriba == Vector3.zero)
            {
                puntoBaseArriba = puntoFinal.position;
            }

            // Generar más arriba del punto base
            posicionGeneracion = new Vector3(
                puntoFinal.position.x,
                puntoBaseArriba.y + 15f, // Altura fija para niveles de arriba
                puntoFinal.position.z
            );

            // Rotar 180 grados para que el nivel "baje"
            rotacionGeneracion = Quaternion.Euler(0, 0, 180f);
        }

        GameObject nuevoNivel = Instantiate(prefabSeleccionado, posicionGeneracion, rotacionGeneracion);

        nivelesActivos.Enqueue(nuevoNivel);
        contadorNivelesGenerados++;

        Transform nuevoPuntoFinal = BuscarPuntoFinal(nuevoNivel, "PuntoFinal");
        if (nuevoPuntoFinal != null)
        {
            puntoFinal = nuevoPuntoFinal;
            Debug.Log($"Nivel generado: {prefabSeleccionado.name} ({contadorNivelesGenerados}/{(usarArriba ? "Arriba" : (usarHorizontal ? "Horizontal" : "Vertical"))})");
        }
        else
        {
            Debug.LogError($"No se encontró PuntoFinal en el nivel generado: {nuevoNivel.name}");
        }

        EliminarNivelesExcedentes();
    }

    private void GenerarTransicionHorizontal()
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

            Debug.Log($"Transición a Horizontal completada. Ahora generando niveles horizontales");
        }
        else
        {
            Debug.LogError("No se encontró PuntoFinal en el nivel de transición");
        }

        EliminarNivelesExcedentes();
    }

    private void GenerarTransicionArriba()
    {
        Debug.Log("Generando nivel de transición a vertical arriba");

        GameObject transicion = Instantiate(prefabTransicionArriba, puntoFinal.position, Quaternion.identity);
        nivelesActivos.Enqueue(transicion);
        contadorNivelesGenerados++;

        Transform nuevoPuntoFinal = BuscarPuntoFinal(transicion, "PuntoFinal");
        if (nuevoPuntoFinal != null)
        {
            puntoFinal = nuevoPuntoFinal;
            usarArriba = true;
            transicionArribaRealizada = true;
            usarHorizontal = false;
            nivelesListActual = new List<GameObject>(nivelesVerticalesArriba);
            nivelesListActual.RemoveAll(item => item == null);

            Debug.Log($"Transición a Arriba completada. Ahora generando niveles verticales desde arriba");
        }
        else
        {
            Debug.LogError("No se encontró PuntoFinal en el nivel de transición arriba");
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
            if (transicionRealizada && !transicionArribaRealizada)
            {
                nivelesListActual.Add(nuevoNivel);
            }
        }
    }

    public void AgregarNivelVerticalArriba(GameObject nuevoNivel)
    {
        if (nuevoNivel != null && !nivelesVerticalesArriba.Contains(nuevoNivel))
        {
            nivelesVerticalesArriba.Add(nuevoNivel);
            if (transicionArribaRealizada)
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
        transicionArribaRealizada = false;
        usarHorizontal = false;
        usarArriba = false;
        contadorNivelesGenerados = 0;
        puntoBaseArriba = Vector3.zero;
        nivelesListActual = new List<GameObject>(nivelesVerticales);

        for (int i = 0; i < cantidadInicial; i++)
        {
            GenerarNivel();
        }
    }
}