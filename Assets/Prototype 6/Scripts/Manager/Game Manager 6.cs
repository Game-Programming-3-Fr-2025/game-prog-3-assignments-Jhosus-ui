using UnityEngine;
using System.Collections;

public class GameManager6 : MonoBehaviour
{
    public static GameManager6 Instance;

    [Header("Settings")]
    public float respawnDelay = 2f;
    public Transform respawnPoint;
    public bool resetBoss = true;

    private GameObject player;
    private BossHealth boss;
    private Vector3 bossStartPosition;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Encontrar y guardar referencia del boss
            boss = FindObjectOfType<BossHealth>();
            if (boss != null)
                bossStartPosition = boss.transform.position;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (respawnPoint == null)
            respawnPoint = GameObject.FindGameObjectWithTag("Respawn")?.transform;
    }

    public void PlayerDied() => StartCoroutine(RespawnCoroutine());

    public void OnPlayerTeleported()
    {
        if (resetBoss) ResetBoss();
    }

    IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        RespawnPlayer();
        if (resetBoss) ResetBoss();
    }

    void RespawnPlayer()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || respawnPoint == null) return;

        Vector3 spawnPos = respawnPoint.position;
        spawnPos.z = 0;

        player.transform.position = spawnPos;
        player.SetActive(true);

        // CORREGIDO: Usar linearVelocity en vez de Reset()
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        player.GetComponent<HealthP6>()?.Respawn();
        FindObjectOfType<ManagerUp>()?.OnPlayerRespawn();
    }

    void ResetBoss()
    {
        if (boss != null)
        {
            boss.transform.position = bossStartPosition;
            boss.ResetBoss();
        }
    }
}