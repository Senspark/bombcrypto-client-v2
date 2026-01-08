using BomberLand.InGame;

using Engine.Components;

using UnityEngine;

namespace Engine.Entities {
    public class Prison : Entity {
        [SerializeField]
        private PrisonAnimation prisonAnimation;

        [SerializeField]
        private EmojiInPlayer emoji;
        
        private bool _isJailCountDown = false;
        private float _jailDuration = 5.0f;
        private float _jailElapse = 0.0f;

        private void Awake() {
            var updater = new Updater()
                .OnUpdate(delta => {
                    if (!IsAlive) {
                        return;
                    }
                    if (!_isJailCountDown) {
                        return;
                    }
                    _jailElapse += delta;
                    if (_jailElapse >= _jailDuration) {
                        prisonAnimation.DoBroken();
                        _isJailCountDown = false;
                    }
                });
            AddEntityComponent<Updater>(updater);
        }

        public void Init(System.Action callback) {
            if (callback == null) {
                return;
            }
            prisonAnimation.Init(callback);
            _isJailCountDown = true;
        }

        public void ShowEmoji(int itemId) {
            emoji.SetAnimation(itemId, HideEmoji);
            emoji.SetActive(true);
        }

        private void HideEmoji() {
            emoji.SetActive(false);
        }
        
        public void JailBreak(System.Action callback) {
            _isJailCountDown = false;
            prisonAnimation.DoBroken(callback);
        }
    }
}