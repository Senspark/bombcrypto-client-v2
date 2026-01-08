using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark.Internal {
    public class BaseFullScreenAd : ObserverManager<AdObserver>, IFullScreenAd {
        [Inject]
        private readonly ILogManager _logManager;

        [NotNull]
        private readonly IFullScreenAdBridge _bridge;

        private bool _initialized;

        public bool IsLoaded => _bridge.IsLoaded;

        public BaseFullScreenAd(
            [NotNull] IServiceLocator serviceLocator,
            [NotNull] IFullScreenAdBridge bridge
        ) {
            serviceLocator.ResolveInjection(this);
            _bridge = bridge;
        }

        public void Dispose() {
            _bridge.Dispose();
        }

        public void Initialize() {
            _bridge.Initialize();
            _initialized = true;
        }

        public async Task<bool> Load() {
            if (!_initialized) {
                return false;
            }
            var (result, message) = await _bridge.Load();
            if (result) {
                DispatchEvent(observer => observer.OnLoaded?.Invoke());
                return true;
            }
            _logManager.Log($"message={message}");
            DispatchEvent(observer => observer.OnFailedToLoad?.Invoke(message));
            return false;
        }

        public async Task<(AdResult, string)> Show() {
            var (result, message) = await ShowInternal();
            _logManager.Log($"result={result} message={message}");
            return (result, message);
        }

        private async Task<(AdResult, string)> ShowInternal() {
            if (!_initialized) {
                return (AdResult.NotInitialized, "No ads");
            }
            if (!IsLoaded) {
                return (AdResult.NotLoaded, "No ads");
            }
            return await _bridge.Show();
        }
    }
}