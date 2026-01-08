using Game.UI;

using UnityEngine;
using UnityEngine.UI;

namespace GameUI {
    public class BLInventoryEmojiSlot : MonoBehaviour {
        [SerializeField]
        private BLTablListAvatar avatar;

        [SerializeField]
        private Button button;
        
        private int _itemId;
        private System.Action<int> _clickedCallback;

        public void SetClickedCallback(System.Action<int> callback) {
            _clickedCallback = callback;
        }
        
        public void OnClicked() {
            if (_itemId > 0) {
                _clickedCallback?.Invoke(_itemId);
            }
        }
        
        public void SetItemToSlot(int itemId) {
            _itemId = itemId;
            avatar.gameObject.SetActive(true);
            avatar.ChangeAvatarByItemId(itemId);
            button.interactable = true;
        }

        public void UnsetItem() {
            _itemId = -1;
            avatar.gameObject.SetActive(false);
            button.interactable = false;
        }

        public int GetItemId() {
            return _itemId;
        }
        
        public bool IsEmpty() {
            return _itemId < 0;
        }
    }
}