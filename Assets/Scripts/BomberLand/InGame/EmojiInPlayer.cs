
using Engine.Utils;

using Game.Dialog.BomberLand.BLGacha;

using UnityEngine;

namespace BomberLand.InGame {
    public class EmojiInPlayer : MonoBehaviour {
        [SerializeField]
        private ImageAnimation imageAnimation;

        [SerializeField]
        private BLGachaRes resource;

        private const float CoolDown = 3;
        
        private float _elapsed;
        private bool _isCountDown;

        private System.Action _onEndCountDown;
        
        private void Update() {
            if (!_isCountDown) {
                return;
            }
            _elapsed -= Time.deltaTime;
            if (_elapsed > 0) {
                return;
            }
            _isCountDown = false;
            OnEndCountDown();
        }

        private void OnEndCountDown() {
            _onEndCountDown?.Invoke();
        }
        
        public void SetActive(bool value) {
            gameObject.SetActive(value);
            if (value) {
                _elapsed = CoolDown;
                _isCountDown = true;
            }
        }
        
        public async void SetAnimation(int itemId, System.Action callback) {
            _onEndCountDown = callback;
            var ani = await resource.GetAnimationByItemId(itemId);
            imageAnimation.StartAni(ani.AnimationIdle);
        }
    }
}