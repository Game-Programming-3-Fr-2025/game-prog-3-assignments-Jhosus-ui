using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class UPManager7 : MonoBehaviour
{
    [Header("Referencias UI")]
    public Button btnVelocidadDisparo;
    public Button btnAreaDisparo;
    public Button btnDamageCritico;
    public Button btnAreaDamage;
    public Button btnLife;
    public Button btnLifeRegeneration;

    [Header("Textos UI")]
    public TMP_Text txtVelocidadDisparo;
    public TMP_Text txtAreaDisparo;
    public TMP_Text txtDamageCritico;
    public TMP_Text txtAreaDamage;
    public TMP_Text txtLife;
    public TMP_Text txtLifeRegeneration;
    public TMP_Text txtRequisitoActual;

    [Header("Configuración")]
    public int requisitoBase = 5;
    public float multiplicadorRequisito = 1.6f;

    // Niveles actuales de cada mejora
    private int nivelVelocidadDisparo = 0;
    private int nivelAreaDisparo = 0;
    private int nivelDamageCritico = 0;
    private int nivelAreaDamage = 0;
    private int nivelLife = 0;
    private int nivelLifeRegeneration = 0;

    // Requisito actual de EXP para la próxima mejora
    private int requisitoActual;

    // Valores base por nivel (según tu tabla)
    private readonly float[,] valoresMejoras = {
        // Nivel 0, 1, 2, 3, 4
        {1.80f, 1.40f, 1.00f, 0.80f, 0.40f},    // V. Disparo
        {4.00f, 4.50f, 5.00f, 5.50f, 6.00f},    // A. Disparo
        {6.00f, 12.00f, 18.00f, 24.00f, 30.00f}, // DMA Critic (%)
        {0.00f, 0.75f, 1.50f, 2.25f, 3.00f},    // DMA Area
        {3.00f, 4.00f, 4.00f, 6.00f, 7.00f},    // Life
        {0.00f, 20.00f, 16.00f, 12.00f, 8.00f}  // Reg Life (segundos por corazón)
    };

    private const int MAX_NIVEL = 4;
    private int totalMejorasCompradas = 0;

    // Referencias
    private GameManager7 gameManager;
    private Combate7 combatePlayer;
    private Health7 playerHealth;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager7>();
        combatePlayer = FindObjectOfType<Combate7>();
        requisitoActual = requisitoBase;
        playerHealth = FindObjectOfType<Health7>();

        // Configurar listeners de los botones
        btnVelocidadDisparo.onClick.AddListener(() => ComprarMejora(MejoraType.VelocidadDisparo));
        btnAreaDisparo.onClick.AddListener(() => ComprarMejora(MejoraType.AreaDisparo));
        btnDamageCritico.onClick.AddListener(() => ComprarMejora(MejoraType.DamageCritico));
        btnAreaDamage.onClick.AddListener(() => ComprarMejora(MejoraType.AreaDamage));
        btnLife.onClick.AddListener(() => ComprarMejora(MejoraType.Life));
        btnLifeRegeneration.onClick.AddListener(() => ComprarMejora(MejoraType.LifeRegeneration));

        ActualizarUI();
    }

    void Update()
    {
        // Actualizar estado de botones según EXP actual
        bool puedeComprar = gameManager != null && gameManager.GetCurrentEXP() >= requisitoActual;

        btnVelocidadDisparo.interactable = puedeComprar && nivelVelocidadDisparo < MAX_NIVEL;
        btnAreaDisparo.interactable = puedeComprar && nivelAreaDisparo < MAX_NIVEL;
        btnDamageCritico.interactable = puedeComprar && nivelDamageCritico < MAX_NIVEL;
        btnAreaDamage.interactable = puedeComprar && nivelAreaDamage < MAX_NIVEL;
        btnLife.interactable = puedeComprar && nivelLife < MAX_NIVEL;
        btnLifeRegeneration.interactable = puedeComprar && nivelLifeRegeneration < MAX_NIVEL;

        // Actualizar texto del requisito
        if (txtRequisitoActual != null)
        {
            txtRequisitoActual.text = $"REQ: {requisitoActual} exp";
        }
    }

    enum MejoraType
    {
        VelocidadDisparo,
        AreaDisparo,
        DamageCritico,
        AreaDamage,
        Life,
        LifeRegeneration
    }

    void ComprarMejora(MejoraType tipo)
    {
        if (gameManager == null || gameManager.GetCurrentEXP() < requisitoActual) return;

        int nivelActual = GetNivel(tipo);
        if (nivelActual >= MAX_NIVEL) return;

        // Aplicar la mejora
        AplicarMejora(tipo);

        // Aumentar el requisito para la próxima mejora
        totalMejorasCompradas++;
        requisitoActual = Mathf.RoundToInt(requisitoBase * Mathf.Pow(multiplicadorRequisito, totalMejorasCompradas));

        ActualizarUI();
        Debug.Log($"Mejora comprada: {tipo}. Nuevo requisito: {requisitoActual} EXP");
    }

    int GetNivel(MejoraType tipo)
    {
        switch (tipo)
        {
            case MejoraType.VelocidadDisparo: return nivelVelocidadDisparo;
            case MejoraType.AreaDisparo: return nivelAreaDisparo;
            case MejoraType.DamageCritico: return nivelDamageCritico;
            case MejoraType.AreaDamage: return nivelAreaDamage;
            case MejoraType.Life: return nivelLife;
            case MejoraType.LifeRegeneration: return nivelLifeRegeneration;
            default: return 0;
        }
    }

    void AplicarMejora(MejoraType tipo)
    {
        int nuevoNivel = 0;

        switch (tipo)
        {
            case MejoraType.VelocidadDisparo:
                nivelVelocidadDisparo++;
                nuevoNivel = nivelVelocidadDisparo;
                if (combatePlayer != null)
                {
                    combatePlayer.ActualizarVelocidadDisparo(valoresMejoras[0, nuevoNivel]);
                }
                break;

            case MejoraType.AreaDisparo:
                nivelAreaDisparo++;
                nuevoNivel = nivelAreaDisparo;
                if (combatePlayer != null)
                {
                    combatePlayer.ActualizarAreaDisparo(valoresMejoras[1, nuevoNivel]);
                }
                break;

            case MejoraType.DamageCritico:
                nivelDamageCritico++;
                nuevoNivel = nivelDamageCritico;
                if (combatePlayer != null)
                {
                    combatePlayer.ActualizarCritico(valoresMejoras[2, nuevoNivel]);
                }
                break;

            case MejoraType.AreaDamage:
                nivelAreaDamage++;
                nuevoNivel = nivelAreaDamage;
                if (combatePlayer != null)
                {
                    combatePlayer.ActualizarDamageArea(valoresMejoras[3, nuevoNivel], valoresMejoras[3, nuevoNivel]);
                }
                break;


            case MejoraType.Life:
                nivelLife++;
                nuevoNivel = nivelLife;
                if (playerHealth != null)
                {
                    playerHealth.ActualizarVidaMaxima((int)valoresMejoras[4, nuevoNivel]);
                }
                break;

            case MejoraType.LifeRegeneration:
                nivelLifeRegeneration++;
                nuevoNivel = nivelLifeRegeneration;
                if (playerHealth != null)
                {
                    playerHealth.ActualizarRegeneracion(valoresMejoras[5, nuevoNivel]);
                }
                break;
        }
    }

    void ActualizarUI()
    {
        // Actualizar textos de niveles
        txtVelocidadDisparo.text = $"V. Fire: {valoresMejoras[0, nivelVelocidadDisparo]}s\nLevel: {nivelVelocidadDisparo}/4";
        txtAreaDisparo.text = $"D. Fire: {valoresMejoras[1, nivelAreaDisparo]}m\nLevel: {nivelAreaDisparo}/4";
        txtDamageCritico.text = $"DMA Critic: {valoresMejoras[2, nivelDamageCritico]}%\nLevel: {nivelDamageCritico}/4";
        txtAreaDamage.text = $"DMA Area: {valoresMejoras[3, nivelAreaDamage]}m\nLevel: {nivelAreaDamage}/4";
        txtLife.text = $"N. Life: 0{valoresMejoras[4, nivelLife]}\nLevel: {nivelLife}/4";
        txtLifeRegeneration.text = $"Reg. Life: 1/{valoresMejoras[5, nivelLifeRegeneration]}s\nLevel: {nivelLifeRegeneration}/4";

        // Actualizar requisito
        if (txtRequisitoActual != null)
        {
            txtRequisitoActual.text = $"REQ: {requisitoActual} exp";
        }
    }

    // Métodos para consultar niveles (para otros scripts)
    public int GetNivelVelocidadDisparo() => nivelVelocidadDisparo;
    public int GetNivelAreaDisparo() => nivelAreaDisparo;
    public int GetNivelDamageCritico() => nivelDamageCritico;
    public int GetNivelAreaDamage() => nivelAreaDamage;
    public int GetNivelLife() => nivelLife;
    public int GetNivelLifeRegeneration() => nivelLifeRegeneration;
    public int GetRequisitoActual() => requisitoActual;
}