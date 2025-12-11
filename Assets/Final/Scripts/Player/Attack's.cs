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
    public AudioClip attackSound;
    public float attackVolume = 1f;

    [Header("Referencias")]
    public Animator animator;

    private bool canAttack = true;
    private bool isPlayer1;
    private float vibrationTimer = 0f;
    private bool isVibrating = false;
    private Gamepad playerGamepad;

    public AudioSource audioSource;
    void Start()
    {
        Transform parent = transform.parent;
        if (parent != null)
        {
            isPlayer1 = parent.CompareTag("Player 1");
            playerGamepad = GetGamepadForPlayer(isPlayer1 ? 0 : 1);
        }

        animator = animator ?? GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        UpdateVibration();

        if (canAttack && DetectAttackInput())
            Attack();
    }

    void UpdateVibration()
    {
        if (isVibrating) 
        {
            vibrationTimer -= Time.deltaTime;
            if (vibrationTimer <= 0f)
                StopVibration();
        }
    }

    bool DetectAttackInput()
    {
        KeyCode attackKey = isPlayer1 ? player1Key : player2Key;
        bool keyboardAttack = Input.GetKeyDown(attackKey);
        bool controllerAttack = playerGamepad?.buttonWest.wasPressedThisFrame ?? false; // X button o cuadrado da uigual

        if (controllerAttack)
            TriggerVibration();

        return keyboardAttack || controllerAttack; //devolver un solo true si se presionó alguna de las dos
    }

    void Attack()
    {
        PlayAttackSound();

        if (animator != null)
            animator.SetTrigger("Attack");

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer); // Detectar enemigos en rango medianete Giz

        foreach (Collider2D enemy in enemies)
            ApplyDamage(enemy);

        canAttack = false;
        Invoke(nameof(ResetAttack), cooldown);
    }

    private void PlayAttackSound()
    {
        if (attackSound != null)
            audioSource.PlayOneShot(attackSound, attackVolume); // Reproducir sonido de ataque aunque esperemos que logre 
    }

    void ApplyDamage(Collider2D enemy)
    {
        Dameable dameable = enemy.GetComponent<Dameable>();
        if (dameable != null)
        {
            dameable.TakeDamage(damage, transform.position);
            return;
        }

        LifeSystem lifeSystem = enemy.GetComponent<LifeSystem>(); //referenciamos al sistema de vida
        if (lifeSystem != null)
        {
            lifeSystem.TakeDamage(damage, transform.position); // Aplicar daño al sistema de vida
            return;
        }

        EnemyGP enemyGP = enemy.GetComponent<EnemyGP>(); 
        if (enemyGP != null)
        {
            return;
        }

        TryInvokeTakeDamage(enemy);
    }

    void TryInvokeTakeDamage(Collider2D enemy)
    {
        MonoBehaviour[] scripts = enemy.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            var method = script.GetType().GetMethod("TakeDamage",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                null, new Type[] { typeof(int) }, null); //ACUERDATE DE ESTO PARA FUTURO

            if (method != null)
            {
                method.Invoke(script, new object[] { damage });
                return;
            }
        }
    }

    void ResetAttack() => canAttack = true;

    Gamepad GetGamepadForPlayer(int playerIndex)
    {
        return Gamepad.all.Count > playerIndex ? Gamepad.all[playerIndex] : null;
    }

    void TriggerVibration()
    {
        if (playerGamepad != null) // Asegurémonos de que el gamepad exista
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
            playerGamepad.SetMotorSpeeds(0f, 0f); //Matemos la vibración
            isVibrating = false;
        }
    }

    void OnDisable() => StopVibration(); 
    void OnDestroy() => StopVibration();

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}