using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HealthP : MonoBehaviour
{
    [Header("Configuración de Salud")]
    public int saludMaxima = 100;
    public int saludActual;
    public bool esChasis = false;

    [Header("Referencias 3D")]
    public TextMeshPro textoSalud3D;

    [Header("Debug")]
    public bool mostrarLogsDamage = true;

    [Header("Mejoras")]
    public string upgradeLifeButtonName = "UpgradeLifeButton";
    public string upgradeLifeCostTextName = "UpgradeLifeCostText";
    private Button upgradeLifeButton;
    private TextMeshProUGUI upgradeLifeCostText;
    public int upgradeLevel = 0;
    public int maxUpgradeLevel = 4;
    public int healthIncreasePerLevel = 20;
    public int upgradeBaseCost = 30;
    public int upgradeIncrement = 10;

    private GameManager gameManager;

    void Start()
    {
        BuscarReferenciasMejoras();

        if (upgradeLifeButton != null)
        {
            upgradeLifeButton.onClick.AddListener(UpgradeLife);
            upgradeLifeButton.gameObject.SetActive(!MoneyManager.Instance.isPlaying && upgradeLevel < maxUpgradeLevel);
        }

        saludActual = saludMaxima;
        gameManager = FindObjectOfType<GameManager>();

        if (esChasis && !gameObject.CompareTag("Chasis"))
        {
            gameObject.tag = "Chasis";
        }
        else if (!esChasis && !gameObject.CompareTag("Modulo"))
        {
            gameObject.tag = "Modulo";
        }

        ConfigurarTextoSalud();
        ActualizarUI();

        Debug.Log($"HealthP inicializado: {gameObject.name} - Tag: {gameObject.tag} - Salud: {saludActual}/{saludMaxima}");
    }

    void Update()
    {
        if (upgradeLifeButton != null)
        {
            upgradeLifeButton.gameObject.SetActive(!MoneyManager.Instance.isPlaying && upgradeLevel < maxUpgradeLevel);
        }

        if (upgradeLifeCostText != null)
        {
            upgradeLifeCostText.text = GetUpgradeCost().ToString();
        }
    }

    private void BuscarReferenciasMejoras()
    {
        upgradeLifeButton = BuscarBotonPorNombre(upgradeLifeButtonName);
        upgradeLifeCostText = BuscarTextoPorNombre(upgradeLifeCostTextName);
    }

    private Button BuscarBotonPorNombre(string nombre)
    {
        Button[] todosBotones = FindObjectsOfType<Button>(true);
        foreach (Button boton in todosBotones)
        {
            if (boton.name == nombre) return boton;
        }
        return null;
    }

    private TextMeshProUGUI BuscarTextoPorNombre(string nombre)
    {
        TextMeshProUGUI[] todosTextos = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI texto in todosTextos)
        {
            if (texto.name == nombre) return texto;
        }
        return null;
    }

    private void UpgradeLife()
    {
        int cost = GetUpgradeCost();
        if (MoneyManager.Instance.SpendMoney(cost) && upgradeLevel < maxUpgradeLevel)
        {
            upgradeLevel++;
            saludMaxima += healthIncreasePerLevel;
            saludActual = saludMaxima; // Ahora también cura completamente al mejorar
            ActualizarUI();
            Debug.Log($"Vida mejorada! Nuevo máximo: {saludMaxima}, Vida actual: {saludActual}");
        }
    }

    private int GetUpgradeCost()
    {
        return upgradeBaseCost + (upgradeLevel * upgradeIncrement);
    }

    // NUEVO MÉTODO: Restablece la vida al máximo (incluyendo mejoras)
    public void RestablecerVida()
    {
        saludActual = saludMaxima;
        ActualizarUI();
        Debug.Log($"{gameObject.name} - Vida restablecida: {saludActual}/{saludMaxima}");
    }

    private void ConfigurarTextoSalud()
    {
        if (textoSalud3D == null)
        {
            textoSalud3D = GetComponentInChildren<TextMeshPro>();
        }

        if (textoSalud3D == null)
        {
            CrearTextoSaludAutomatico();
        }
    }

    private void CrearTextoSaludAutomatico()
    {
        GameObject textoObj = new GameObject("TextoSalud");
        textoObj.transform.SetParent(transform);
        textoObj.transform.localPosition = new Vector3(0, 0.8f, 0);
        textoObj.transform.localRotation = Quaternion.identity;

        textoSalud3D = textoObj.AddComponent<TextMeshPro>();
        textoSalud3D.text = $"{saludActual}/{saludMaxima}";
        textoSalud3D.fontSize = 2;
        textoSalud3D.alignment = TextAlignmentOptions.Center;
        textoSalud3D.sortingOrder = 10;
    }

    public void RecibirDanio(int cantidad)
    {
        saludActual -= cantidad;

        if (mostrarLogsDamage)
        {
            Debug.Log($"{gameObject.name} recibió {cantidad} daño. Salud: {saludActual}/{saludMaxima}");
        }

        if (saludActual <= 0)
        {
            saludActual = 0;
            Morir();
        }

        ActualizarUI();
    }

    private void Morir()
    {
        if (esChasis)
        {
            Debug.Log("CHASIS DESTRUIDO - REINICIANDO A LOBBY");

            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.ResetToLobby();
            }
        }
        else
        {
            Debug.Log($"Módulo {gameObject.name} destruido");
            if (gameManager != null)
            {
                gameManager.EliminarModulo(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void ActualizarUI()
    {
        if (textoSalud3D != null)
        {
            textoSalud3D.text = $"{saludActual}/{saludMaxima}";

            float porcentaje = (float)saludActual / saludMaxima;
            if (porcentaje < 0.3f)
                textoSalud3D.color = Color.red;
            else if (porcentaje < 0.6f)
                textoSalud3D.color = Color.yellow;
            else
                textoSalud3D.color = Color.green;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (esChasis)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, new Vector3(1.5f, 1.5f, 0.1f));
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(1.2f, 1.2f, 0.1f));
        }
    }
}