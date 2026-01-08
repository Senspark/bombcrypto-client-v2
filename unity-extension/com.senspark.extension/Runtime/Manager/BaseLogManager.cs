using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark.Internal {
    public class BaseLogManager : ILogManager {
        [NotNull]
        private readonly ILogBridge _bridge;

        [NotNull]
        private readonly Queue<string> _pendingMessages;

        [CanBeNull]
        private Task _initializer;

        private bool? _initialized;

        public BaseLogManager([NotNull] ILogBridge bridge) {
            _bridge = bridge;
            _pendingMessages = new Queue<string>();
        }

        public Task Initialize(float timeOut) => _initializer ??= InitializeImpl(timeOut);

        [NotNull]
        private async Task InitializeImpl(float timeOut) {
            await Task.WhenAny(((Func<Task>) (async () => {
                var result = await _bridge.Initialize();
                _initialized = result;
                if (result) {
                    while (_pendingMessages.Count > 0) {
                        _bridge.Log(_pendingMessages.Dequeue());
                    }
                }
            }))(), ((Func<Task>) (async () => { //
                await Task.Delay((int) (timeOut * 1000));
            }))());
        }

        public void Log(
            string message = "",
            string memberName = "",
            string sourceFilePath = "",
            int sourceLineNumber = 0) {
            var fileName = Path.GetFileName(sourceFilePath);
            var messageWithColon = message.Length == 0 ? "" : $": {message}";
            var fullMessage = $"{fileName}:{sourceLineNumber}: {memberName + messageWithColon}";
            if (_initialized == null) {
                _pendingMessages.Enqueue(fullMessage);
                return;
            }
            if (!_initialized.Value) {
                return;
            }
            _bridge.Log(fullMessage);
        }
    }
}