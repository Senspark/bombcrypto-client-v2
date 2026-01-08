using System.Threading.Tasks;

namespace Senspark.Internal {
    public class NullFullScreenAd : ObserverManager<AdObserver>, IFullScreenAd {
        public bool IsLoaded => false;
        public void Dispose() { }
        public void Initialize() { }
        public Task<bool> Load() => Task.FromResult(false);

        Task<(AdResult result, string message)> IFullScreenAd.Show() =>
            Task.FromResult((AdResult.NotConfigured, "No ads"));
    }
}