using System;

using UnityEngine;
using UnityEngine.Serialization;

namespace Utils {
    public class FixedTrail : MonoBehaviour {
        [FormerlySerializedAs("_length")]
        [SerializeField]
        private float length;

        [SerializeField]
        private TrailRenderer _renderer;

        [SerializeField]
        private Transform _transform;

        private Vector3 _position;
        private float _time;
        private float _velocity;

        private void Awake() {
            _position = _transform.position;
            _time = Time.time;
        }

        private void Update() {
            var position = _transform.position;
            if (position == _position) {
                return;
            }
            var velocity = (position - _position).magnitude / (Time.time - _time);
            _position = position;
            _time = Time.time;
            _renderer.time = length / velocity;
        }
    }
}