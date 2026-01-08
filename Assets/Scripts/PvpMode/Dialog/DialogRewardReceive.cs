using App;

using Game.Dialog;

using PvpMode.Services;

using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.Dialogs {
    public class DialogRewardReceive : Dialog {
        [SerializeField]
        private Text heroText;

        [SerializeField]
        private Text coinText;

        [SerializeField]
        private Text shieldText;

        public static DialogRewardReceive Create() {
            var prefab = Resources.Load<DialogRewardReceive>("Prefabs/PvpMode/Dialog/DialogRewardReceive");
            return Instantiate(prefab);
        }

        public void SetInfo(IPvpCurrentRewardResult reward) {
            heroText.text = "+" + reward.HeroBox;
            coinText.text = "+" + (int) reward.BCoin;
            shieldText.text = "+" + reward.Shield;
        }
    }
}
