using UnityEngine;
using TMPro;

public class PlayerScore : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text coinsText;
    public TMP_Text distanceText;
    public TMP_Text totalText;

    [Header("Settings")]
    public bool isPlayer1 = true;
    public int coinsMultiplier = 10;
    public float distanceMultiplier = 0.1f;

    private int coins = 0;
    private float distance = 0f;
    private Vector3 lastPosition;
    private bool isCounting = false;

    void Start()
    {
        lastPosition = transform.position;
        UpdateUI();
    }

    void Update()
    {
        if (isCounting)
        {
            // Calcular distancia desde la última posición
            float moved = Vector3.Distance(transform.position, lastPosition);
            distance += moved;
            lastPosition = transform.position;

            UpdateUI();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Detectar moneda
        if (other.CompareTag("Coin"))
        {
            coins++;
            UpdateUI();
            Destroy(other.gameObject);
        }

        // Detectar inicio de carrera
        if (other.CompareTag("Start") && !isCounting)
        {
            isCounting = true;
            Debug.Log(gameObject.name + " empezó a contar");
        }
    }

    public void UpdateUI()
    {
        // Calcular puntaje total
        int totalScore = (coins * coinsMultiplier) + Mathf.RoundToInt(distance * distanceMultiplier);

        // Actualizar textos
        if (coinsText != null) coinsText.text = "Coins: " + coins;
        if (distanceText != null) distanceText.text = "Dist: " + distance.ToString("F0") + "m";
        if (totalText != null) totalText.text = "Score: " + totalScore;
    }

    // Métodos públicos para acceder a los valores
    public int GetCoins() => coins;
    public float GetDistance() => distance;
    public int GetTotalScore() => (coins * coinsMultiplier) + Mathf.RoundToInt(distance * distanceMultiplier);
}