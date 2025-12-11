using System.Collections.Generic;
using UnityEngine;

public class GeneratiorLEvel : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float distanciaActivacion = 10f;
    [SerializeField] private int cantidadInicial = 3;
    [SerializeField] private int maxNivelesActivos = 3;

    [Header("Puntos Finales")]
    [SerializeField] private Transform puntoFinalPlayer1;
    [SerializeField] private Transform puntoFinalPlayer2;

    [Header("Transiciones")]
    [SerializeField] private int nivelesAntesDeTransicion = 5;
    [SerializeField] private GameObject prefabTransicion;
    [SerializeField] private int nivelesHorizontalesAntesDeArriba = 5;
    [SerializeField] private GameObject prefabTransicionArriba;

    [Header("Prefabs")]
    [SerializeField] private List<GameObject> nivelesVerticales;
    [SerializeField] private List<GameObject> nivelesHorizontales;
    [SerializeField] private List<GameObject> nivelesVerticalesArriba;

    [Header("Referencias")]
    [SerializeField] private Transform jugador1;
    [SerializeField] private Transform jugador2;

    private Queue<GameObject> nivelesActivosP1 = new Queue<GameObject>();
    private Queue<GameObject> nivelesActivosP2 = new Queue<GameObject>();
    private List<GameObject> nivelesActuales;
    private int contadorNiveles = 0;
    private bool transicionHorizontal = false;
    private bool transicionArriba = false;
    private bool inicializado = false;
    private GameObject ultimoPrefabGenerado = null;

    void Start()
    {
        BuscarJugadores(); //Aseguramos que las referencias a los jugadores estén asignadas

        if (puntoFinalPlayer1 == null || puntoFinalPlayer2 == null)
        {
            Debug.LogError("Puntos finales no asignados");
            return;
        }

        nivelesActuales = new List<GameObject>(nivelesVerticales);
        nivelesActuales.RemoveAll(n => n == null);

        if (nivelesActuales.Count == 0)
        {
            Debug.LogError("No hay niveles válidos");
            return;
        }

        for (int i = 0; i < cantidadInicial; i++)
            GenerarNivelesParaAmbos();

        inicializado = true;
    }

    void Update() //Tengamos en cuenta que este Update se encarga de verificar la distancia de los jugadores a los puntos finales y generar niveles si es necesario.
    {
        if (!inicializado) return;

        bool p1Cerca = jugador1 != null &&
            Vector2.Distance(jugador1.position, puntoFinalPlayer1.position) < distanciaActivacion;

        bool p2Cerca = jugador2 != null &&
            Vector2.Distance(jugador2.position, puntoFinalPlayer2.position) < distanciaActivacion;

        if (p1Cerca || p2Cerca)
            GenerarNivelesParaAmbos();
    }

    void GenerarNivelesParaAmbos() //Generemos niveles para ambos jugadores
    {
        if (!transicionHorizontal && contadorNiveles >= nivelesAntesDeTransicion && prefabTransicion != null)
        {
            GenerarTransicionParaAmbos(prefabTransicion, nivelesHorizontales, ref transicionHorizontal); //Generamos la transición horizontal
            return;
        }

        if (transicionHorizontal && !transicionArriba &&
            contadorNiveles >= nivelesAntesDeTransicion + nivelesHorizontalesAntesDeArriba &&
            prefabTransicionArriba != null)
        {
            GenerarTransicionParaAmbos(prefabTransicionArriba, nivelesVerticalesArriba, ref transicionArriba); //Generamos la transición hacia arriba
            return;
        }

        if (nivelesActuales.Count == 0) return;

        GameObject prefab = ObtenerPrefabNoRepetido();
        Quaternion rotacion = transicionArriba ? Quaternion.Euler(0, 0, 0f) : Quaternion.identity; //Ajustamos la rotación si es necesario aunque no creo que haga falta

        GameObject nivelP1 = Instantiate(prefab, puntoFinalPlayer1.position, rotacion);
        nivelesActivosP1.Enqueue(nivelP1);

        GameObject nivelP2 = Instantiate(prefab, puntoFinalPlayer2.position, rotacion);
        nivelesActivosP2.Enqueue(nivelP2);

        ultimoPrefabGenerado = prefab;
        contadorNiveles++;

        ActualizarPuntoFinal(nivelP1, ref puntoFinalPlayer1, "PuntoFinalP1");
        ActualizarPuntoFinal(nivelP2, ref puntoFinalPlayer2, "PuntoFinalP2");

        LimpiarNivelesViejos();
    }

    GameObject ObtenerPrefabNoRepetido() //Evitemos repetir el mismo prefab consecutivamente
    {
        if (nivelesActuales.Count == 1)
            return nivelesActuales[0];

        GameObject prefabSeleccionado;
        int intentos = 0;
        const int maxIntentos = 10;

        do
        {
            prefabSeleccionado = nivelesActuales[Random.Range(0, nivelesActuales.Count)]; //Seleccion aleatoria
            intentos++;
        }
        while (prefabSeleccionado == ultimoPrefabGenerado && intentos < maxIntentos); //Evitar bucle infinito

        return prefabSeleccionado;
    }

    void GenerarTransicionParaAmbos(GameObject prefab, List<GameObject> nuevaLista, ref bool flag)
    {
        GameObject transicionP1 = Instantiate(prefab, puntoFinalPlayer1.position, Quaternion.identity); //Generamos la transición para el jugador 1
        nivelesActivosP1.Enqueue(transicionP1);

        GameObject transicionP2 = Instantiate(prefab, puntoFinalPlayer2.position, Quaternion.identity); //Generamos la transición para el jugador 2
        nivelesActivosP2.Enqueue(transicionP2);

        contadorNiveles++;

        ActualizarPuntoFinal(transicionP1, ref puntoFinalPlayer1, "PuntoFinalP1");
        ActualizarPuntoFinal(transicionP2, ref puntoFinalPlayer2, "PuntoFinalP2");

        flag = true;
        nivelesActuales = new List<GameObject>(nuevaLista); //Cambiamos la lista de niveles actuales
        nivelesActuales.RemoveAll(n => n == null);
        ultimoPrefabGenerado = null;

        LimpiarNivelesViejos();
    }

    void ActualizarPuntoFinal(GameObject nivel, ref Transform puntoFinal, string tagEspecifico) //Busquemos el punto final adecuado
    {
        Transform puntoEspecifico = BuscarEnHijos(nivel.transform, tagEspecifico);

        if (puntoEspecifico != null)
        {
            puntoFinal = puntoEspecifico;
            return;
        }

        Transform puntoGeneral = BuscarEnHijos(nivel.transform, "PuntoFinal");
        if (puntoGeneral != null)
            puntoFinal = puntoGeneral;
    }

    Transform BuscarEnHijos(Transform parent, string tag)
    {
        if (parent.CompareTag(tag)) return parent;

        foreach (Transform child in parent)
        {
            Transform result = BuscarEnHijos(child, tag);
            if (result != null) return result;
        }
        return null;
    }

    void LimpiarNivelesViejos() //Limpiemos los niveles viejos si excedemos el máximo permitido esto para evitar problemas de rendimiento
    {
        while (nivelesActivosP1.Count > maxNivelesActivos)
        {
            GameObject viejo = nivelesActivosP1.Dequeue();
            if (viejo != null) Destroy(viejo);
        }

        while (nivelesActivosP2.Count > maxNivelesActivos)
        {
            GameObject viejo = nivelesActivosP2.Dequeue();
            if (viejo != null) Destroy(viejo);
        }
    }

    void BuscarJugadores() //Aseguramos que las referencias a los jugadores estén agregadas o sino las buscamos por tag
    {
        if (jugador1 == null)
        {
            GameObject p1 = GameObject.FindGameObjectWithTag("Player 1");
            if (p1 != null) jugador1 = p1.transform;
        }

        if (jugador2 == null)
        {
            GameObject p2 = GameObject.FindGameObjectWithTag("Player 2");
            if (p2 != null) jugador2 = p2.transform;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        if (puntoFinalPlayer1 != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(puntoFinalPlayer1.position, distanciaActivacion);
        }

        if (puntoFinalPlayer2 != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(puntoFinalPlayer2.position, distanciaActivacion);
        }
    }

    public void ForzarGeneracionNivel() => GenerarNivelesParaAmbos(); //Método público para forzar la generación de niveles

    public void LimpiarTodo() //Util para reiniciar el generador de niveles
    {
        while (nivelesActivosP1.Count > 0)
        {
            GameObject nivel = nivelesActivosP1.Dequeue();
            if (nivel != null) Destroy(nivel);
        }

        while (nivelesActivosP2.Count > 0)
        {
            GameObject nivel = nivelesActivosP2.Dequeue();
            if (nivel != null) Destroy(nivel);
        }

        transicionHorizontal = false;
        transicionArriba = false;
        contadorNiveles = 0;
        ultimoPrefabGenerado = null;
        nivelesActuales = new List<GameObject>(nivelesVerticales);

        for (int i = 0; i < cantidadInicial; i++)
            GenerarNivelesParaAmbos();
    }
}