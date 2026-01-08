using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Senspark;

namespace Services.DeepLink {
    public class DefaultDeepLinkListener : IDeepLinkListener {
        private readonly IDeepLinkListener _bridge;

        public DefaultDeepLinkListener() {
#if UNITY_WEBGL
            _bridge = new WebDeepLinkListener();
#else
            _bridge = new MobileDeepLinkListener();
#endif
        }

        public Task<bool> Initialize() {
            return _bridge.Initialize();
        }

        public void Destroy() {
            _bridge.Destroy();
        }

        public Dictionary<string, string> GetDeepLinkData() {
            return _bridge.GetDeepLinkData();
        }

        public int AddObserver(DeepLinkObserver observer) {
            return _bridge.AddObserver(observer);
        }

        public bool RemoveObserver(int id) {
            return _bridge.RemoveObserver(id);
        }

        public void DispatchEvent(Action<DeepLinkObserver> dispatcher) {
            _bridge.DispatchEvent(dispatcher);
        }
    }
}