using UnityEngine;
using UnityEngine.UI;

public class StatusPopupController : MonoBehaviour
{
    public GameObject popupRoot;
    public PeisukeStats stats;

    public Text nameText;
    public Text levelText;
    public Text hpText;
    public Text mpText;
    public Text attackText;
    public Text defenseText;
    public Text magicAttackText;
    public Text magicDefenseText;
    public Text speedText;
    public Text luckText;
    public Text expText;

    public void Toggle()
    {
        if (popupRoot.activeSelf) Close();
        else Open();
    }

    public void Open()
    {
        popupRoot.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        popupRoot.SetActive(false);
    }

    void Refresh()
    {
        if (stats == null) return;
        if (nameText != null) nameText.text = stats.characterName;
        if (levelText != null) levelText.text = $"Lv. {stats.level}";
        if (hpText != null) hpText.text = $"HP  {stats.currentHP} / {stats.maxHP}";
        if (mpText != null) mpText.text = $"MP  {stats.currentMP} / {stats.maxMP}";
        if (attackText != null) attackText.text = $"こうげき  {stats.attack}";
        if (defenseText != null) defenseText.text = $"ぼうぎょ  {stats.defense}";
        if (magicAttackText != null) magicAttackText.text = $"まほうこうげき  {stats.magicAttack}";
        if (magicDefenseText != null) magicDefenseText.text = $"まほうぼうぎょ  {stats.magicDefense}";
        if (speedText != null) speedText.text = $"すばやさ  {stats.speed}";
        if (luckText != null) luckText.text = $"うん  {stats.luck}";
        if (expText != null) expText.text = $"EXP  {stats.exp} / {stats.expToNext}";
    }
}
