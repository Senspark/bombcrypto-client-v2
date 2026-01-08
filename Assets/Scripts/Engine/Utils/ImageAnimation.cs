using System;

using UnityEngine;
using UnityEngine.UI;

namespace Engine.Utils {
    public class ImageAnimation : MonoBehaviour {
        [SerializeField]
        public Sprite[] sprites = null;

        [SerializeField]
        public int spritePerFrame = 50;

        [SerializeField]
        public int frameDelayBegin = 0;

        [SerializeField]
        public bool autoPlay = false;

        [SerializeField]
        public bool loop = true;

        [SerializeField]
        public bool destroyOnEnd = false;

        private int _index = 0;
        private Image _image;
        private SpriteRenderer _sprite;
        private float _time = 0;
        private float _timeFrameDelay = 0;
        private int _frameDelay = 0;
        private Action _onDoneAni;

        private Sprite SpriteTarget {
            set {
                if (_image) {
                    _image.sprite = value;
                }
                if (_sprite) {
                    _sprite.sprite = value;
                }
            }
        }

        private bool SpriteEnable {
            set {
                if (_image) {
                    _image.enabled = value;
                }
                if (_sprite) {
                    _sprite.enabled = value;
                }
            }
        }

        private void Awake() {
            _image = GetComponent<Image>();
            _sprite = GetComponent<SpriteRenderer>();
            _index = 0;
            _time = 0;
            _frameDelay = frameDelayBegin;
            if (autoPlay && sprites != null) {
                SpriteTarget = sprites[_index];
                SpriteEnable = true;
            } else {
                SpriteEnable = false;
            }
            _timeFrameDelay = 0.016f * spritePerFrame;
        }

        public void SetImageSprite(Sprite sprite) {
            _image.sprite = sprite;
        }
        
        public void StartAni(Sprite[] animationOpen) {
            sprites = animationOpen;
            _index = 0;
            _time = 0;
            _frameDelay = frameDelayBegin;
            SpriteTarget = sprites[_index];
            SpriteEnable = true;
        }
        
        public void StartLoop(Sprite[] animationIdle, bool flipX = false) {
            if (_image == null) {
                return;
            }
            if (animationIdle == null || animationIdle.Length == 0) {
                return;
            }
            sprites = animationIdle;
            loop = true;
            _index = 0;
            _time = 0;
            _frameDelay = frameDelayBegin;
            _image.sprite = sprites [_index];
            
            var trans = _image.transform;
            var scale = trans.localScale;
            if (flipX && scale.x > 0 || 
                !flipX && scale.x < 0) {
                scale.x = -scale.x;
            }
            trans.localScale = scale;
            _image.enabled = true;
            _onDoneAni = null;
        }

        public void SetOnDoneAni(Action onDoneAni) {
            _onDoneAni = onDoneAni;
        }

        private void Update() {
            if (sprites == null) {
                return;
            }
            if (sprites.Length == 0) {
                return;
            }
            if (!loop && _index == sprites.Length) {
                return;
            }
            if (_frameDelay > 0) {
                _frameDelay--;
                return;
            }
            _time += Time.deltaTime;
            if (_time < _timeFrameDelay) {
                return;
            }
            SpriteTarget = sprites[_index];
            _time = 0;
            _index++;
            if (_index >= sprites.Length) {
                if (loop) {
                    _index = 0;
                }
                _onDoneAni?.Invoke();
                if (destroyOnEnd) {
                    Destroy(gameObject);
                }
            }
        }
    }
}