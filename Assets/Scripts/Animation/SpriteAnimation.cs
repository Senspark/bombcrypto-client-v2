using System;
using System.Collections.Generic;

using UnityEngine;

public class SpriteAnimation : MonoBehaviour {
    [SerializeField]
    public int spritePerFrame = 10;

    [SerializeField]
    public int frameDelayBegin = 0;

    [SerializeField]
    public bool loop = true;

    private readonly List<Action> _onAnimationEndActions = new List<Action>();

    private SpriteRenderer _sprite;
    private Sprite[] _sprites;
    private int _index = 0;
    private int _frameDelay = 0;
    private float _time = 0;
    private float _timeFrameDelay = 0;

    private void Awake() {
        _sprite = GetComponent<SpriteRenderer>();
        _index = 0;
        _time = 0;
        _frameDelay = frameDelayBegin;
        _timeFrameDelay = 0.016f * spritePerFrame;
    }

    public void SetSprite(Sprite sprite) {
        _sprite.sprite = sprite;
    }

    public void DoFlip(bool flipX, float rotate) {
        _sprite.flipX = flipX;
        _sprite.transform.rotation = Quaternion.Euler(0, 0, rotate);
    }

    public void StartAnimation(Sprite[] sprites, Action callback = null) {
        _sprites = sprites;
        loop = false;
        _index = 0;
        _time = 0;
        _frameDelay = frameDelayBegin;
        _sprite.sprite = sprites[_index];

        if (callback != null) {
            _onAnimationEndActions.Add(callback);
        }
    }

    public void StartLoop(Sprite[] sprites, bool flipX = false) {
        _sprites = sprites;
        loop = true;
        // Nếu loop update index frame tiếp tục, thay vì bắt đầu lại frame 0
        _index %= sprites.Length;
        _time = Mathf.Min(_time, _timeFrameDelay);
        _frameDelay = frameDelayBegin;
        _sprite.sprite = _sprites[_index];
        _sprite.flipX = flipX;
    }

    public void Step(float delta) {
        if (_sprites == null) {
            return;
        }
        if (_sprites.Length == 0) {
            return;
        }
        if (!loop && _index == _sprites.Length) {
            return;
        }
        if (_frameDelay > 0) {
            _frameDelay--;
            return;
        }

        _time += delta;
        if (_time < _timeFrameDelay) {
            return;
        }
        _sprite.sprite = _sprites[_index];
        _time = 0;
        _index++;
        if (_index >= _sprites.Length) {
            if (loop) {
                _index = 0;
            }

            if (_onAnimationEndActions.Count > 0) {
                var action = _onAnimationEndActions[0];
                action.Invoke();
                _onAnimationEndActions.RemoveAt(0);
            }
        }
    }
}