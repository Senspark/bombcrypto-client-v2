using System.Linq;

using Constant;

using Data;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class UpgradeStatButton : MonoBehaviour {
        [SerializeField]
        private StatId statId;

        [SerializeField]
        private Button button;

        [SerializeField]
        private Image icon;
        
        public StatId StatId => statId;
        
        public void InitWithConfig(ConfigUpgradeHeroData[] upgradeConfig, int feeIndex) {
            var isHpDamage = statId is StatId.Health or StatId.Damage;
            var upgradeType = isHpDamage ? "DMG_HP" : "SPEED_FIRE_BOMB";
            foreach (var iter in upgradeConfig) {
                if (iter.UpgradeType != upgradeType) {
                    continue;
                }
                button.interactable = CheckWithFees(iter.Fees, feeIndex);
                icon.gameObject.SetActive(button.interactable);
                break;
            }
        }

        public void SetActive(bool value) {
            button.interactable = value;
            icon.gameObject.SetActive(button.interactable);
        }

        private bool CheckWithFees(Fee[] feesConfig, int index) {
            return feesConfig.Any(fee => fee.Index == index);
        }
    }
}