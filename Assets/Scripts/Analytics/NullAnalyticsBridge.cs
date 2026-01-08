using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Senspark;

namespace Analytics {
    public class NullAnalyticsBridge : IAppsFlyerAnalyticsBridge {
        private readonly ILogManager _logManager;
        private readonly string _parentName;

        public NullAnalyticsBridge(ILogManager logManager, string parentName) {
            _logManager = logManager;
            _parentName = parentName;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void LogScene(string sceneName) {
            _logManager.Log($"{_parentName} Scene Name: {sceneName}");
        }

        public void LogEvent(string name) {
            _logManager.Log($"{_parentName}: {name}");
        }

        public void LogEvent(string name, Parameter[] parameters) {
            _logManager.Log($"{_parentName}: {name}: {JsonConvert.SerializeObject(parameters)}");
        }

        public void LogEvent(string name, Dictionary<string, object> parameters) {
            _logManager.Log($"{_parentName} {name}");
        }

        public void PushGameLevel(int levelNo, string levelMode) { }

        public void PopGameLevel(bool winGame) { }

        public void LogAdRevenue(AdNetwork mediationNetwork, string monetizationNetwork, double revenue, string currencyCode,
            AdFormat format, string adUnit, Dictionary<string, object> extraParameters) { }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue, string currencyIso,
            string receipt) { }
        
        public void SetWalletAddress(string address) {
            _logManager.Log(address);
        }
    }
}