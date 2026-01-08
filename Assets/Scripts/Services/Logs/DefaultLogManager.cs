using System;
using System.IO;
using System.Threading.Tasks;

using Senspark;

using UnityEngine;

namespace App {
    public class DefaultLogManager : IService, ILogManager {
        private readonly bool _enableLog;

        public DefaultLogManager(bool enableLog) {
            _enableLog = enableLog;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() { }

        public Task Initialize(float timeOut) {
            return Task.FromResult(true);
        }

        public void Log(
            string message = "",
            string memberName = "",
            string sourceFilePath = "",
            int sourceLineNumber = 0) {
            var fileName = Path.GetFileName(sourceFilePath);
            var time = DateTime.Now.ToString("hh:mm:ss");
            var messageWithColon = message.Length == 0 ? "" : $": {message}";
            var fullMessage = $"{fileName}:{sourceLineNumber}-{time}: {memberName + messageWithColon}";
            Debug.Log(fullMessage);
        }
    }
}