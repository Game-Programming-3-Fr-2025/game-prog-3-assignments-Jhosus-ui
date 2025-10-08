using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ManagerUp : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text expText;
    public TMP_Text healthText;
    public TMP_Text damageText;
    public TMP_Text expBonusText;

    [Header("Ability Buttons")]
    public Button dashButton;
    public Button doubleJumpButton;
    public Button wallClimbButton;

    [Header("Costs")]
    public int dashCost = 100;
    public int doubleJumpCost = 150;
    public int wallClimbCost = 120;
    public int healthCost = 80;
    public int damageCost = 100;
    public int expBonusCost = 60;

    // Persistent data
    private int exp = 1000;
    private int healthLevel = 0;
    private int damageLevel = 0;
    private int expLevel = 0;
    private bool[] abilities = new bool[3]; // [dash, doubleJump, wallClimb]

    // Config
    private const int MAX_HEALTH = 9;
    private const int MAX_DAMAGE = 4;
    private const int MAX_EXP = 11;
    private int[] damageValues = { 5, 9, 13, 17, 21 };

    private int hitCount = 0;
    private Evasion playerEvasion;
    private HealthP6 playerHealth;
    private Combate playerCombat;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

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

    void ApplyUpgrades()
    {
        if (playerEvasion != null)
        {
            if (abilities[0]) playerEvasion.UnlockDash();
            if (abilities[1]) playerEvasion.UnlockDoubleJump();
            if (abilities[2]) playerEvasion.UnlockWallClimb();
        }

        if (playerHealth != null && healthLevel > 0)
        {
            playerHealth.maxHealth = 3 + healthLevel;
            playerHealth.Heal(playerHealth.maxHealth);
        }

        if (playerCombat != null && damageLevel < damageValues.Length)
        {
            playerCombat.attackDamage = damageValues[damageLevel];
        }
    }

    void SetupButtons()
    {
        if (dashButton != null)
            dashButton.onClick.AddListener(() => BuyAbility(0, dashCost));
        if (doubleJumpButton != null)
            doubleJumpButton.onClick.AddListener(() => BuyAbility(1, doubleJumpCost));
        if (wallClimbButton != null)
            wallClimbButton.onClick.AddListener(() => BuyAbility(2, wallClimbCost));
    }

    void BuyAbility(int index, int cost)
    {
        if (abilities[index] || exp < cost) return;

        exp -= cost;
        abilities[index] = true;

        if (playerEvasion != null)
        {
            if (index == 0) playerEvasion.UnlockDash();
            else if (index == 1) playerEvasion.UnlockDoubleJump();
            else if (index == 2) playerEvasion.UnlockWallClimb();
        }

        UpdateUI();
    }

    public void BuyHealth()
    {
        int cost = healthCost * (healthLevel + 1);
        if (exp < cost || healthLevel >= MAX_HEALTH) return;

        exp -= cost;
        healthLevel++;

        if (playerHealth != null)
        {
            playerHealth.maxHealth = 3 + healthLevel;
            playerHealth.Heal(1);
        }

        UpdateUI();
    }

    public void BuyDamage()
    {
        int cost = damageCost * (damageLevel + 1);
        if (exp < cost || damageLevel >= MAX_DAMAGE) return;

        exp -= cost;
        damageLevel++;

        if (playerCombat != null && damageLevel < damageValues.Length)
        {
            playerCombat.attackDamage = damageValues[damageLevel];
        }

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
        hitCount++;
        if (hitCount >= 3)
        {
            hitCount = 0;
            float multiplier = 1.12f + (expLevel * 0.06f);
            int gained = Mathf.RoundToInt(10 * multiplier);
            exp += gained;
            UpdateUI();
            Debug.Log($"+{gained} EXP! Total: {exp}");
        }
    }

    public void UpdateUI()
    {
        if (expText != null) expText.text = $"EXP: {exp}";

        // Abilities
        if (dashButton != null) dashButton.gameObject.SetActive(!abilities[0]);
        if (doubleJumpButton != null) doubleJumpButton.gameObject.SetActive(!abilities[1]);
        if (wallClimbButton != null) wallClimbButton.gameObject.SetActive(!abilities[2]);

        // Upgrades
        if (healthText != null)
            healthText.text = healthLevel >= MAX_HEALTH ? "MAX" :
                $"HP +1\n{healthLevel}/{MAX_HEALTH}\n{healthCost * (healthLevel + 1)} EXP";

        if (damageText != null)
            damageText.text = damageLevel >= MAX_DAMAGE ? "MAX" :
                $"DMG: {damageValues[damageLevel + 1]}\n{damageLevel}/{MAX_DAMAGE}\n{damageCost * (damageLevel + 1)} EXP";

        if (expBonusText != null)
            expBonusText.text = expLevel >= MAX_EXP ? "MAX" :
                $"+{12 + expLevel * 6}%\n{expLevel}/{MAX_EXP}\n{expBonusCost * (expLevel + 1)} EXP";
    }

    public void OnPlayerRespawn()
    {
        FindPlayer();
    }
}