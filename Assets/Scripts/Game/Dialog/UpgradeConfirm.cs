using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;


public class UpgradeConfirm : MonoBehaviour
{
    [SerializeField]
    private Sprite rectOn;
    [SerializeField]
    private Color colorOn;
    [SerializeField]
    private Sprite rectOff;
    [SerializeField]
    private Color colorOff;
    [SerializeField]
    private Text underStand;

    [SerializeField] private Image imagebuttonUnderStand;
    [SerializeField] private Button buttonUpgrade;

    [SerializeField]
    private HeroRarityDisplay rarityDisplay;
    [SerializeField]
    private Text description;
    [SerializeField]
    private Avatar avatar;

    private bool chooseUnderStand;

    public void SetHero(PlayerData player)
    {
        if(rarityDisplay) {
            rarityDisplay.Show(player.rare);
        }

        if(avatar) {
            avatar.ChangeImage(player);
        }

        var rarityData = HeroRarityDisplay.GetRarityData(player.rare);

        var strDesc = ServiceLocator.Instance.Resolve<ILanguageManager>().GetValue(LocalizeKey.info_upgrade_warning_rare);
        description.text = string.Format(strDesc, rarityData.Name);
    }

    public void OnUnderStandClicked()
    {
        chooseUnderStand = !chooseUnderStand;
        if (chooseUnderStand)
        {
            imagebuttonUnderStand.sprite = rectOn;
            underStand.color = colorOn;
            buttonUpgrade.interactable = true;
        }
        else
        {
            imagebuttonUnderStand.sprite = rectOff;
            underStand.color = colorOff;
            buttonUpgrade.interactable = false;
        }
    }

}
