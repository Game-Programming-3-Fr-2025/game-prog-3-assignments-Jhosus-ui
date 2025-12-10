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

    void Start()
    {
        Transform parent = transform.parent;
        if (parent != null)
        {
            isPlayer1 = parent.CompareTag("Player 1");
            playerGamepad = GetGamepadForPlayer(isPlayer1 ? 0 : 1);
        }

        animator = animator ?? GetComponent<Animator>();
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
        bool controllerAttack = playerGamepad?.buttonWest.wasPressedThisFrame ?? false;

        if (controllerAttack)
            TriggerVibration();

        return keyboardAttack || controllerAttack;
    }

    void Attack()
    {
        PlayAttackSound();

        if (animator != null)
            animator.SetTrigger("Attack");

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in enemies)
            ApplyDamage(enemy);

        canAttack = false;
        Invoke(nameof(ResetAttack), cooldown);
    }

    private void PlayAttackSound()
    {
        if (attackSound != null)
            AudioSource.PlayClipAtPoint(attackSound, transform.position, attackVolume);
    }

    void ApplyDamage(Collider2D enemy)
    {
        Dameable dameable = enemy.GetComponent<Dameable>();
        if (dameable != null)
        {
            dameable.TakeDamage(damage, transform.position);
            return;
        }

        LifeSystem lifeSystem = enemy.GetComponent<LifeSystem>();
        if (lifeSystem != null)
        {
            lifeSystem.TakeDamage(damage, transform.position);
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
                null, new Type[] { typeof(int) }, null);

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

    void OnDisable() => StopVibration();
    void OnDestroy() => StopVibration();

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}