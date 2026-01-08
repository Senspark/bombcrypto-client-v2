using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark.Internal {
    public class LocalRemoteConfigBridge : IRemoteConfigBridge {
        private readonly Dictionary<string, object> _defaults;

        public LocalRemoteConfigBridge([NotNull] Dictionary<string, object> defaults) {
            _defaults = defaults;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public Task<bool> ForceFetch() {
            return Task.FromResult(true);
        }

        public bool GetBool(string key) {
            return (bool) _defaults[key];
        }

        public long GetLong(string key) {
            var value = _defaults[key];
            return value switch {
                int v => v,
                long v => v,
                _ => throw new InvalidCastException(key),
            };
        }

        public double GetDouble(string key) {
            var value = _defaults[key];
            return value switch {
                float v => v,
                double v => v,
                _ => throw new InvalidCastException(key),
            };
        }

        public string GetString(string key) {
            var value = _defaults[key];
            return value switch {
                string v => v,
                _ => throw new InvalidCastException(key),
            };
        }
    }
}