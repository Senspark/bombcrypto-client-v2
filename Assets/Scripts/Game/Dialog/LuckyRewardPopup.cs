using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class LuckyRewardPopup : MonoBehaviour {
        [SerializeField]
        private Text rewardName;

        [SerializeField]
        private Image rewardIcon;

        [SerializeField]
        private Image[] imageIcons;

        private static readonly string[] RewardNames = new string[] {
            "RANK GUARDIAN",
            "F.CONQUEST CARD",
            "BOOSTER SHIELD",
            "HERO BOX BOMB 2",
            "BNB",
            "BOMB CANDY BALL",
            "HERO S BOX BOMB 1",
            "PIECE OF SKIN",
            "CONQUEST CARD",
            "F.RANK GUARDIAN"
        };

        public Sprite GetRewardSprite(int rewardId) {
            return imageIcons[rewardId].sprite;
        }
        
        public string GetRewardName(int rewardId) {
            return RewardNames[rewardId];
        }
        
        public void ShowMe(int rewardId) {
            rewardName.text = GetRewardName(rewardId);
            rewardIcon.sprite = GetRewardSprite(rewardId);
            gameObject.SetActive(true);
        }

        public void OnButtonOkClicked() {
            gameObject.SetActive(false);
        }
        
    }
}