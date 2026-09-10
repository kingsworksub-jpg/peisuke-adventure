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
    public int luck = 5;

    public Image hpBarFill;
    public Image hpBarFillMenu;
    public Text hpText;

    void Start()
    {
        currentHP = maxHP;
        currentMP = maxMP;
        UpdateHPBar();
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
