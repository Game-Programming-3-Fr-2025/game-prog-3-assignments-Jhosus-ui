using UnityEngine;

public class GeneradorLevel : MonoBehaviour
{
    [SerializeField] private GameObject[] niveles;
    [SerializeField] private float distanciaActivacion = 10f; // Distancia para activar generación
    [SerializeField] private Transform puntoFinal;
    [SerializeField] private int CantidadIncial;

    private Transform jugador;
    private bool inicializado = false;

    private void Start()
    {
        // Buscar jugador de forma segura
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            jugador = playerObj.transform;
        }
        else
        {
            Debug.LogError("No se encontró objeto con tag 'Player'");
            return;
        }

        // Verificar que puntoFinal esté asignado
        if (puntoFinal == null)
        {
            Debug.LogError("puntoFinal no está asignado en el inspector");
            return;
        }

        // Generar niveles iniciales
        for (int i = 0; i < CantidadIncial; i++)
        {
            GenerarNivel();
        }

        inicializado = true;
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
        if (niveles == null || niveles.Length == 0)
        {
            Debug.LogError("Array de niveles está vacío o no asignado");
            return;
        }

        int indiceNivel = Random.Range(0, niveles.Length);

        // El nuevo nivel se genera en la posición del punto final actual
        // (la separación física entre niveles debe estar definida en los prefabs)
        GameObject nuevoNivel = Instantiate(niveles[indiceNivel], puntoFinal.position, Quaternion.identity);

        // Buscar nuevo punto final en el nivel recién generado
        Transform nuevoPuntoFinal = BuscarPuntoFinal(nuevoNivel, "PuntoFinal");
        if (nuevoPuntoFinal != null)
        {
            puntoFinal = nuevoPuntoFinal;
        }
        else
        {
            Debug.LogError("No se encontró PuntoFinal en el nivel generado: " + nuevoNivel.name);
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

    // Método para debug visual
    private void OnDrawGizmos()
    {
        if (puntoFinal != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(puntoFinal.position, distanciaActivacion);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(puntoFinal.position, 0.5f);
        }
    }
}