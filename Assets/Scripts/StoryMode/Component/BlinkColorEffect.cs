using System;
using System.Diagnostics;

using Engine.Entities;

using UnityEngine;

namespace Engine.Components {
    public class BlinkColorEffect : MonoBehaviour {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private SpriteRenderer wingRenderer;

        [SerializeField]
        private TrailRenderer trailRenderer;

        private Action _onEndBlinkCallback;

        private float _spriteBlinkingTimer;
        private const float SpriteBlinkingMiniDuration = 0.15f;
        private float _spriteBlinkingTotalTimer;
        private float _spriteBlinkingTotalDuration = 2.0f;
        private bool _isBlinking;
        private bool _nonStopBlinking;
        public Color BlinkColor { set; private get; } = Color.red;
        private Color SpriteColor { get; set; }

        private void Awake() {
            var entity = GetComponent<Entity>();
            entity.GetEntityComponent<Updater>()
                .OnUpdate(delta => {
                    if (_isBlinking) {
                        SpriteBlinkingEffect(delta);
                    }
                });
        }

        private void Start() {
            SpriteColor = spriteRenderer.color;
        }

        public void StopBlink() {
            _isBlinking = false;
            _spriteBlinkingTotalTimer = 0.0f;
            spriteRenderer.color = SpriteColor;
            if (wingRenderer != null) {
                wingRenderer.color = SpriteColor;
            }
        }

        public void StartBlink(Action callback, float duration = 2.0f) {
            _spriteBlinkingTotalDuration = duration;
            _onEndBlinkCallback = callback;
            _nonStopBlinking = duration < 0;
            _isBlinking = true;
        }

        private void FinishBlink() {
            StopBlink();
            _onEndBlinkCallback?.Invoke();
        }

        private void SpriteBlinkingEffect(float delta) {
            if (!_nonStopBlinking) {
                _spriteBlinkingTotalTimer += delta;
                if (_spriteBlinkingTotalTimer >= _spriteBlinkingTotalDuration) {
                    FinishBlink();
                    return;
                }
            }

            _spriteBlinkingTimer += delta;
            if (!(_spriteBlinkingTimer >= SpriteBlinkingMiniDuration)) {
                return;
            }
            _spriteBlinkingTimer = 0.0f;

            if (spriteRenderer.color == SpriteColor) {
                spriteRenderer.color = BlinkColor;
                if (wingRenderer != null) {
                    wingRenderer.color = BlinkColor;
                }
            } else {
                spriteRenderer.color = SpriteColor;
                if (wingRenderer != null) {
                    wingRenderer.color = SpriteColor;
                }
            }
        }
    }
}