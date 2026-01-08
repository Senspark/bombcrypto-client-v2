using System.Collections.Generic;
using System.Threading.Tasks;

using Senspark;

namespace Services.DeepLink {
    public class WebDeepLinkListener : ObserverManager<DeepLinkObserver>, IDeepLinkListener {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public Dictionary<string, string> GetDeepLinkData() {
            return new Dictionary<string, string>();
        }
    }
}