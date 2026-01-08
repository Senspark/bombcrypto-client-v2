using System;

using App;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewHero : MonoBehaviour
{
    [SerializeField] private Text id;
    [SerializeField] private Avatar avatar;
    [SerializeField] private CharacterSkillPointDisplay charPower;
    [SerializeField] private CharacterSkillPointDisplay charSpeed;
    [SerializeField] private CharacterSkillPointDisplay charStamina;
    [SerializeField] private CharacterSkillPointDisplay charBombNum;
    [SerializeField] private CharacterSkillPointDisplay charBombRange;
    [SerializeField] private GameObject[] AbilityIcons;
    [SerializeField] private HeroRarityDisplay rarityDisplay;

    public void SetInfo(PlayerData player)
    {
        avatar.ChangeImage(player);
        //charName.text = player.genID;
        //charLv.text = "Lv" + player.level;

        id.text = player.heroId.Id.ToString();
        charPower.SetPoint(player.bombDamage, player.GetUpgradePower());
        charSpeed.SetPoint(player.speed);
        charStamina.SetPoint(player.stamina);
        charBombNum.SetPoint(player.bombNum);
        charBombRange.SetPoint(player.bombRange);

        foreach (var icon in AbilityIcons)
        {
            icon.SetActive(false);
        }
        foreach (var ability in player.abilities)
        {
            AbilityIcons[(int)ability].SetActive(true);
        }

        rarityDisplay.Show(player.rare);
    }

    public void SetIdAndRare(PlayerData player)
    {
        avatar.ChangeImage(player);
        id.text = player.heroId.Id.ToString();
        rarityDisplay.Show(player.rare);
    }

    public void OnBtnControlEnter(BaseEventData data)
    {
        var pointerEventData = (PointerEventData)data;
        if (pointerEventData == null)
        {
            return;
        }

        var ability = pointerEventData.pointerEnter.GetComponent<AbilityItem>();
        ability?.ShowTip();
    }

    public void OnBtnControlExit(BaseEventData data)
    {
        var pointerEventData = (PointerEventData)data;
        if (pointerEventData == null)
        {
            return;
        }

        var ability = pointerEventData.pointerEnter.GetComponent<AbilityItem>();
        ability?.HideTip();
    }

    public void Clear()
    {
        id.text = null;
        charPower.Clear();
        charSpeed.Clear();
        charStamina.Clear();
        charBombNum.Clear();
        charBombRange.Clear();
        Array.ForEach(AbilityIcons, e => e.SetActive(false));
        rarityDisplay.Hide();
    }
}
