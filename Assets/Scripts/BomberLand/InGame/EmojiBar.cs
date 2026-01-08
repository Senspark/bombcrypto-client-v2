using System;

using BomberLand.KeyboardInput;

using Senspark;

using UnityEngine;

namespace BomberLand.InGame {
    public class EmojiBar : MonoBehaviour {
        private EmojiIcon[] _emojis;
        
        private const float CoolDown = 5;
        
        private float _elapsed;
        private bool _isCountDown;

        private Action<int> _onEmojiChoose;
        private KeyboardInputListener _keyboardListener;
        private ObserverHandle _handle;
        
        public void SetEmoji(EmojiIcon[] emojis, System.Action<int> onEmojiChoose) {
            _emojis = emojis;
            _onEmojiChoose = onEmojiChoose;
            _keyboardListener = new KeyboardInputListener();
            for (var i = 0; i < _emojis.Length; i++) {
                var iter = _emojis[i];
                iter.SetClickCallback(OnEmojiClicked);
                var keyCodes = iter.GetHotKey(i);
                _keyboardListener.AddKeys(keyCodes, iter.ItemId);
            }
            _handle = new ObserverHandle();
            _handle.AddObserver(_keyboardListener, new KeyInputObserver() {
                    KeyDownOnItem = OnEmojiClicked
                });
        }

        private void OnEmojiClicked(int itemId) {
            if (_isCountDown) {
                return;
            }
            _onEmojiChoose(itemId);
            StartCoolDown();
        }
        
        private void StartCoolDown() {
            _elapsed = CoolDown;

            foreach (var iter in _emojis) {
                iter.Interactable = false;
                iter.SetProgress(1);
            }
            
            _isCountDown = true;
        }
        
        private void Update() {
            var delta = Time.deltaTime;
            _keyboardListener.OnProcess(delta);
            
            if (!_isCountDown) {
                return;
            }
            _elapsed -= delta;;
            if (_elapsed <= 0) {
                _isCountDown = false;
                OnEndCountDown();
                return;
            }
            var percent = (CoolDown - _elapsed) / CoolDown;
            SetProgress(1 - percent);
        }

        private void OnDestroy() {
            _keyboardListener.ClearKeys();
            _handle.Dispose();
        }

        private void OnEndCountDown() {
            foreach (var iter in _emojis) {
                iter.SetProgress(0);
                iter.Interactable = true;
            }            
        }
        
        private void SetProgress(float value) {
            foreach (var iter in _emojis) {
                iter.SetProgress(value);
            }
        }
    }
}