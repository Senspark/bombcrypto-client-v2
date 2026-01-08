using Data;

using Game.Dialog.BomberLand.BLGacha;

using Services;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class BLEquipItem : MonoBehaviour {
        [SerializeField]
        private BLTablListAvatar avatar;

        [SerializeField]
        private Image highLight;

        [SerializeField]
        private Text quantity;
        
        private System.Action<ISkinManager.Skin> _onClickCallback;
        private ISkinManager.Skin _data;
        public ISkinManager.Skin SkinData => _data;

        public void OnClicked() {
            _onClickCallback(_data);
        }

        public async void SetInfo(BLGachaRes gachaRes, ISkinManager.Skin data, System.Action<ISkinManager.Skin> callback) {
            _data = data;
            _onClickCallback = callback;
            quantity.text = $"x{data.Quantity}";
            var sprite= await gachaRes.GetSpriteByItemId(data.SkinId);
            avatar.ChangeImage(sprite);
            // avatar.ChangeAvatarByItemId(data.SkinId);
        }
        
        public void SetSelected(bool value) {
            highLight.gameObject.SetActive(value);
        }

        public void UnEquip() {
            _data.UnEquip();
        }
    }
}