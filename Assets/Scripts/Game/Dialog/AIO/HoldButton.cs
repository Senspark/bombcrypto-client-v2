using System;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Dialog.AIO {
    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
        private float _downTime;
        private bool _enable = true;
        private Action<float> _onHold;

        public void Init(Action<float> onHold) {
            _onHold = onHold;
        }

        public void SetEnable(bool enable) {
            _enable = enable;
            _downTime = 0;
        }

        private void Update() {
            if (!_enable || _downTime <= 0) {
                return;
            }
            var now = Time.time;
            var timePassed = now - _downTime;
            if (now > 0) {
                _onHold?.Invoke(timePassed);
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            _downTime = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData) {
            _downTime = 0;
            _onHold?.Invoke(0);
        }
    }
}