using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using App;

using Senspark;

namespace Analytics {
    public class WebGLFirebaseAnalyticsBridge : IAnalyticsBridge {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void initialize(
            string apiKey,
            string authDomain,
            string projectId,
            string storageBucket,
            string messagingSenderId,
            string appId,
            string measurementId
        );

        [DllImport("__Internal")]
        private static extern void logEvent(string eventName, string param);

        [DllImport("__Internal")]
        private static extern void logEvent(string eventName);
#endif

        private static bool _initialized;
        private readonly string _appId;
        private readonly string _measurementId;
        private readonly bool _enableLog;

        public WebGLFirebaseAnalyticsBridge(bool enableLog, string appId, string measurementId) {
            _enableLog = enableLog;
            _appId = appId;
            _measurementId = measurementId;
        }

        public Task<bool> Initialize() {
#if UNITY_WEBGL
            if (!_initialized) {
                _initialized = true;
                initialize(
                    AppConfig.FirebaseApiKey,
                    AppConfig.FirebaseAuthDomain,
                    AppConfig.FirebaseProjectId,
                    AppConfig.FirebaseStorageBucket,
                    AppConfig.FirebaseMessagingSenderId,
                    AppConfig.FirebaseAppId,
                    AppConfig.FirebaseMeasurementId
                );
            }
#endif
            return Task.FromResult(true);
        }

        public void LogScene(string sceneName) {
        }

        public void LogEvent(string name) {
#if UNITY_WEBGL
            if (_initialized && _enableLog) {
                logEvent(name);
            }
#endif
        }

        public void LogEvent(string name, Parameter[] parameters) {
#if UNITY_WEBGL
            if (_initialized && _enableLog) {
                var strParameters = ParseToString(parameters);
                logEvent(name, strParameters);
            }
#endif
        }

        public void LogEvent(string name, Dictionary<string, object> parameters) {
#if UNITY_WEBGL
            if (_initialized && _enableLog) {
                var strParameters = ParseToString(parameters);
                logEvent(name, strParameters);
            }
#endif
        }

        private static string ParseToString(Parameter[] parameters) {
            var str = "";
            foreach (var parameter in parameters) {
                str += parameter.name + ";" + parameter.value + ";";
            }
            return str;
        }
        
        public void PushGameLevel(int levelNo, string levelMode) { }

        public void PopGameLevel(bool winGame) { }

        public void LogAdRevenue(AdNetwork mediationNetwork, string monetizationNetwork, double revenue, string currencyCode,
            AdFormat format, string adUnit, Dictionary<string, object> extraParameters) { }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue, string currencyIso,
            string receipt) { }

        private static string ParseToString(Dictionary<string, object> parameters) {
            var str = "";
            foreach (var parameter in parameters) {
                str += parameter.Key + ";" + parameter.Value + ";";
            }
            return str;
        }
    }
}