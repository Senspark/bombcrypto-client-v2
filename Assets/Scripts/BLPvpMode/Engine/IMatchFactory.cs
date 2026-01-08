using BLPvpMode.Engine.Manager;

using JetBrains.Annotations;

namespace BLPvpMode.Engine {
    public interface IMatchFactory {
        [NotNull]
        IMatchController Controller { get; }

        void Initialize();
        void Destroy();
    }
}