using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TPI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI normalCountText;
    public TextMeshProUGUI symptomsCountText;
    public TextMeshProUGUI infectedCountText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI mistakesText;
    public TextMeshProUGUI resultText;

    [Header("Game Settings")]
    public float updateInterval = 0.5f;
    public float missionTime = 180f;
    public float infectionLimit = 0.8f;
    public int maxMistakes = 3;
    public int minPopulationToCheck = 18; 

    private float updateTimer = 0f;
    private float currentTime = 0f;
    private int currentMistakes = 0;
    private bool gameActive = true;

    void Start()
    {
        currentTime = missionTime;
        if (timerText) timerText.text = FormatTime(currentTime);
        if (mistakesText) mistakesText.text = $"Mistakes: 0/{maxMistakes}";
        if (resultText) resultText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!gameActive) return;

        currentTime -= Time.deltaTime;
        if (timerText) timerText.text = FormatTime(currentTime);

        if (currentTime <= 0f)
        {
            Victory();
            return;
        }

        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            UpdateCounters();
            updateTimer = 0f;
        }
    }

    void UpdateCounters()
    {
        People[] allPeople = FindObjectsOfType<People>();
        int normalCount = 0, symptomsCount = 0, infectedCount = 0;

        foreach (People person in allPeople)
        {
            switch (person.currentState)
            {
                case People.PersonState.Normal: normalCount++; break;
                case People.PersonState.IntermittentCough:
                case People.PersonState.Coughing: symptomsCount++; break;
                case People.PersonState.Infected: infectedCount++; break;
            }
        }

        if (normalCountText) normalCountText.text = $"Healthy: {normalCount}";
        if (symptomsCountText) symptomsCountText.text = $"Possible: {symptomsCount}";
        if (infectedCountText) infectedCountText.text = $"Infected: {infectedCount}";

        int total = normalCount + symptomsCount + infectedCount;
        if (total >= minPopulationToCheck)
        {
            float infectionPercentage = (infectedCount + symptomsCount) / (float)total;
            if (infectionPercentage >= infectionLimit)
            {
                GameOver("Most were infected");
            }
        }
    }

    public void RegisterKill(People.PersonState state)
    {
        if (!gameActive) return;

        if (state == People.PersonState.Normal)
        {
            currentMistakes++;
            if (mistakesText) mistakesText.text = $"Mistakes: {currentMistakes}/{maxMistakes}";

            if (currentMistakes >= maxMistakes)
            {
                GameOver("Innocent dead");
            }
        }
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    void GameOver(string reason)
    {
        gameActive = false;
        if (resultText)
        {
            resultText.text = $"GAME OVER";
            resultText.gameObject.SetActive(true);
        }
        Invoke("RestartGame", 3f);
    }

    void Victory()
    {
        gameActive = false;
        if (resultText)
        {
            resultText.text = "WIN";
            resultText.gameObject.SetActive(true);
        }
        Invoke("RestartGame", 3f);
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public int GetCurrentMistakes() => currentMistakes;
    public int GetMaxMistakes() => maxMistakes;
}