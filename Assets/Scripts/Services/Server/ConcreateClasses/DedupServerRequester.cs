using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App {
    public class DedupServerRequester : IDedupServerRequester {
        private readonly Dictionary<string, Task> _inflight = new();
        private readonly Dictionary<string, (DateTime At, Task Task)> _cache = new();
        private readonly TimeSpan _ttl;

        public DedupServerRequester(double ttlSeconds = 1) {
            _ttl = TimeSpan.FromSeconds(ttlSeconds);
        }

        public Task<T> Dedup<T>(string key, Func<Task<T>> fetch) {
            if (_inflight.TryGetValue(key, out var inflight)) {
                return (Task<T>) inflight;
            }
            if (_cache.TryGetValue(key, out var cached) && DateTime.UtcNow - cached.At < _ttl) {
                return (Task<T>) cached.Task;
            }
            var task = Run(key, fetch);
            _inflight[key] = task;
            return task;
        }

        private async Task<T> Run<T>(string key, Func<Task<T>> fetch) {
            try {
                var result = await fetch();
                _cache[key] = (DateTime.UtcNow, Task.FromResult(result));
                return result;
            } finally {
                _inflight.Remove(key);
            }
        }
    }
}
