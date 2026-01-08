using System;

using Game.Dialog.BomberLand.BLGacha;

using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.InGame {
    public class EmojiIcon : MonoBehaviour {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private Image iconHotKey;
        
        [SerializeField]
        private Image progress;

        [SerializeField]
        private UnityEngine.UI.Button button;
        
        [SerializeField]
        private BLGachaRes resource;

        [SerializeField]
        private Sprite[] hotKeySprites;
        
        private System.Action<int> _onClickedCallback;
        
        public bool Interactable {
            set => button.interactable = value;
            get => button.interactable;
        }
        
        public int ItemId { get; private set; }

        public void SetClickCallback(System.Action<int> callback) {
            _onClickedCallback = callback;
        }
        
        public async void SetItemId(int itemId) {
            ItemId = itemId;
            icon.sprite = await resource.GetSpriteByItemId(itemId);
            progress.sprite = icon.sprite;
        }

        public KeyCode[] GetHotKey(int hotKey) {
            var keyCodes = Array.Empty<KeyCode>();
            if (hotKey >= 0) {
                iconHotKey.sprite = hotKeySprites[hotKey];
                keyCodes = hotKey switch {
                    0 => new KeyCode[] {KeyCode.Alpha1, KeyCode.Keypad1},
                    1 => new KeyCode[] {KeyCode.Alpha2, KeyCode.Keypad2},
                    2 => new KeyCode[] {KeyCode.Alpha3, KeyCode.Keypad3},
                    3 => new KeyCode[] {KeyCode.Alpha4, KeyCode.Keypad4},
                };
            } else {
                iconHotKey.gameObject.SetActive(false);
                
            }
            return keyCodes;
        }
        
        public void OnClicked() {
            _onClickedCallback?.Invoke(ItemId);
        }

        public void SetProgress(float value) {
            progress.fillAmount = value;
        }
    }
}