using System;

using UnityEngine;

namespace Utils {
    public class GmToolTrigger {
        public delegate void Log(string message);

        public delegate void Trigger();

        public bool IsTrigger => _touchCount == _testers.Length;

        private bool _locked;
        private readonly Log _log;

        private readonly Func<Vector2, bool>[] _testers = {
            pos => pos.x > 0 && pos.y > 0,
            pos => pos.x > 0 && pos.y < 0,
            pos => pos.x < 0 && pos.y < 0,
            pos => pos.x < 0 && pos.y > 0
        };

        private readonly Trigger _trigger;
        private int _touchCount;
        private long _touchTime;

        public GmToolTrigger(Log log, Trigger trigger) {
            _log = log;
            _trigger = trigger;
        }

        public void Reset() {
            _locked = false;
            _touchCount = 0;
        }

        public void ProcessEvents() {
            if (_locked) {
                return;
            }
            if (!Input.GetMouseButtonUp(0)) {
                return;
            }
            var now = DateTime.Now.Ticks;
            _touchTime = now;
            if (now - _touchTime > 500 * TimeSpan.TicksPerMillisecond ||
                !_testers[_touchCount](Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2))) {
                _touchCount = 0;
                return;
            }
            _touchCount++;
            if (_touchCount == _testers.Length) {
                _trigger();
            }
            _log($"[GMToolTrigger] _touchCount: {_touchCount}");
        }
    }
}