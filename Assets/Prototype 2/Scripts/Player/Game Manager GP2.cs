using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManagerGP2: MonoBehaviour
{
    public static GameManagerGP2 Instance;

    [Header("Build Settings Index")]
    public int menuSceneIndex = 0;
    public int gameSceneIndex = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cuando se carga una nueva escena, RESTAURAR TODO
        ResetSceneForTwoPlayers();
    }

    void ResetSceneForTwoPlayers()
    {
        // 1. Asegurar tiempo normal
        Time.timeScale = 1f;

        // 2. Resetear cámaras (IMPORTANTE para split-screen)
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in cameras)
        {
            if (cam.orthographic)
            {
                // Para cámaras ortográficas (2D)
                cam.orthographicSize = 5f;
                cam.transform.position = new Vector3(0, 0, -10);
                cam.rect = new Rect(0, 0, 1, 1); // Resetear viewport
            }
        }

        // 3. Buscar y destruir jugadores VIEJOS (si los hay)
        GameObject[] oldPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in oldPlayers)
        {
            // Solo destruir si no es parte de la nueva escena
            if (player.scene.buildIndex != SceneManager.GetActiveScene().buildIndex)
            {
                Destroy(player);
            }
        }

        // 4. Buscar y resetear el SplitScreenManager
        GameObject splitScreenObj = GameObject.Find("SplitScreenManager");
        if (splitScreenObj != null)
        {
            Destroy(splitScreenObj);
        }
    }

    // Método para cargar desde el MENÚ
    public void LoadGameFromMenu()
    {
        StartCoroutine(LoadGameSceneCoroutine());
    }

    // Método para volver al MENÚ (desde ScoreManager)
    public void LoadMenuFromGame()
    {
        StartCoroutine(LoadMenuSceneCoroutine());
    }

    IEnumerator LoadGameSceneCoroutine()
    {
        // 1. Limpiar menú
        CleanupCurrentScene();
        yield return new WaitForSecondsRealtime(0.1f);

        // 2. Cargar juego
        SceneManager.LoadScene(gameSceneIndex, LoadSceneMode.Single);
    }

    IEnumerator LoadMenuSceneCoroutine()
    {
        // 1. Matar a los jugadores actuales (para que no persistan)
        KillAllPlayers();
        yield return new WaitForSecondsRealtime(0.1f);

        // 2. Limpiar escena de juego
        CleanupCurrentScene();
        yield return new WaitForSecondsRealtime(0.1f);

        // 3. Cargar menú
        SceneManager.LoadScene(menuSceneIndex, LoadSceneMode.Single);
    }

    void KillAllPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            // Desactivar scripts primero
            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                script.enabled = false;
            }

            // Luego destruir
            Destroy(player);
        }
    }

    void CleanupCurrentScene()
    {
        // Destruir objetos que NO deben persistir
        string[] tagsToDestroy = { "Player", "Enemy", "Item", "Collectible", "Bullet" };

        foreach (string tag in tagsToDestroy)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objects)
            {
                Destroy(obj);
            }
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}