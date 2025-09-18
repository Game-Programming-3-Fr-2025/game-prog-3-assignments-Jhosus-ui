using UnityEngine;
using TMPro;
using System.Collections;

public class Player3 : MonoBehaviour
{
    [Header("Movimiento")] public float velocidad = 10f;

    [Header("Disparo")]
    public int balasCartucho = 6;
    public int balasTotal = 24;
    public float radioDeteccion = 3f;
    public float tiempoEntreDisparos = 0.2f;
    public float tiempoRecarga = 1.5f;
    public Transform puntoDisparo;

    [Header("Efectos Visuales")]
    public float fuerzaOscilacion = 0.1f;
    public float fuerzaRetroceso = 0.3f;

    [Header("UI")]
    public TextMeshProUGUI textoCartucho;
    public TextMeshProUGUI textoTotal;
    public TextMeshProUGUI textoTiempoRecarga;
    public GameObject panelRecarga;

    private int balasActuales;
    private bool puedeDisparar = true;
    private bool estaRecargando = false;
    private Vector3 posicionReal;
    private Vector3 offsetVisual;

    void Start()
    {
        balasActuales = balasCartucho;
        posicionReal = transform.position;
        ActualizarUI();
        OcultarUIRecarga();
    }

    void Update()
    {
        Mover();
        AplicarEfectosVisuales();

        if (Input.GetKeyDown(KeyCode.Space) && puedeDisparar)
            Disparar();

        if (Input.GetKeyDown(KeyCode.R) && !estaRecargando)
            StartCoroutine(Recargar());
    }

    void Mover()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontal != 0 || vertical != 0)
        {
            Vector3 direccion = new Vector3(horizontal, vertical, 0).normalized;
            posicionReal += direccion * velocidad * Time.deltaTime;
        }
    }

    void AplicarEfectosVisuales()
    {
        float oscilacionX = Mathf.Sin(Time.time * 2f) * fuerzaOscilacion * 0.5f;
        float oscilacionY = Mathf.Cos(Time.time * 1.8f) * fuerzaOscilacion;
        Vector3 oscilacion = new Vector3(oscilacionX, oscilacionY, 0);

        transform.position = posicionReal + oscilacion + offsetVisual;
        offsetVisual = Vector3.Lerp(offsetVisual, Vector3.zero, 8f * Time.deltaTime);
    }

    void Disparar()
    {
        if (balasActuales <= 0)
        {
            if (!estaRecargando) StartCoroutine(Recargar());
            return;
        }

        balasActuales--;
        ActualizarUI();
        offsetVisual += (Vector3)Random.insideUnitCircle * fuerzaRetroceso;

        Collider2D[] objetivos = Physics2D.OverlapCircleAll(puntoDisparo.position, radioDeteccion);
        foreach (Collider2D objetivo in objetivos)
        {
            if (objetivo.CompareTag("Aldeano") || objetivo.CompareTag("Infected"))
            {
                People person = objetivo.GetComponent<People>();
                DeatthAldean death = objetivo.GetComponent<DeatthAldean>();

                if (death != null)
                {
                    death.TakeDamage(1);

                    if (person != null)
                    {
                        TPI tpi = FindObjectOfType<TPI>();
                        if (tpi != null)
                        {
                            tpi.RegisterKill(person.currentState);
                        }
                    }
                }
                break;
            }
        }

        StartCoroutine(CooldownDisparo());
    }

    IEnumerator CooldownDisparo()
    {
        puedeDisparar = false;
        yield return new WaitForSeconds(tiempoEntreDisparos);
        puedeDisparar = true;
    }

    IEnumerator Recargar()
    {
        if (balasActuales >= balasCartucho || balasTotal <= 0) yield break;

        estaRecargando = true;
        MostrarUIRecarga();
        float tiempoRestante = tiempoRecarga;

        while (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            if (textoTiempoRecarga) textoTiempoRecarga.text = $"Reload... {tiempoRestante:.0}s";
            yield return null;
        }

        int recargar = Mathf.Min(balasCartucho - balasActuales, balasTotal);
        balasActuales += recargar;
        balasTotal -= recargar;

        ActualizarUI();
        OcultarUIRecarga();
        estaRecargando = false;
    }

    void MostrarUIRecarga()
    {
        if (panelRecarga) panelRecarga.SetActive(true);
        if (textoTiempoRecarga) textoTiempoRecarga.gameObject.SetActive(true);
    }

    void OcultarUIRecarga()
    {
        if (panelRecarga) panelRecarga.SetActive(false);
        if (textoTiempoRecarga) textoTiempoRecarga.gameObject.SetActive(false);
    }

    void ActualizarUI()
    {
        if (textoCartucho) textoCartucho.text = $"{balasActuales}/{balasCartucho}";
        if (textoTotal) textoTotal.text = balasTotal.ToString();
    }

    void OnDrawGizmos()
    {
        if (!puntoDisparo) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoDisparo.position, radioDeteccion);
    }
}