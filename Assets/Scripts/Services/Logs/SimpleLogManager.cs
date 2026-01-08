using System.Threading.Tasks;

using Senspark;

using UnityEngine;

namespace App {
    public class SimpleLogManager : ILogManager {
        private readonly bool _enableLog;

        public SimpleLogManager(bool enableLog) {
            _enableLog = enableLog;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() { }

        public Task Initialize(float timeOut) {
            return Task.FromResult(true);
        }

        public void Log(string message = "", string memberName = "", string sourceFilePath = "",
            int sourceLineNumber = 0) {
            if (!_enableLog && !Application.isEditor) {
                return;
            }
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "LOG: {0}", message);
        }
    }
}