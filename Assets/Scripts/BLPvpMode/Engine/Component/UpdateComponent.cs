using System;
using System.Collections.Generic;

using BLPvpMode.Engine.Entity;

using JetBrains.Annotations;

namespace BLPvpMode.Engine {
    public sealed class UpdateComponent : IEntityComponent {
        private readonly List<Action> _beginCallbacks = new(1);
        private readonly List<Action> _endCallbacks = new(1);
        private readonly List<Action<float>> _updateCallbacks = new(1);

        private float _timeMultiplier = 1;

        public IEntity Entity { get; }

        public float TimeMultiplier {
            get => _timeMultiplier;
            set => _timeMultiplier = value;
        }

        public UpdateComponent(IEntity entity) {
            Entity = entity;
        }

        public UpdateComponent OnBegin([NotNull] Action callback) {
            _beginCallbacks.Add(callback);
            return this;
        }

        public UpdateComponent OnEnd([NotNull] Action callback) {
            _endCallbacks.Add(callback);
            return this;
        }

        public UpdateComponent OnUpdate([NotNull] Action<float> callback) {
            _updateCallbacks.Add(callback);
            return this;
        }

        public void Begin() {
            for (var i = _beginCallbacks.Count - 1; i >= 0; --i) {
                _beginCallbacks[i]();
            }
        }

        public void End() {
            for (var i = _endCallbacks.Count - 1; i >= 0; --i) {
                _endCallbacks[i]();
            }
        }

        public void ProcessUpdate(float delta) {
            for (var i = _updateCallbacks.Count - 1; i >= 0; --i) {
                _updateCallbacks[i](delta * _timeMultiplier);
            }
        }
    }
}