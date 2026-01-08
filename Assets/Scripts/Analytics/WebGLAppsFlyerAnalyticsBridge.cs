using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using App;

using Newtonsoft.Json;

using Senspark;

namespace Analytics {
    public class WebGLAppsFlyerAnalyticsBridge : IAppsFlyerAnalyticsBridge {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void initializeAppsFlyer(string webDevKey);

        [DllImport("__Internal")]
        private static extern void setAppsFlyerCUID(string userId);

        [DllImport("__Internal")]
        private static extern void logEventAppsFlyer(string eventName, string eventData);
#endif

        private readonly string _webDevKey;
        private readonly ILogManager _logManager;
        private static bool _initialized;

        public WebGLAppsFlyerAnalyticsBridge(
            ILogManager logManager, 
            string webDevKey) {
            _logManager = logManager;
            _webDevKey = webDevKey;
        }

        public Task<bool> Initialize() {
#if UNITY_WEBGL
            if (!_initialized && !AppConfig.IsTon()) {
                _initialized = true;
                initializeAppsFlyer(_webDevKey);
            }
#endif
            return Task.FromResult(true);
        }

        public void LogScene(string sceneName) {
        }

        public void SetWalletAddress(string address) {
#if UNITY_WEBGL
            setAppsFlyerCUID(address);
#endif
        }

        public void LogEvent(string name) {
#if UNITY_WEBGL
            if(!AppConfig.IsWebGL())
                return;
            logEventAppsFlyer(name, null);
#endif
        }

        public void LogEvent(string name, Parameter[] parameters) {
#if UNITY_WEBGL
            if(!AppConfig.IsWebGL())
                return;
            var d = parameters.ToDictionary(e => e.name, e => e.value);
            var j = JsonConvert.SerializeObject(d);
            logEventAppsFlyer(name, j);
#endif
        }

        public void LogEvent(string name, Dictionary<string, object> parameters) {
#if UNITY_WEBGL
            if(!AppConfig.IsWebGL())
                return;
            var j = JsonConvert.SerializeObject(parameters);
            logEventAppsFlyer(name, j);
#endif
        }
        
        public void PushGameLevel(int levelNo, string levelMode) { }

        public void PopGameLevel(bool winGame) { }

        public void LogAdRevenue(AdNetwork mediationNetwork, string monetizationNetwork, double revenue, string currencyCode,
            AdFormat format, string adUnit, Dictionary<string, object> extraParameters) { }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue, string currencyIso,
            string receipt) { }
        
    }
}