using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    [Header("Player Scores")]
    public PlayerScore player1;
    public PlayerScore player2;

    [Header("Player Lives")]
    public LifeSystem player1Life;
    public LifeSystem player2Life;

    [Header("UI Elements")]
    public TMP_Text player1FinalText;
    public TMP_Text player2FinalText;
    public TMP_Text winnerText;

    [Header("Result Screen")]
    public GameObject resultsScreen; // Pantalla completa de resultados
    public Image backgroundPanel;    // Fondo (opcional si está dentro de resultsScreen)
    public float delayBeforeResults = 1f;
    public bool pauseGameOnEnd = true;

    [Header("Game State")]
    public bool gameEnded = false;

    void Start()
    {
        // Buscar jugadores automáticamente si no están asignados
        if (player1 == null || player2 == null)
        {
            FindPlayers();
        }

        if (player1Life == null || player2Life == null)
        {
            FindLifeSystems();
        }

        // Ocultar pantalla de resultados al inicio
        HideResultsScreen();
    }

    void Update()
    {
        // Actualizar UI global
        UpdateGlobalUI();

        // Verificar si ambos jugadores han muerto
        if (!gameEnded && CheckBothPlayersDead())
        {
            gameEnded = true;
            StartCoroutine(GameOverSequence());
        }
    }

    void FindPlayers()
    {
        PlayerScore[] allScores = FindObjectsOfType<PlayerScore>();
        foreach (PlayerScore score in allScores)
        {
            if (score.isPlayer1 && player1 == null)
                player1 = score;
            else if (!score.isPlayer1 && player2 == null)
                player2 = score;
        }
    }

    void FindLifeSystems()
    {
        LifeSystem[] allLives = FindObjectsOfType<LifeSystem>();
        foreach (LifeSystem life in allLives)
        {
            if (life.playerType == PlayerType.Player1 && player1Life == null)
                player1Life = life;
            else if (life.playerType == PlayerType.Player2 && player2Life == null)
                player2Life = life;
        }
    }

    bool CheckBothPlayersDead()
    {
        if (player1Life == null || player2Life == null) return false;
        return player1Life.currentHealth <= 0 && player2Life.currentHealth <= 0;
    }

    IEnumerator GameOverSequence()
    {
        // Esperar antes de mostrar resultados
        yield return new WaitForSeconds(delayBeforeResults);

        // Pausar el juego
        if (pauseGameOnEnd)
        {
            Time.timeScale = 0f;
        }

        // Mostrar pantalla de resultados
        ShowResultsScreen();

        // Calcular y mostrar quién ganó
        ShowResults();
    }

    void UpdateGlobalUI()
    {
        if (player1FinalText != null && player1 != null)
            player1FinalText.text = player1.GetTotalScore().ToString();

        if (player2FinalText != null && player2 != null)
            player2FinalText.text = player2.GetTotalScore().ToString();
    }

    void HideResultsScreen()
    {
        if (resultsScreen != null)
            resultsScreen.SetActive(false);

        if (backgroundPanel != null)
            backgroundPanel.gameObject.SetActive(false);

        if (winnerText != null)
            winnerText.gameObject.SetActive(false);
    }

    void ShowResultsScreen()
    {
        // Primero mostrar el fondo
        if (backgroundPanel != null)
            backgroundPanel.gameObject.SetActive(true);

        // Luego la pantalla completa
        if (resultsScreen != null)
            resultsScreen.SetActive(true);

        // Finalmente el texto del ganador
        if (winnerText != null)
            winnerText.gameObject.SetActive(true);
    }

    public void ShowResults()
    {
        if (player1 == null || player2 == null) return;

        int score1 = player1.GetTotalScore();
        int score2 = player2.GetTotalScore();

        if (winnerText != null)
        {
            if (score1 > score2)
                winnerText.text = "Player 1 Win!";
            else if (score2 > score1)
                winnerText.text = "Player 2 Win!";
            else
                winnerText.text = "Tied!";
        }

        Debug.Log("Resultados: P1=" + score1 + ", P2=" + score2);
    }

    public void ResetGame()
    {
        gameEnded = false;
        Time.timeScale = 1f;
        HideResultsScreen();
        Debug.Log("Juego reseteado");
    }
}