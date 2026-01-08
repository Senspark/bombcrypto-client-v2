using System;
using UnityEngine;

namespace Engine.Strategy.Reloader
{
    public class AutoReloader : IReloader
    {
        private float _cooldown;
        private float _elapsed;
        private bool _isStop = false;
        
        public float Progress => _cooldown > 0 ? Math.Min(_elapsed / _cooldown, 1) : 0;
        public bool IsReady => _cooldown > 0 && _elapsed >= _cooldown;
        public bool IsActive => _cooldown > 0;
        public AutoReloader(float cooldown)
        {
            _cooldown = cooldown;
            _isStop = _cooldown <= 0;
        }

        public void ChangeCooldown(float cooldown) {
            _cooldown = cooldown;
            _isStop = _cooldown <= 0;
        }

        public void Stop() {
            _isStop = true;
            _elapsed = 0;
        }

        public void Start() {
            if (_cooldown > 0) {
                _isStop = false;
                _elapsed = 0;
            }
        }
        
        public bool Reload()
        {
            if (!IsReady)
            {
                return false;
            }

            _isStop = false;
            _elapsed = 0;
            return true;
        }

        public void Update(float delta)
        {
            if (_isStop) {
                return;
            }
            
            _elapsed += delta;
        }
    }
}