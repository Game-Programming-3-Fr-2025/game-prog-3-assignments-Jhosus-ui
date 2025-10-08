using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ManagerUp : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text expText, healthText, damageText, expBonusText;
    public Button dashButton, doubleJumpButton, wallClimbButton;

    [Header("Costs")]
    public int dashCost = 100, doubleJumpCost = 150, wallClimbCost = 120;
    public int healthCost = 80, damageCost = 100, expBonusCost = 60;

    private int exp = 1000;
    private int healthLevel = 0, damageLevel = 0, expLevel = 0;
    private bool dashBought, doubleJumpBought, wallClimbBought;

    private const int MAX_HEALTH = 4, MAX_DAMAGE = 4, MAX_EXP = 11;
    private int[] damageValues = { 5, 9, 13, 17, 21 };

    private Evasion playerEvasion;
    private HealthP6 playerHealth;
    private Combate playerCombat;

    void Awake() => DontDestroyOnLoad(gameObject);

    void Start()
    {
        FindPlayer();
        SetupButtons();
        UpdateUI();
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        playerEvasion = player.GetComponent<Evasion>();
        playerHealth = player.GetComponent<HealthP6>();
        playerCombat = player.GetComponent<Combate>();

        ApplyUpgrades();
    }

    void SetupButtons()
    {
        dashButton?.onClick.AddListener(() => BuyAbility(ref dashBought, dashCost, 0));
        doubleJumpButton?.onClick.AddListener(() => BuyAbility(ref doubleJumpBought, doubleJumpCost, 1));
        wallClimbButton?.onClick.AddListener(() => BuyAbility(ref wallClimbBought, wallClimbCost, 2));
    }

    void BuyAbility(ref bool abilityBought, int cost, int abilityIndex)
    {
        if (abilityBought || exp < cost) return;

        exp -= cost;
        abilityBought = true;
        ApplyAbility(abilityIndex);
        UpdateUI();
    }

    void ApplyAbility(int index)
    {
        if (playerEvasion == null) return;

        switch (index)
        {
            case 0: playerEvasion.UnlockDash(); break;
            case 1: playerEvasion.UnlockDoubleJump(); break;
            case 2: playerEvasion.UnlockWallClimb(); break;
        }
    }

    void ApplyUpgrades()
    {
        if (playerEvasion != null)
        {
            if (dashBought) playerEvasion.UnlockDash();
            if (doubleJumpBought) playerEvasion.UnlockDoubleJump();
            if (wallClimbBought) playerEvasion.UnlockWallClimb();
        }

        if (playerHealth != null && healthLevel > 0)
            UpdatePlayerHealth();

        if (playerCombat != null && damageLevel < damageValues.Length)
            playerCombat.attackDamage = damageValues[damageLevel];
    }

    public void BuyHealth()
    {
        int cost = healthCost * (healthLevel + 1);
        if (exp < cost || healthLevel >= MAX_HEALTH) return;

        exp -= cost;
        healthLevel++;
        UpdatePlayerHealth();
        UpdateUI();
    }

    void UpdatePlayerHealth()
    {
        if (playerHealth == null) return;

        playerHealth.maxHealth = 4 + healthLevel;
        playerHealth.Heal(playerHealth.maxHealth - playerHealth.GetCurrentHealth());
        playerHealth.UpdateHealthUI();
    }

    public void BuyDamage()
    {
        int cost = damageCost * (damageLevel + 1);
        if (exp < cost || damageLevel >= MAX_DAMAGE) return;

        exp -= cost;
        damageLevel++;

        if (playerCombat != null && damageLevel < damageValues.Length)
            playerCombat.attackDamage = damageValues[damageLevel];

        UpdateUI();
    }

    public void BuyExpBonus()
    {
        int cost = expBonusCost * (expLevel + 1);
        if (exp < cost || expLevel >= MAX_EXP) return;

        exp -= cost;
        expLevel++;
        UpdateUI();
    }

    public void RegisterHit()
    {
        if (++hitCount >= 3)
        {
            hitCount = 0;
            exp += Mathf.RoundToInt(10 * (1.12f + expLevel * 0.06f));
            UpdateUI();
        }
    }
    private int hitCount = 0;

    public void UpdateUI()
    {
        expText.text = $"EXP: {exp}";
        dashButton.gameObject.SetActive(!dashBought);
        doubleJumpButton.gameObject.SetActive(!doubleJumpBought);
        wallClimbButton.gameObject.SetActive(!wallClimbBought);

        healthText.text = GetUpgradeText("HP +1", healthLevel, MAX_HEALTH, healthCost);
        damageText.text = GetUpgradeText($"DMG: {damageValues[damageLevel]}", damageLevel, MAX_DAMAGE, damageCost);
        expBonusText.text = GetUpgradeText($"+{12 + expLevel * 6}%", expLevel, MAX_EXP, expBonusCost);
    }

    string GetUpgradeText(string description, int level, int maxLevel, int cost)
    {
        return level >= maxLevel ? "MAX" : $"{description}\n{level}/{maxLevel}\n{cost * (level + 1)} EXP";
    }

    public void OnPlayerRespawn() => FindPlayer();
    public int GetHealthLevel() => healthLevel;
}