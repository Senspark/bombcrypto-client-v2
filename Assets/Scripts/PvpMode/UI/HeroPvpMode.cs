using App;

using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.UI {
    public class HeroPvpMode : MonoBehaviour {
        [SerializeField]
        private Image plus;

        [SerializeField]
        private Avatar avatar;
        
        [SerializeField]
        private Text id;

        [SerializeField]
        private Text level;

        [SerializeField]
        private Sprite[] batterySprites;
        
        [SerializeField]
        private Image batteryImage;

        [SerializeField]
        private Text battery;
        
        [SerializeField]
        private HeroRarityDisplay rarity;

        public void UpdateHero(PlayerData playerData) {
            if (playerData == null) {
                plus.gameObject.SetActive(true);
                avatar.gameObject.SetActive(false);
                rarity.gameObject.SetActive(false);
                id.gameObject.SetActive(false);
                level.gameObject.SetActive(false);
                battery.gameObject.SetActive(false);
            } else {
                plus.gameObject.SetActive(false);
                avatar.gameObject.SetActive(true);
                rarity.gameObject.SetActive(true);
                id.gameObject.SetActive(true);
                level.gameObject.SetActive(true);
                battery.gameObject.SetActive(true);
                avatar.ChangeImage(playerData);
                rarity.Show(playerData.rare);
                id.text = playerData.heroId.Id.ToString();
                level.text = "Lv" + playerData.level;
                var batteryValue = playerData.battery;
                batteryImage.sprite = batterySprites[batteryValue > 3 ? 3 : batteryValue];
                battery.text = "" + batteryValue;
            }
        }
   }
}
