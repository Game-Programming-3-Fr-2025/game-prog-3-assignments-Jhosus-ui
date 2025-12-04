using UnityEngine;
using System;

public class Attacks : MonoBehaviour
{
    [Header("Configuración")]
    public float attackRange = 1.5f;
    public int damage = 1;
    public float cooldown = 0.5f;
    public LayerMask enemyLayer;

    [Header("Teclas")]
    private KeyCode player1Key = KeyCode.E;
    private KeyCode player2Key = KeyCode.Slash;

    [Header("Referencias")]
    public Animator animator;

    private bool canAttack = true;
    private bool isPlayer1;

    void Start()
    {
        // Determinar si es Player 1 o 2 por el tag del padre
        Transform parent = transform.parent;
        if (parent != null)
        {
            string parentTag = parent.tag;

            if (parentTag == "Player 1")
            {
                isPlayer1 = true;
            }
            else if (parentTag == "Player 2")
            {
                isPlayer1 = false;
            }
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        KeyCode attackKey = isPlayer1 ? player1Key : player2Key;

        if (Input.GetKeyDown(attackKey) && canAttack)
        {
            Attack();
        }
    }

    void Attack()
    {
        // Activar animación
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Detectar enemigos
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in enemies)
        {
            ApplyDamage(enemy);
        }

        // Cooldown
        canAttack = false;
        Invoke(nameof(ResetAttack), cooldown);
    }

    void ApplyDamage(Collider2D enemy)
    {
        // 1. Dameable (para objetos/enemigos)
        Dameable dameable = enemy.GetComponent<Dameable>();
        if (dameable != null)
        {
            dameable.TakeDamage(damage, transform.position);
            return;
        }

        // 2. LifeSystem (para jugadores)
        LifeSystem lifeSystem = enemy.GetComponent<LifeSystem>();
        if (lifeSystem != null)
        {
            lifeSystem.TakeDamage(damage, transform.position);
            return;
        }

        // 3. EnemyGP (para enemigos voladores)
        EnemyGP enemyGP = enemy.GetComponent<EnemyGP>();
        if (enemyGP != null)
        {
            // Si tu EnemyGP tiene método para daño
            // enemyGP.TakeDamage(damage);
            return;
        }

        // 4. Cualquier MonoBehaviour con método TakeDamage(int)
        MonoBehaviour[] scripts = enemy.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            // Buscar SOLO métodos que reciban int
            var method = script.GetType().GetMethod("TakeDamage",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance,
                null,
                new Type[] { typeof(int) },
                null);

            if (method != null)
            {
                method.Invoke(script, new object[] { damage });
                return;
            }
        }
    }

    void ResetAttack()
    {
        canAttack = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}