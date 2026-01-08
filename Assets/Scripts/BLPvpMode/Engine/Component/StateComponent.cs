using System;

using BLPvpMode.Engine.Entity;

namespace BLPvpMode.Engine {
    public sealed class StateComponent : IEntityComponent {
        private readonly Func<IEntityState> _stateGetter;
        public IEntity Entity { get; }
        public IEntityState State => _stateGetter();

        public StateComponent(IEntity entity, Func<IEntityState> stateGetter) {
            Entity = entity;
            _stateGetter = stateGetter;
        }
    }
}