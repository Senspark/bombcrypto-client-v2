using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.Dialogs {
    public class RankRewardItem : MonoBehaviour {
        [SerializeField]
        private Text addressText;
        
        [SerializeField]
        private Text rankText;

        [SerializeField]
        private Text coinText;

        [SerializeField]
        private Text heroText;

        [SerializeField]
        private Text shieldText;

        public void SetInfo(int minRank, int maxRank, double coin, int hero, int shield) {
            if (minRank == 0 || minRank == maxRank) {
                rankText.text = "" + maxRank;
            } else {
                rankText.text = "" + minRank + " - " + maxRank;
            }
            coinText.text = "" + (int) coin;
            heroText.text = "" + hero;
            shieldText.text = "" + shield;
        }

        public void SetCurrentInfo(int rank, string userName, double coin, int hero, int shield) {
            addressText.text = userName;
            if (rank <= 0) {
                return;
            }
            rankText.text = "" + rank;
            coinText.text = "" + (int) coin;
            heroText.text = "" + hero;
            shieldText.text = "" + shield;
        }
    }
}
