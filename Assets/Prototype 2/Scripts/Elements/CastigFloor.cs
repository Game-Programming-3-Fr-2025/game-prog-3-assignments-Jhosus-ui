using UnityEngine;

public class CastigFloor : MonoBehaviour
{
    [Header("Sprites de Amenaza")]
    [SerializeField] private SpriteRenderer arriba;
    [SerializeField] private SpriteRenderer izquierda;
    [SerializeField] private SpriteRenderer abajo;

    private Camera cam;
    private int estado = 0; // 0=arriba, 1=izquierda, 2=abajo
    private float tiempoUltimoTrigger = -100f;

    void Start()
    {
        cam = GetComponent<Camera>();
        CambiarSprite(0); // Empezar con arriba
    }

    void Update()
    {
        ActualizarPosiciones();
    }

    void ActualizarPosiciones()
    {
        if (cam == null) return;

        Vector3 pos = transform.position;
        float altura = cam.orthographicSize;
        float ancho = altura * cam.aspect;

        if (arriba)
            arriba.transform.position = new Vector3(pos.x, pos.y + altura, arriba.transform.position.z);

        if (izquierda)
            izquierda.transform.position = new Vector3(pos.x - ancho, pos.y, izquierda.transform.position.z);

        if (abajo)
            abajo.transform.position = new Vector3(pos.x, pos.y - altura, abajo.transform.position.z);
    }

    void CambiarSprite(int nuevoEstado)
    {
        estado = nuevoEstado;

        // Apagar todos
        if (arriba) arriba.enabled = false;
        if (izquierda) izquierda.enabled = false;
        if (abajo) abajo.enabled = false;

        // Encender solo el actual
        switch (estado)
        {
            case 0: if (arriba) arriba.enabled = true; break;
            case 1: if (izquierda) izquierda.enabled = true; break;
            case 2: if (abajo) abajo.enabled = true; break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Horizontal"))
        {
            // Si pasaron más de 20 segundos desde el último trigger
            if (Time.time - tiempoUltimoTrigger > 20f)
            {
                // Cambiar al siguiente estado
                int nuevoEstado = (estado + 1) % 3; // 0→1→2→0
                CambiarSprite(nuevoEstado);
                tiempoUltimoTrigger = Time.time;

                Debug.Log($"{gameObject.name}: Cambió a {(nuevoEstado == 0 ? "ARRIBA" : nuevoEstado == 1 ? "IZQUIERDA" : "ABAJO")}");
            }
        }
    }
}