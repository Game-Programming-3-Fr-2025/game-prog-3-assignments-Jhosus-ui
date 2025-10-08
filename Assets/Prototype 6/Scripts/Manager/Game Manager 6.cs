using UnityEngine;
using System.Collections;

public class GameManager6 : MonoBehaviour
{
    public static GameManager6 Instance;

    [Header("Respawn Settings")]
    public float respawnDelay = 2f;
    public Transform respawnPoint;

    private GameObject player;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        FindPlayer();
        if (respawnPoint == null)
        {
            GameObject spawnObj = GameObject.FindGameObjectWithTag("Respawn");
            if (spawnObj != null) respawnPoint = spawnObj.transform;
        }
    }

    void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void PlayerDied()
    {
        StartCoroutine(RespawnSequence());
    }

    IEnumerator RespawnSequence()
    {
        yield return new WaitForSeconds(respawnDelay);
        ExecuteRespawn();
    }

    void ExecuteRespawn()
    {
        if (player == null) FindPlayer();

        if (player != null && respawnPoint != null)
        {
            // ✅ FORZAR POSICIÓN Z = 0
            Vector3 spawnPosition = respawnPoint.position;
            spawnPosition.z = 0; // ← ESTA ES LA LÍNEA CLAVE

            player.transform.position = spawnPosition;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            player.SetActive(true);

            HealthP6 playerHealth = player.GetComponent<HealthP6>();
            if (playerHealth != null)
            {
                playerHealth.Respawn();
            }

            ManagerUp manager = FindObjectOfType<ManagerUp>();
            manager?.OnPlayerRespawn();
        }
    }
}