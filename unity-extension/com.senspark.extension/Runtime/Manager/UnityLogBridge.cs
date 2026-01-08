using System.Threading.Tasks;

using UnityEngine;

namespace Senspark.Internal {
    public class UnityLogBridge : ILogBridge {
        private static readonly ILogger Logger = Debug.unityLogger;

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Log(string message) {
            Logger.Log(LogType.Log, message);
        }
    }
}