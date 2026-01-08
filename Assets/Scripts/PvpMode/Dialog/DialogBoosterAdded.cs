using Game.Dialog;

using PvpMode.Manager;
using PvpMode.UI;

using UnityEngine;

namespace PvpMode.Dialogs {
    public class DialogBoosterAdded : Dialog {
        [SerializeField]
        private BoosterButton[] boosters;
        
        public static DialogBoosterAdded Create() {
            var prefab = Resources.Load<DialogBoosterAdded>("Prefabs/PvpMode/Dialog/DialogBoosterAdded");
            return Instantiate(prefab);
        }

        public void SetInfo((BoosterType boosterType, Sprite spr, int quantity)[] items) {
            var i = 0;
            for (; i < items.Length; i++) {
                var item = items[i];
                var booster = boosters[i];
                booster.gameObject.SetActive(true);
                booster.ChangeBoosterType(item.boosterType, item.spr);
                booster.SetAddQuantity(item.quantity);
            }

            for (; i < boosters.Length; i++) {
                boosters[i].gameObject.SetActive(false);
            }
        }
    }
}
