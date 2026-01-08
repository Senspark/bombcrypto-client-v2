using BLPvpMode.Engine.Entity;

using JetBrains.Annotations;

namespace BLPvpMode.Engine {
    public interface IEntityComponent {
        [NotNull]
        public IEntity Entity { get; }
    }
}