using System;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark.Internal {
    public class BaseRemoteConfigManager : IRemoteConfigManager {
        [NotNull]
        private readonly IRemoteConfigBridge _bridge;

        [NotNull]
        private readonly IRemoteConfigBridge _fallbackBridge;

        [CanBeNull]
        private Task _initializer;

        private bool _initialized;

        public BaseRemoteConfigManager(
            [NotNull] IRemoteConfigBridge bridge,
            [NotNull] IRemoteConfigBridge fallbackBridge
        ) {
            _bridge = bridge;
            _fallbackBridge = fallbackBridge;
        }

        public Task Initialize(float timeOut) => _initializer ??= InitializeImpl(timeOut);
        
        public Task<bool> ForceFetch() {
            return _initialized ? _bridge.ForceFetch() : _fallbackBridge.ForceFetch();
        }

        [NotNull]
        private async Task InitializeImpl(float timeOut) {
            await Task.WhenAny(((Func<Task>) (async () => {
                var result = await _bridge.Initialize();
                _initialized = result;
            }))(), ((Func<Task>) (async () => { //
                await Task.Delay((int) (timeOut * 1000));
            }))());
        }

        public bool GetBool(string key) {
            return _initialized ? _bridge.GetBool(key) : _fallbackBridge.GetBool(key);
        }

        public long GetLong(string key) {
            return _initialized ? _bridge.GetLong(key) : _fallbackBridge.GetLong(key);
        }

        public double GetDouble(string key) {
            return _initialized ? _bridge.GetDouble(key) : _fallbackBridge.GetDouble(key);
        }

        public string GetString(string key) {
            return _initialized ? _bridge.GetString(key) : _fallbackBridge.GetString(key);
        }
    }
}