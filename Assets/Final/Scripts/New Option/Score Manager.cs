using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

    [Header("Contador")]
    public TMP_Text countdownText;
    public float countdownTime = 5f;

    [Header("Escena")]
    public string nextSceneName = "Menu";

    [Header("Result Screen")]
    public GameObject resultsScreen;
    public Image backgroundPanel;

    [Header("Game State")]
    public bool gameEnded = false;

    private bool player1Dead = false;
    private bool player2Dead = false;
    private float currentCountdownTime;
    private bool countdownActive = false;

    void Start()
    {
        if (player1 == null || player2 == null) FindPlayers();
        if (player1Life == null || player2Life == null) FindLifeSystems();
        HideResultsScreen();
    }

    void Update()
    {
        if (!gameEnded)
        {
            UpdateGlobalUI();
            CheckIndividualDeaths();

            if (CheckBothPlayersDead())
            {
                gameEnded = true;
                StartCoroutine(GameOverSequence());
            }
        }

        if (countdownActive)
        {
            currentCountdownTime -= Time.unscaledDeltaTime;
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(currentCountdownTime).ToString(); 

            if (currentCountdownTime <= 0)
            {
                countdownActive = false;
                CargarEscenaFinal();
            }
        } 
    }

    void FindPlayers()
    {
        PlayerScore[] allScores = FindObjectsOfType<PlayerScore>();
        foreach (PlayerScore score in allScores)
        {
            if (score.isPlayer1 && player1 == null) player1 = score;
            else if (!score.isPlayer1 && player2 == null) player2 = score;
        }
    }

    void FindLifeSystems()
    {
        LifeSystem[] allLives = FindObjectsOfType<LifeSystem>(); //refereciemos a todos los sistemas de vida en la escena
        foreach (LifeSystem life in allLives)
        {
            if (life.playerType == PlayerType.Player1 && player1Life == null) player1Life = life;
            else if (life.playerType == PlayerType.Player2 && player2Life == null) player2Life = life;
        }
    }

    void CheckIndividualDeaths()
    {
        if (player1Life != null && player1Life.currentHealth <= 0 && !player1Dead) //Verifiquemos si el jugador 1 ha muerto
        {
            player1Dead = true;
            StopPlayerScoreCounting(player1);
            HidePlayer(player1Life.gameObject);
        }

        if (player2Life != null && player2Life.currentHealth <= 0 && !player2Dead) //Verifiquemos si el jugador 2 ha muerto
        {
            player2Dead = true;
            StopPlayerScoreCounting(player2);
            HidePlayer(player2Life.gameObject);
        }
    }

    void StopPlayerScoreCounting(PlayerScore playerScore)
    {
        if (playerScore != null)
        {
            MonoBehaviour mb = playerScore as MonoBehaviour; //Transformar PlayerScore a MonoBehaviour para deshabilitarlo
            if (mb != null) mb.enabled = false;
        }
    }

    void HidePlayer(GameObject player)
    {
        if (player == null) return;

        TrailRenderer trail = player.GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.enabled = false;
            trail.Clear();
        }

        ParticleSystem[] particles = player.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particles)
        {
            ps.Stop();
            ps.Clear();
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        Collider2D[] colliders = player.GetComponents<Collider2D>();
        foreach (Collider2D col in colliders) col.enabled = false;

        MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>(); //Desactivemos solo lifesystem y player score
        foreach (MonoBehaviour script in scripts)
        {
            if (script is LifeSystem || script is PlayerScore) continue; 
            script.enabled = false;
        }

        SpriteRenderer sprite = player.GetComponent<SpriteRenderer>(); //Ocultemos el sprite del jugador
        if (sprite != null) sprite.enabled = false;
    }

    bool CheckBothPlayersDead()
    {
        if (player1Life == null || player2Life == null) return false;
        return player1Life.currentHealth <= 0 && player2Life.currentHealth <= 0;
    }

    public IEnumerator GameOverSequence() //Pasemos a la pantalla de resultados
    {
        Time.timeScale = 0f;
        ShowResultsScreen();
        ShowResults();
        yield return new WaitForSecondsRealtime(1f);
        StartCountdown();
    }

    void StartCountdown() //No tanto la escena final sino abrir paso a repetir nivel
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            currentCountdownTime = countdownTime;
            countdownActive = true;
        }
        else
        {
            Invoke(nameof(CargarEscenaFinal), countdownTime);
        }
    }

    void CargarEscenaFinal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
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
        if (resultsScreen != null) resultsScreen.SetActive(false);
        if (backgroundPanel != null) backgroundPanel.gameObject.SetActive(false);
        if (winnerText != null) winnerText.gameObject.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    void ShowResultsScreen()
    {
        if (backgroundPanel != null) backgroundPanel.gameObject.SetActive(true);
        if (resultsScreen != null) resultsScreen.SetActive(true);
        if (winnerText != null) winnerText.gameObject.SetActive(true);
    }

    public void ShowResults()
    {
        if (player1 == null || player2 == null) return;

        int score1 = player1.GetTotalScore();
        int score2 = player2.GetTotalScore();

        if (winnerText != null) //Mostrar el ganador
        {
            if (score1 > score2) winnerText.text = "Player 1 Win!";
            else if (score2 > score1) winnerText.text = "Player 2 Win!";
            else winnerText.text = "Tied!";
        }
    }
}