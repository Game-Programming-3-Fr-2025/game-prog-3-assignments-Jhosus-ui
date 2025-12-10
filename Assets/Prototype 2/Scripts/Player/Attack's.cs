using UnityEngine;
using UnityEngine.InputSystem;
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

    [Header("Vibración")]
    public float vibrationLowFrequency = 0.3f;
    public float vibrationHighFrequency = 0.1f;
    public float vibrationDuration = 0.1f;

    [Header("Sonido")]
    public AudioClip attackSound; // Sonido cuando ataca
    public float attackVolume = 1f; // Volumen del sonido

    [Header("Referencias")]
    public Animator animator;

    private bool canAttack = true;
    private bool isPlayer1;
    private float vibrationTimer = 0f;
    private bool isVibrating = false;
    private Gamepad playerGamepad;

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
                playerGamepad = GetGamepadForPlayer(0);
            }
            else if (parentTag == "Player 2")
            {
                isPlayer1 = false;
                playerGamepad = GetGamepadForPlayer(1);
            }
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // Actualizar temporizador de vibración
        if (isVibrating)
        {
            vibrationTimer -= Time.deltaTime;
            if (vibrationTimer <= 0f)
            {
                StopVibration();
            }
        }

        // Verificar ataque por teclado
        KeyCode attackKey = isPlayer1 ? player1Key : player2Key;
        bool keyboardAttack = Input.GetKeyDown(attackKey);

        // Verificar ataque por mando (si hay mando conectado)
        bool controllerAttack = false;
        if (playerGamepad != null)
        {
            controllerAttack = playerGamepad.buttonWest.wasPressedThisFrame;
        }

        // Atacar si está disponible y se presiona alguna entrada
        if (canAttack && (keyboardAttack || controllerAttack))
        {
            Attack();

            // Activar vibración si se usó el mando
            if (controllerAttack)
            {
                TriggerVibration();
            }
        }
    }

    void Attack()
    {
        // Reproducir sonido de ataque
        PlayAttackSound();

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

    private void PlayAttackSound()
    {
        if (attackSound != null)
        {
            AudioSource.PlayClipAtPoint(attackSound, transform.position, attackVolume);
        }
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
            // Si tu EnemyGP tiene método para dańo
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

    // Métodos para manejar el mando
    Gamepad GetGamepadForPlayer(int playerIndex)
    {
        if (Gamepad.all.Count > playerIndex)
        {
            return Gamepad.all[playerIndex];
        }
        return null;
    }

    void TriggerVibration()
    {
        if (playerGamepad != null)
        {
            playerGamepad.SetMotorSpeeds(vibrationLowFrequency, vibrationHighFrequency);
            vibrationTimer = vibrationDuration;
            isVibrating = true;
        }
    }

    void StopVibration()
    {
        if (playerGamepad != null)
        {
            playerGamepad.SetMotorSpeeds(0f, 0f);
            isVibrating = false;
        }
    }

    void OnDisable()
    {
        // Asegurarse de detener la vibración al desactivar el script
        StopVibration();
    }

    void OnDestroy()
    {
        // Asegurarse de detener la vibración al destruir el objeto
        StopVibration();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}