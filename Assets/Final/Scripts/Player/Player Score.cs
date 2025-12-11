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
    public float distanceIncrementRate = 1f;
    public bool useTimeBasedDistance = true;

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
        if (!isCounting || !enabled) return;

        if (useTimeBasedDistance)
        {
            countingTime += Time.deltaTime;
            distance = countingTime * distanceIncrementRate; //Distancia basada en tiempo
        }
        else
        {
            float xMovement = Mathf.Max(0, transform.position.x - lastPosition.x); //Solo contar movimiento hacia la derecha (Para algunos Casos)
            distance += xMovement;
            lastPosition = transform.position;
        }

        UpdateUI();
    }

    public void StopCounting()
    {
        isCounting = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin"))
        {
            coins++;
            UpdateUI();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Start") && !isCounting)
        {
            IniciarConteo();
        }
    }

    void IniciarConteo()
    {
        isCounting = true;
        countingTime = 0f;
        lastPosition = transform.position;
    }

    public void UpdateUI()
    {
        int totalScore = GetTotalScore(); //Calcular Puntuación Total

        if (coinsText != null) coinsText.text = coins.ToString();
        if (distanceText != null) distanceText.text = distance.ToString("F0"); 
        if (totalText != null) totalText.text = totalScore.ToString();
    } //Actualizar UI

    public int GetCoins() => coins;
    public float GetDistance() => distance;
    public int GetTotalScore() => (coins * coinsMultiplier) + Mathf.RoundToInt(distance * distanceMultiplier); //calcular mediante fórmula
}