#if !UNITY_WEBGL
using System.Threading.Tasks;

using Firebase.Crashlytics;

namespace Senspark.Internal {
    internal class FirebaseLogBridge : ILogBridge {
        public async Task<bool> Initialize() {
            var status = await FirebaseInitializer.Initialize();
            Crashlytics.ReportUncaughtExceptionsAsFatal = true;
            return status;
        }

        public void Log(string message) {
            Crashlytics.Log(message);
        }
    }
}
#endif