using Data;

using Game.UI.Custom;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopSubscriptionInfo : MonoBehaviour {
        [SerializeField] 
        private Text description;

        public void SetData(IAPSubscriptionItemData data) {
            description.text = data.Description;
        }
    }
}