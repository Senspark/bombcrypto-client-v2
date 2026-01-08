using System;

using Engine.Entities;

using UnityEngine;

namespace Engine.Components {
    public class BlinkEffect : MonoBehaviour {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private SpriteRenderer wingRenderer;

        [SerializeField]
        private GameObject trailRenderer;
        
        private Action _onEndBlinkCallback;

        private float _spriteBlinkingTimer;
        private const float SpriteBlinkingMiniDuration = 0.15f;
        private float _spriteBlinkingTotalTimer;
        private float _spriteBlinkingTotalDuration = 2.0f;
        private bool _isBlinking;
        private bool _nonStopBlinking;
        public bool IsBlinkRed { set; private get; } = false;
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
            if (IsBlinkRed) {
                spriteRenderer.color = SpriteColor;
                if (wingRenderer != null) {
                    wingRenderer.color = SpriteColor;
                }
            } else {
                spriteRenderer.enabled = true;
                if (wingRenderer != null) {
                    wingRenderer.enabled = true;
                }
                if (trailRenderer != null) {
                    trailRenderer.gameObject.SetActive(true);
                }
            }
        }

        public void StartBlink(Action callback, float duration = 2.0f) {
            _spriteBlinkingTotalDuration = duration;
            _onEndBlinkCallback = callback;
            _nonStopBlinking = duration < 0;
            _isBlinking = true;
        }

        public void SetTrailRenderer(GameObject t) {
            trailRenderer = t;
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

            if (IsBlinkRed) {
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
            } else {
                if (spriteRenderer.enabled) {
                    spriteRenderer.enabled = false;
                    if (wingRenderer != null) {
                        wingRenderer.enabled = false;
                    }
                    if (trailRenderer != null) {
                        trailRenderer.SetActive(false);
                    }
                } else {
                    spriteRenderer.enabled = true;
                    if (wingRenderer != null) {
                        wingRenderer.enabled = true;
                    }
                    if (trailRenderer != null) {
                        trailRenderer.SetActive(true);
                    }
                }
            }
        }
    }
}