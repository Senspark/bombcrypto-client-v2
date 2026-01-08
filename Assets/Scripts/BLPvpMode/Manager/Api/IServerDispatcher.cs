using JetBrains.Annotations;

namespace BLPvpMode.Manager.Api {
    public interface IServerDispatcher {
        void AddListener([NotNull] IServerListener listener);
        void RemoveListener([NotNull] IServerListener listener);
    }
}