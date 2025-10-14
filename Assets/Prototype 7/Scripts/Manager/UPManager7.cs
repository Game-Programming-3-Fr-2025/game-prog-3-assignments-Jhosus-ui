using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UPManager7 : MonoBehaviour
{
    [Header("Referencias UI")]
    public Button btnVelocidadDisparo;
    public Button btnAreaDisparo;
    public Button btnDamageCritico;
    public Button btnLife;
    public Button btnLifeRegeneration;
    public Button btnAreaDamage;

    [Header("Textos UI")]
    public TMP_Text txtVelocidadDisparo;
    public TMP_Text txtAreaDisparo;
    public TMP_Text txtDamageCritico;
    public TMP_Text txtLife;
    public TMP_Text txtLifeRegeneration;
    public TMP_Text txtAreaDamage;
    public TMP_Text txtEXPDisponible;

    [Header("Configuración de Costos")]
    public int costoBase = 5;
    public float multiplicadorCosto = 1.6f;

    // Niveles actuales de cada mejora
    private int nivelVelocidadDisparo = 0;
    private int nivelAreaDisparo = 0;
    private int nivelDamageCritico = 0;
    private int nivelLife = 0;
    private int nivelLifeRegeneration = 0;
    private int nivelAreaDamage = 0;

    // Máximo nivel por mejora
    private const int MAX_NIVEL = 5;

    // Referencia al GameManager para acceder al EXP
    private GameManager7 gameManager;
    private Combate7 combatePlayer;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager7>();
        combatePlayer = FindObjectOfType<Combate7>();

        // Configurar listeners de los botones
        btnVelocidadDisparo.onClick.AddListener(() => ComprarMejora(MejoraType.VelocidadDisparo));
        btnAreaDisparo.onClick.AddListener(() => ComprarMejora(MejoraType.AreaDisparo));
        btnDamageCritico.onClick.AddListener(() => ComprarMejora(MejoraType.DamageCritico));
        btnLife.onClick.AddListener(() => ComprarMejora(MejoraType.Life));
        btnLifeRegeneration.onClick.AddListener(() => ComprarMejora(MejoraType.LifeRegeneration));
        btnAreaDamage.onClick.AddListener(() => ComprarMejora(MejoraType.AreaDamage));

        ActualizarUI();
    }

    void Update()
    {
        // Actualizar EXP disponible constantemente
        if (gameManager != null)
        {
            // Necesitarías agregar un método público en GameManager7 para obtener el EXP actual
            // Por ahora asumimos que hay una variable pública o propiedad
            // txtEXPDisponible.text = $"EXP: {gameManager.GetCurrentEXP()}";
        }
    }

    enum MejoraType
    {
        VelocidadDisparo,
        AreaDisparo,
        DamageCritico,
        Life,
        LifeRegeneration,
        AreaDamage
    }

    void ComprarMejora(MejoraType tipo)
    {
        int nivelActual = GetNivel(tipo);
        int costo = CalcularCosto(nivelActual);

        if (gameManager.GetCurrentEXP() >= costo && nivelActual < MAX_NIVEL)
        {
            if (gameManager.GastarEXP(costo))
            {
                AplicarMejora(tipo);
                ActualizarUI();
            }
        }
    }

    int GetNivel(MejoraType tipo)
    {
        switch (tipo)
        {
            case MejoraType.VelocidadDisparo: return nivelVelocidadDisparo;
            case MejoraType.AreaDisparo: return nivelAreaDisparo;
            case MejoraType.DamageCritico: return nivelDamageCritico;
            case MejoraType.Life: return nivelLife;
            case MejoraType.LifeRegeneration: return nivelLifeRegeneration;
            case MejoraType.AreaDamage: return nivelAreaDamage;
            default: return 0;
        }
    }

    int CalcularCosto(int nivelActual)
    {
        if (nivelActual == 0) return costoBase;
        return Mathf.RoundToInt(costoBase * Mathf.Pow(multiplicadorCosto, nivelActual));
    }

    void AplicarMejora(MejoraType tipo)
    {
        switch (tipo)
        {
            case MejoraType.VelocidadDisparo:
                nivelVelocidadDisparo++;
                if (combatePlayer != null)
                {
                    // Reducir cadencia de disparo en 20% por nivel
                    combatePlayer.cadenciaDisparo *= 0.8f;
                }
                break;

            case MejoraType.AreaDisparo:
                nivelAreaDisparo++;
                if (combatePlayer != null)
                {
                    // Aumentar rango de detección en 25% por nivel
                    combatePlayer.rangoDeteccion *= 1.25f;
                }
                break;

            case MejoraType.DamageCritico:
                nivelDamageCritico++;
                if (combatePlayer != null)
                {
                    // Aumentar damage en 30% por nivel
                    combatePlayer.damage *= 1.3f;
                }
                break;

            case MejoraType.Life:
                nivelLife++;
                // Aquí necesitarías una referencia al sistema de vida del player
                // playerHealth.maxHealth += 20;
                break;

            case MejoraType.LifeRegeneration:
                nivelLifeRegeneration++;
                // Implementar regeneración de vida
                break;

            case MejoraType.AreaDamage:
                nivelAreaDamage++;
                // Implementar daño en área
                break;
        }
    }

    void ActualizarUI()
    {
        // Actualizar textos de niveles y costos
        txtVelocidadDisparo.text = $"Vel. Disparo: {nivelVelocidadDisparo}/5\nCosto: {CalcularCosto(nivelVelocidadDisparo)}";
        txtAreaDisparo.text = $"Área Disparo: {nivelAreaDisparo}/5\nCosto: {CalcularCosto(nivelAreaDisparo)}";
        txtDamageCritico.text = $"Daño Crítico: {nivelDamageCritico}/5\nCosto: {CalcularCosto(nivelDamageCritico)}";
        txtLife.text = $"Vida: {nivelLife}/5\nCosto: {CalcularCosto(nivelLife)}";
        txtLifeRegeneration.text = $"Regen Vida: {nivelLifeRegeneration}/5\nCosto: {CalcularCosto(nivelLifeRegeneration)}";
        txtAreaDamage.text = $"Daño Área: {nivelAreaDamage}/5\nCosto: {CalcularCosto(nivelAreaDamage)}";

        // Deshabilitar botones si alcanzó el nivel máximo
        btnVelocidadDisparo.interactable = nivelVelocidadDisparo < MAX_NIVEL;
        btnAreaDisparo.interactable = nivelAreaDisparo < MAX_NIVEL;
        btnDamageCritico.interactable = nivelDamageCritico < MAX_NIVEL;
        btnLife.interactable = nivelLife < MAX_NIVEL;
        btnLifeRegeneration.interactable = nivelLifeRegeneration < MAX_NIVEL;
        btnAreaDamage.interactable = nivelAreaDamage < MAX_NIVEL;
    }

    // Método para que otros scripts puedan consultar los niveles
    public int GetNivelVelocidadDisparo() => nivelVelocidadDisparo;
    public int GetNivelAreaDisparo() => nivelAreaDisparo;
    public int GetNivelDamageCritico() => nivelDamageCritico;
    public int GetNivelLife() => nivelLife;
    public int GetNivelLifeRegeneration() => nivelLifeRegeneration;
    public int GetNivelAreaDamage() => nivelAreaDamage;
}