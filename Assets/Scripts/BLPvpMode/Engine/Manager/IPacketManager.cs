using System;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public interface IPacketManager {
        void Add([NotNull] Action action);
        void Flush();
    }
}