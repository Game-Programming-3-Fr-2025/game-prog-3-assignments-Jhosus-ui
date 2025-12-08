using UnityEngine;
using TMPro;

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

    [Header("Game Over Settings")]
    public bool gameEnded = false;
    public float delayBeforeResults = 1f;

    void Start()
    {
        // Buscar jugadores automáticamente si no están asignados
        if (player1 == null || player2 == null)
        {
            FindPlayers();
        }

        // Buscar LifeSystems automáticamente
        if (player1Life == null || player2Life == null)
        {
            FindLifeSystems();
        }
    }

    void Update()
    {
        // Opcional: Actualizar UI global cada frame
        UpdateGlobalUI();

        // Verificar si ambos jugadores han muerto
        if (!gameEnded && CheckBothPlayersDead())
        {
            gameEnded = true;
            Invoke("ShowResults", delayBeforeResults);
        }
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

    void FindLifeSystems()
    {
        // Buscar todos los LifeSystem en la escena
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
        if (player1Life == null || player2Life == null)
        {
            // Si no hay referencias a los LifeSystems, no podemos verificar
            return false;
        }

        // Verificar si ambos tienen 0 o menos de vida
        // Puedes acceder al currentHealth si es público, o crear un método público en LifeSystem
        return player1Life.currentHealth <= 0 && player2Life.currentHealth <= 0;
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
        gameEnded = false;
        Debug.Log("Juego reseteado");
    }
}