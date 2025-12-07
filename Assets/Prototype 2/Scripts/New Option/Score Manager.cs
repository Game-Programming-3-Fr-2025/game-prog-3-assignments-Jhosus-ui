using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Player Scores")]
    public PlayerScore player1;
    public PlayerScore player2;

    [Header("UI Elements")]
    public TMP_Text player1FinalText;
    public TMP_Text player2FinalText;
    public TMP_Text winnerText;

    void Start()
    {
        // Buscar jugadores automáticamente si no están asignados
        if (player1 == null || player2 == null)
        {
            FindPlayers();
        }
    }

    void Update()
    {
        // Opcional: Actualizar UI global cada frame
        UpdateGlobalUI();
    }

    void FindPlayers()
    {
        // Buscar todos los PlayerScore en la escena
        PlayerScore[] allScores = FindObjectsOfType<PlayerScore>();

        foreach (PlayerScore score in allScores)
        {
            if (score.isPlayer1 && player1 == null)
                player1 = score;
            else if (!score.isPlayer1 && player2 == null)
                player2 = score;
        }
    }

    void UpdateGlobalUI()
    {
        if (player1FinalText != null && player1 != null)
            player1FinalText.text = "P1: " + player1.GetTotalScore();

        if (player2FinalText != null && player2 != null)
            player2FinalText.text = "P2: " + player2.GetTotalScore();
    }

    // Llamar este método cuando termine el nivel
    public void ShowResults()
    {
        if (player1 == null || player2 == null) return;

        int score1 = player1.GetTotalScore();
        int score2 = player2.GetTotalScore();

        if (winnerText != null)
        {
            if (score1 > score2)
                winnerText.text = "Jugador 1 Gana!";
            else if (score2 > score1)
                winnerText.text = "Jugador 2 Gana!";
            else
                winnerText.text = "Empate!";
        }

        Debug.Log("Resultados: P1=" + score1 + ", P2=" + score2);
    }

    // Resetear puntajes (para nueva partida)
    public void ResetGame()
    {
        // Necesitarías recargar la escena o crear método Reset() en PlayerScore
        Debug.Log("Juego reseteado");
    }
}