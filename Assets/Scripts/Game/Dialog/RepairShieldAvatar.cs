using System;

using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class RepairShieldAvatar : MonoBehaviour {
        [SerializeField]
        private Avatar avatar;

        [SerializeField]
        private Image plusSymbol;

        [SerializeField]
        private Image background;

        private int _itemIndex;
        private Action<int> _onSelect;
        private ISoundManager _soundManager;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            avatar.gameObject.SetActive(false);
            plusSymbol.gameObject.SetActive(true);
        }

        public void Init(int itemIndex, Action<int> onClicked) {
            _itemIndex = itemIndex;
            _onSelect = onClicked;
        }

        public void SetEnable(bool enable) {
            var color = enable ? Color.white : new Color(1, 1, 1, 0.5f);
            plusSymbol.color = color;
            background.color = color;
        }

        public void SetData(PlayerData playerData) {
            if (playerData == null) {
                avatar.gameObject.SetActive(false);
                plusSymbol.gameObject.SetActive(true);
                return;
            }
            
            avatar.ChangeImage(playerData);
            avatar.gameObject.SetActive(true);
            plusSymbol.gameObject.SetActive(false);
        }

        public void OnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _onSelect?.Invoke(_itemIndex);
        }
    }
}