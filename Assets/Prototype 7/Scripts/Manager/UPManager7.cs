using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UPManager7 : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button btnVelocidadDisparo;
    [SerializeField] private Button btnAreaDisparo;
    [SerializeField] private Button btnDamageCritico;
    [SerializeField] private Button btnAreaDamage;
    [SerializeField] private Button btnLife;
    [SerializeField] private Button btnLifeRegeneration;

    [Header("UI Text")]
    [SerializeField] private TMP_Text txtVelocidadDisparo;
    [SerializeField] private TMP_Text txtAreaDisparo;
    [SerializeField] private TMP_Text txtDamageCritico;
    [SerializeField] private TMP_Text txtAreaDamage;
    [SerializeField] private TMP_Text txtLife;
    [SerializeField] private TMP_Text txtLifeRegeneration;
    [SerializeField] private TMP_Text txtRequisitoActual;

    [Header("Settings")]
    [SerializeField] private int requisitoBase = 5;
    [SerializeField] private float multiplicadorRequisito = 1.6f;

    private int[] niveles = new int[6];
    private int requisitoActual;
    private int totalMejorasCompradas;

    private readonly float[,] valoresMejoras = {
        {1.80f, 1.40f, 1.00f, 0.80f, 0.40f},
        {4.00f, 4.50f, 5.00f, 5.50f, 6.00f},
        {6.00f, 12.00f, 18.00f, 24.00f, 30.00f},
        {0.00f, 0.75f, 1.50f, 2.25f, 3.00f},
        {3.00f, 4.00f, 4.00f, 6.00f, 7.00f},
        {0.00f, 20.00f, 16.00f, 12.00f, 8.00f}
    };// Mejoras: Vel.Disparo, AreaDisparo, DmgCritico, DmgArea, Vida, RegVida

    private const int MAX_NIVEL = 4;

    private GameManager7 gameManager;
    private Combate7 combatePlayer;
    private Health7 playerHealth;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager7>();
        combatePlayer = FindObjectOfType<Combate7>();
        playerHealth = FindObjectOfType<Health7>();
        requisitoActual = requisitoBase;

        ConfigureButtons();
        UpdateUI();
    }

    void Update()
    {
        bool canBuy = gameManager && gameManager.GetCurrentEXP() >= requisitoActual;
        // Activar o desactivar botones según si se puede comprar y si no se ha alcanzado el nivel máximo
        btnVelocidadDisparo.interactable = canBuy && niveles[0] < MAX_NIVEL;
        btnAreaDisparo.interactable = canBuy && niveles[1] < MAX_NIVEL;
        btnDamageCritico.interactable = canBuy && niveles[2] < MAX_NIVEL;
        btnAreaDamage.interactable = canBuy && niveles[3] < MAX_NIVEL;
        btnLife.interactable = canBuy && niveles[4] < MAX_NIVEL;
        btnLifeRegeneration.interactable = canBuy && niveles[5] < MAX_NIVEL;

        if (txtRequisitoActual)
            txtRequisitoActual.text = $"REQ: {requisitoActual} exp";
    }

    void ConfigureButtons()
    {
        // Asignar listeners a los botones
        btnVelocidadDisparo.onClick.AddListener(() => BuyUpgrade(0));
        btnAreaDisparo.onClick.AddListener(() => BuyUpgrade(1));
        btnDamageCritico.onClick.AddListener(() => BuyUpgrade(2));
        btnAreaDamage.onClick.AddListener(() => BuyUpgrade(3));
        btnLife.onClick.AddListener(() => BuyUpgrade(4));
        btnLifeRegeneration.onClick.AddListener(() => BuyUpgrade(5));
    }

    void BuyUpgrade(int upgradeIndex)
    {
        if (!gameManager || gameManager.GetCurrentEXP() < requisitoActual) return;
        if (niveles[upgradeIndex] >= MAX_NIVEL) return;

        ApplyUpgrade(upgradeIndex);

        totalMejorasCompradas++;
        requisitoActual = Mathf.RoundToInt(requisitoBase * Mathf.Pow(multiplicadorRequisito, totalMejorasCompradas));

        UpdateUI();
    }

    void ApplyUpgrade(int index)
    {
        niveles[index]++;
        int level = niveles[index];
        float value = valoresMejoras[index, level];

        switch (index) // Aplicar la mejora correspondiente
        {
            case 0: combatePlayer?.ActualizarVelocidadDisparo(value); break;
            case 1: combatePlayer?.ActualizarAreaDisparo(value); break;
            case 2: combatePlayer?.ActualizarCritico(value); break;
            case 3: combatePlayer?.ActualizarDamageArea(value, value); break;
            case 4: playerHealth?.ActualizarVidaMaxima((int)value); break;
            case 5: playerHealth?.ActualizarRegeneracion(value); break;
        }
    }

    void UpdateUI() // Actualizar los textos de la UI
    {
        txtVelocidadDisparo.text = $"V. Fire: {valoresMejoras[0, niveles[0]]}s\nLevel: {niveles[0]}/4";
        txtAreaDisparo.text = $"D. Fire: {valoresMejoras[1, niveles[1]]}m\nLevel: {niveles[1]}/4";
        txtDamageCritico.text = $"DMA Critic: {valoresMejoras[2, niveles[2]]}%\nLevel: {niveles[2]}/4";
        txtAreaDamage.text = $"DMA Area: {valoresMejoras[3, niveles[3]]}m\nLevel: {niveles[3]}/4";
        txtLife.text = $"N. Life: 0{valoresMejoras[4, niveles[4]]}\nLevel: {niveles[4]}/4";
        txtLifeRegeneration.text = $"Reg. Life: 1/{valoresMejoras[5, niveles[5]]}s\nLevel: {niveles[5]}/4";

        if (txtRequisitoActual)
            txtRequisitoActual.text = $"REQ: {requisitoActual} exp";
    }

    public int GetNivelVelocidadDisparo() => niveles[0];
    public int GetNivelAreaDisparo() => niveles[1];
    public int GetNivelDamageCritico() => niveles[2];
    public int GetNivelAreaDamage() => niveles[3];
    public int GetNivelLife() => niveles[4];
    public int GetNivelLifeRegeneration() => niveles[5];
    public int GetRequisitoActual() => requisitoActual;
}