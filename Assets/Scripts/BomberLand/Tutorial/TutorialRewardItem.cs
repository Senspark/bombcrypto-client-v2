using Game.Dialog.BomberLand.BLGacha;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Tutorial
{
    public class TutorialRewardItem : MonoBehaviour {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private TextMeshProUGUI footerText;

        [SerializeField]
        private BLGachaRes resource;

        public async void SetInfo(TutorialReward reward) {
            icon.sprite = await resource.GetSpriteByItemId(reward.ItemId);
            footerText.text = reward.Footer;
        }        
    }
}