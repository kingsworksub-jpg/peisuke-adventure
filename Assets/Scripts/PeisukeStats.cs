using UnityEngine;
using UnityEngine.UI;

public class PeisukeStats : MonoBehaviour
{
    public string characterName = "ペイスケ";
    public int level = 1;
    public int exp = 0;
    public int expToNext = 100;

    public int maxHP = 50;
    public int currentHP = 50;
    public int maxMP = 20;
    public int currentMP = 20;

    public int attack = 10;
    public int defense = 8;
    public int magicAttack = 6;
    public int magicDefense = 6;
    public int speed = 12;

    const int MaxTableLevel = 99;
    static readonly int[] ExpTable = BuildExpTable(MaxTableLevel);

    static int[] BuildExpTable(int maxLevel)
    {
        var table = new int[maxLevel + 1]; // 1-based: table[level] = EXP needed to clear that level
        for (int lvl = 1; lvl <= maxLevel; lvl++)
            table[lvl] = 100 + (lvl - 1) * 50;
        return table;
    }

    static int GetExpToNextLevel(int lvl)
    {
        lvl = Mathf.Clamp(lvl, 1, MaxTableLevel);
        return ExpTable[lvl];
    }

    public const int PoopThreshold = 25;
    public int poopMeter = 0;
    const float StepLength = 0.8f; // world units per "step" (~one full stride)
    float stepDistanceAccum = 0f;

    public int snackCount = 0;
    public Text snackText;

    public Image hpBarFill;
    public Image hpBarFillMenu;
    public Text hpText;
    public Image[] poopSegments;
    public Color poopSegmentFilledColor = new Color(0.55f, 0.38f, 0.18f, 1f);
    public Color poopSegmentEmptyColor = new Color(0.2f, 0.2f, 0.2f, 0.85f);

    void Start()
    {
        currentHP = maxHP;
        currentMP = maxMP;
        expToNext = GetExpToNextLevel(level);
        UpdateHPBar();
        UpdatePoopBar();
        UpdateSnackText();
    }

    public void ReportDistanceMoved(float distance)
    {
        stepDistanceAccum += distance;
        while (stepDistanceAccum >= StepLength)
        {
            stepDistanceAccum -= StepLength;
            OnStep();
        }
    }

    void OnStep()
    {
        poopMeter++;
        if (poopMeter >= PoopThreshold)
        {
            poopMeter = 0;
            UpdatePoopBar();
            GetComponent<PoopEffect>()?.PlayPoop();
        }
        else
        {
            UpdatePoopBar();
        }
    }

    public void GoForWalk()
    {
        if (poopMeter > 0)
            GainExp(poopMeter);
        poopMeter = 0;
        UpdatePoopBar();
    }

    public void GainExp(int amount)
    {
        exp += amount;
        while (exp >= expToNext)
        {
            exp -= expToNext;
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        expToNext = GetExpToNextLevel(level);

        maxHP += 5;
        maxMP += 2;
        attack += 2;
        defense += 1;
        magicAttack += 1;
        magicDefense += 1;
        speed += 1;

        currentHP = maxHP;
        currentMP = maxMP;
        UpdateHPBar();
    }

    public void CollectSnack()
    {
        snackCount++;
        UpdateSnackText();
    }

    void UpdateSnackText()
    {
        if (snackText != null) snackText.text = $"おやつ  {snackCount}こ";
    }

    void UpdatePoopBar()
    {
        if (poopSegments == null) return;
        for (int i = 0; i < poopSegments.Length; i++)
        {
            if (poopSegments[i] == null) continue;
            poopSegments[i].color = i < poopMeter ? poopSegmentFilledColor : poopSegmentEmptyColor;
        }
    }

    public void SetHP(int value)
    {
        currentHP = Mathf.Clamp(value, 0, maxHP);
        UpdateHPBar();
    }

    public void TakeDamage(int amount) => SetHP(currentHP - amount);
    public void Heal(int amount) => SetHP(currentHP + amount);

    void UpdateHPBar()
    {
        float ratio = maxHP > 0 ? (float)currentHP / maxHP : 0f;
        if (hpBarFill != null) hpBarFill.fillAmount = ratio;
        if (hpBarFillMenu != null) hpBarFillMenu.fillAmount = ratio;
        if (hpText != null) hpText.text = $"{currentHP}/{maxHP}";
    }
}
