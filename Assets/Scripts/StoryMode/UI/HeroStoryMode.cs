using App;

using UnityEngine;
using UnityEngine.UI;

namespace StoryMode.UI {
    public class HeroStoryMode : MonoBehaviour {
        [SerializeField] private Image plus;
        [SerializeField] private Avatar avatar;
        [SerializeField] private Text id;
        [SerializeField] private Text level;
        [SerializeField] private Text power;
        [SerializeField] private Text upGradePower;
        [SerializeField] private Text stamina;
        [SerializeField] private Text speed;
        [SerializeField] private Text bombRange;
        [SerializeField] private Text bombNum;
        [SerializeField] private HeroRarityDisplay rarity;

        public void UpdateHero(PlayerData playerData) {
            if (playerData == null) 
            {
                plus.gameObject.SetActive(true);
                avatar.gameObject.SetActive(false);
                rarity.gameObject.SetActive(false);
                id.gameObject.SetActive(false);
                level.gameObject.SetActive(false);
                power.text = "0";
                upGradePower.text = null;
                stamina.text = "0";
                speed.text = "0";
                bombRange.text = "0";
                bombNum.text = "0";
            } else {
                plus.gameObject.SetActive(false);
                avatar.gameObject.SetActive(true);
                rarity.gameObject.SetActive(true);
                id.gameObject.SetActive(true);
                level.gameObject.SetActive(true);

                avatar.ChangeImage(playerData);
                rarity.Show(playerData.rare);
                id.text = playerData.heroId.Id.ToString();
                level.text = "Lv" + playerData.level;
                power.text = playerData.bombDamage.ToString("N0");
                var upgradePower = playerData.GetUpgradePower();
                upGradePower.text = upgradePower != 0 ? $"+{upgradePower:N0}" : "";
                stamina.text = playerData.stamina.ToString("N0");
                speed.text = playerData.speed.ToString("N0");
                bombRange.text = playerData.bombRange.ToString("N0");
                bombNum.text = playerData.bombNum.ToString(("N0"));
            }
        }
    }
}