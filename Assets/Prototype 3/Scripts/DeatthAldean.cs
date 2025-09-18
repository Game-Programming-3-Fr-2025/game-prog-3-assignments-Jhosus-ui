using UnityEngine;

public class DeatthAldean : MonoBehaviour
{
    [Header("Health Settings")]
    public int minHealth = 1;
    public int maxHealth = 2;

    private int health;
    private bool isDead;

    void Start()
    {
        health = Random.Range(minHealth, maxHealth + 1);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        if (health <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;      
        Destroy(gameObject, 0.3f);
    }
}