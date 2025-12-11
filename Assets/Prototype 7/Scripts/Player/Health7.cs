using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class Health7 : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHearts = 3;
    [SerializeField] private float damageCooldown = 1f;
    [SerializeField] private float damageRange = 1.5f;

    [Header("UI")]
    [SerializeField] private Image[] heartImages;

    [Header("Regeneration")]
    [SerializeField] private float regenerationRate = 0f;

    private int currentHearts;
    private float regenerationTimer;
    private bool canTakeDamage = true;
    private bool isDead;

    void Start()
    {
        currentHearts = maxHearts;
        UpdateHeartUI();
    }

    void Update()
    {
        if (isDead) return;

        HandleRegeneration();
        CheckEnemyDamage();
    }

    void CheckEnemyDamage()
    {
        if (!canTakeDamage) return;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, damageRange);

        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                TakeDamage(1);
                break;
            }
        }
    }

    public void TakeDamage(int amount = 1)
    {
        if (isDead) return;

        currentHearts = Mathf.Max(0, currentHearts - amount);
        UpdateHeartUI();
        StartCoroutine(DamageCooldown());

        if (currentHearts <= 0)
            Die();
    }

    public void Heal(int amount = 1)
    {
        if (isDead) return;

        currentHearts = Mathf.Min(maxHearts, currentHearts + amount);
        UpdateHeartUI();
    }

    void HandleRegeneration()
    {
        if (regenerationRate <= 0 || currentHearts >= maxHearts) return;

        regenerationTimer += Time.deltaTime;

        if (regenerationTimer >= regenerationRate)
        {
            Heal(1);
            regenerationTimer = 0f;
        }
    }

    void UpdateHeartUI()
    {
        for (int i = 0; i < heartImages.Length; i++)
            heartImages[i].gameObject.SetActive(i < currentHearts);
    }

    IEnumerator DamageCooldown()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canTakeDamage = true;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(RestartScene());
    }

    IEnumerator RestartScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ActualizarVidaMaxima(int newMaxHearts)
    {
        if (isDead) return;

        maxHearts = newMaxHearts;
        currentHearts = Mathf.Min(currentHearts, maxHearts);
        UpdateHeartUI();
    }

    public void ActualizarRegeneracion(float newRate)
    {
        if (isDead) return;
        regenerationRate = newRate;
    }
}