using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class Health7 : MonoBehaviour
{
    [Header("Configuraci�n de Vida")]
    public int maxHearts = 3;
    public int currentHearts;
    public float damageCooldown = 1f;
    public float damageRange = 1.5f;

    [Header("UI de Corazones")]
    public Image[] heartImages;

    [Header("Regeneraci�n")]
    public float regenerationRate = 0f;
    private float regenerationTimer;

    private bool canTakeDamage = true;
    private UPManager7 upManager;
    private bool isDead = false;

    void Start()
    {
        upManager = FindObjectOfType<UPManager7>();
        currentHearts = maxHearts;
        UpdateHeartUI();
    }

    void Update()
    {
        if (isDead) return;

        HandleRegeneration();
        CheckForEnemyDamage();
    }

    void CheckForEnemyDamage()
    {
        if (!canTakeDamage || isDead) return;

        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, damageRange);

        foreach (Collider2D collider in enemiesInRange)
        {
            if (collider.CompareTag("Enemy"))
            {
                TakeDamage(1);
                break;
            }
        }
    }

    public void TakeDamage(int damageAmount = 1)
    {
        if (isDead) return;

        currentHearts = Mathf.Max(0, currentHearts - damageAmount);
        UpdateHeartUI();

        StartCoroutine(DamageCooldown());

        if (currentHearts <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount = 1)
    {
        if (isDead) return;

        currentHearts = Mathf.Min(maxHearts, currentHearts + healAmount);
        UpdateHeartUI();
    }

    void HandleRegeneration()
    {
        if (regenerationRate <= 0 || isDead) return;

        regenerationTimer += Time.deltaTime;
        if (regenerationTimer >= regenerationRate && currentHearts < maxHearts)
        {
            Heal(1);
            regenerationTimer = 0f;
        }
    }

    void UpdateHeartUI()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            heartImages[i].gameObject.SetActive(i < currentHearts);
        }
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
        Debug.Log("Player muri�! Reiniciando escena...");

        // Reiniciar la escena despu�s de un breve delay
        StartCoroutine(ReiniciarEscena());
    }

    IEnumerator ReiniciarEscena()
    {
        // Peque�o delay para que se vea la muerte
        yield return new WaitForSeconds(0.5f);

        // Reiniciar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // M�todos para que UPManager actualice las stats
    public void ActualizarVidaMaxima(int nuevaVidaMaxima)
    {
        if (isDead) return;

        maxHearts = nuevaVidaMaxima;
        currentHearts = Mathf.Min(currentHearts, maxHearts);
        UpdateHeartUI();
    }

    public void ActualizarRegeneracion(float nuevaRegeneracion)
    {
        if (isDead) return;

        regenerationRate = nuevaRegeneracion;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, damageRange);
    }
}