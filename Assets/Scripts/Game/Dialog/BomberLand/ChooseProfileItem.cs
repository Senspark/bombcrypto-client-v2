using System;

using PvpMode.Utils;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand {
    public class ChooseProfileItem : MonoBehaviour {
        [SerializeField]
        private Text account;
        
        [SerializeField]
        private Text dateTime;

        public string Account {
            set => account.text = Ellipsis.EllipsisAddress(value);
        }

        public event Action Clicked;

        public string DateTime {
            set => dateTime.text = value;
        }

        public void OnButtonSelectClicked() {
            Clicked?.Invoke();
        }
    }
}