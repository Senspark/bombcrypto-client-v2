using System;

namespace Engine.Utils {
    public class Timer {
        private readonly float _seconds;
        private readonly bool _loop;
        private readonly Action _timeReachedCb;
        private float _time;

        public Timer(float seconds, Action timeReached, bool loop) {
            _seconds = seconds;
            _loop = loop;
            _timeReachedCb = timeReached;
        }

        public void Update(float deltaTime) {
            _time += deltaTime;
            if (_time >= _seconds) {
                _timeReachedCb?.Invoke();
                if (_loop) {
                    _time = 0;
                }
            }
        }
    }
}