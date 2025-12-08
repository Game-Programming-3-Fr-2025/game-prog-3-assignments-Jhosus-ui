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

    [Header("Distance Settings")]
    public float distanceIncrementRate = 1f; // Cuánto aumenta por segundo
    public bool useTimeBasedDistance = true; // Si true, usa tiempo; si false, usa posición X

    private int coins = 0;
    private float distance = 0f;
    private Vector3 lastPosition;
    private bool isCounting = false;
    private float countingTime = 0f;

    void Start()
    {
        lastPosition = transform.position;
        UpdateUI();
    }

    void Update()
    {
        if (isCounting)
        {
            if (useTimeBasedDistance)
            {
                // Método basado en tiempo (simplemente aumenta con el tiempo)
                countingTime += Time.deltaTime;
                distance = countingTime * distanceIncrementRate;
            }
            else
            {
                // Método basado en posición X (avance horizontal)
                float xMovement = Mathf.Max(0, transform.position.x - lastPosition.x);
                distance += xMovement;
                lastPosition = transform.position;
            }

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
            countingTime = 0f;
            lastPosition = transform.position;
            Debug.Log(gameObject.name + " empezó a contar");
        }

        // Detectar final de carrera (opcional)
        if (other.CompareTag("Finish") && isCounting)
        {
            isCounting = false;
            Debug.Log(gameObject.name + " terminó con distancia: " + distance.ToString("F0"));
        }
    }

    public void UpdateUI()
    {
        // Calcular puntaje total
        int totalScore = (coins * coinsMultiplier) + Mathf.RoundToInt(distance * distanceMultiplier);

        // Actualizar textos (solo números)
        if (coinsText != null) coinsText.text = coins.ToString();
        if (distanceText != null) distanceText.text = distance.ToString("F0");
        if (totalText != null) totalText.text = totalScore.ToString();
    }

    // Métodos públicos para acceder a los valores
    public int GetCoins() => coins;
    public float GetDistance() => distance;
    public int GetTotalScore() => (coins * coinsMultiplier) + Mathf.RoundToInt(distance * distanceMultiplier);
}