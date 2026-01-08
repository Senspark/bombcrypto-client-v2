using Com.LuisPedroFonseca.ProCamera2D;

using Engine.Entities;

using UnityEngine;
using UnityEngine.Assertions;

namespace Engine.Camera {
    public class ProCamera : ICamera {
        private Entity _target;
        private readonly float _maxHoriScroll;
        private readonly float _minHoriScroll;
        private readonly float _maxVertScroll;
        private readonly float _minVertScroll;

        private readonly ProCamera2D _proCamera2D;

        private bool _isShaking = false;
        private float _shakeDuration = 1f;
        private float _shakeAmount = 0.7f;
        private Vector3 _originalPos;

        public ProCamera(ProCamera2D proCamera2D, float maxHoriScroll, float minHoriScroll) {
            _proCamera2D = proCamera2D;
            _target = null;
            _maxHoriScroll = maxHoriScroll;
            _minHoriScroll = minHoriScroll;
            _maxVertScroll = 0;
            _minVertScroll = 0;
        }

        public ProCamera(ProCamera2D proCamera2D, float maxHoriScroll, float minHoriScroll, float maxVertScroll,
            float minVertScrool) {
            _proCamera2D = proCamera2D;
            _target = null;
            _maxHoriScroll = maxHoriScroll;
            _minHoriScroll = minHoriScroll;
            _maxVertScroll = maxVertScroll;
            _minVertScroll = minVertScrool;
        }

        public ProCamera(ProCamera2D proCamera2D, float maxHoriScroll) {
            _proCamera2D = proCamera2D;
            _target = null;
            _maxHoriScroll = maxHoriScroll;
            _minHoriScroll = -maxHoriScroll;
            _maxVertScroll = 0;
            _minVertScroll = 0;
        }

        public void SetTarget(Entity target) {
            Assert.IsTrue(target != null);
            //Assert.IsTrue(this.target == null);
            _target = target;
        }

        public void ClearTarget() {
            Assert.IsTrue(_target != null);
            _target = null;
        }

        public void Shaking(float duration, float amount) {
            _shakeDuration = duration;
            _shakeAmount = amount;
            _isShaking = true;
        }

        public void Process(float delta) {
            if (_isShaking) {
                _shakeDuration -= Time.deltaTime;
                var cameraTarget = _proCamera2D.CameraTargets[0];
                var panTarget = cameraTarget.TargetTransform;
                if (_shakeDuration > 0) {
                    panTarget.localPosition = _originalPos + Random.insideUnitSphere * _shakeAmount;
                    _shakeDuration -= Time.deltaTime;
                } else {
                    panTarget.localPosition = _originalPos;
                    _isShaking = false;
                }
                return;
            }

            if (_target != null && _target.IsAlive) {
                FollowTarget();
            }
        }

        public void ProcessPanHorizontal(float delta) {
            var cameraTarget = _proCamera2D.CameraTargets[0];
            var panTarget = cameraTarget.TargetTransform;
            var panPosition = panTarget.position;

            if (_maxHoriScroll >= _minHoriScroll) {
                if (panPosition.x > _maxHoriScroll) {
                    panPosition.x = _maxHoriScroll;
                }
                else if (panPosition.x < _minHoriScroll) {
                    panPosition.x = _minHoriScroll;
                }
            }

            panTarget.position = panPosition;
            _originalPos = panTarget.position;            
        }

        public void MoveToY(float y) {
            var cameraTarget = _proCamera2D.CameraTargets[0];
            var panTarget = cameraTarget.TargetTransform;
            var position = panTarget.position;
            position.y = y;
            panTarget.position = position;
            _originalPos = position;
        }
        
        private void FollowTarget() {
            var cameraTarget = _proCamera2D.CameraTargets[0];
            var panTarget = cameraTarget.TargetTransform;
            var panPosition = panTarget.position;

            if (_maxHoriScroll >= _minHoriScroll) {
                panPosition.x = _target.transform.position.x;
                if (panPosition.x > _maxHoriScroll) {
                    panPosition.x = _maxHoriScroll;
                }
                else if (panPosition.x < _minHoriScroll) {
                    panPosition.x = _minHoriScroll;
                }
            }

            if (_maxVertScroll >= _minVertScroll) {
                panPosition.y = _target.transform.position.y;
                if (panPosition.y > _maxVertScroll) {
                    panPosition.y = _maxVertScroll;
                }
                else if (panPosition.y < _minVertScroll) {
                    panPosition.y = _minVertScroll;
                }
            }

            panTarget.position = panPosition;
            _originalPos = panTarget.position;
        }
    }
}