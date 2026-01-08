using System;

using Engine.Entities;

using UnityEngine;
using UnityEngine.UI;

namespace Engine.Components {
    public class Shieldable : EntityComponentV2 {
        
        private Action _onEndShieldCallback;

        private float _spriteShieldTotalTimer;
        private float _shieldTotalDuration = 2.0f;
        private Image _shieldProcess;
        private SpriteRenderer _shieldEffect;

        public bool IsShielding { get; private set; }

        public Shieldable(SpriteRenderer shieldEffect,  Updater updater) {
            _shieldEffect = shieldEffect;
            updater
                .OnBegin(Init)
                .OnUpdate(delta => {
                    if (IsShielding) {
                        DoingShieldEffect(delta);
                    }
                });
        }

        private void Init() {
        }

        public void StartShield(Action callback, float duration, Image process) {
            _shieldEffect.gameObject.SetActive(true);
            _spriteShieldTotalTimer = 0;
            _shieldProcess = process;
            _shieldTotalDuration = duration;
            _onEndShieldCallback = callback;
            IsShielding = true;
        }

        public void StopShield() {
            _shieldEffect.gameObject.SetActive(false);
            IsShielding = false;
            _spriteShieldTotalTimer = 0.0f;
            _onEndShieldCallback?.Invoke();
        }
        

        private void DoingShieldEffect(float delta) {
            _spriteShieldTotalTimer += delta;
            if (_spriteShieldTotalTimer >= _shieldTotalDuration) {
                StopShield();
            } else if (_shieldProcess != null) {
                _shieldProcess.fillAmount = 1 - Math.Min(_spriteShieldTotalTimer / _shieldTotalDuration, 1);
            }
        }
    }
}