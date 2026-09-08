using UnityEngine;
using UnityEngine.UI;

public class PeisukeStats : MonoBehaviour
{
    public int maxHP = 50;
    public int currentHP = 50;
    public Image hpBarFill;
    public Image hpBarFillMenu;
    public Text hpText;

    void Start()
    {
        currentHP = maxHP;
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
