using System;
using System.Threading.Tasks;

using App;

namespace BLPvpMode.Manager.Api {
    public class LagSimulator : ILagSimulator {
        private readonly int _minLatency;
        private readonly int _maxLatency;

        public LagSimulator(
            int minLatency,
            int maxLatency
        ) {
            _minLatency = minLatency;
            _maxLatency = maxLatency;
        }

        public async Task Process(Func<Task> action) {
            var latency = UnityEngine.Random.Range(_minLatency, _maxLatency);
            await WebGLTaskDelay.Instance.Delay(latency);
            await action();
        }

        public async Task<T> Process<T>(Func<Task<T>> action) {
            var latency = UnityEngine.Random.Range(_minLatency, _maxLatency);
            await WebGLTaskDelay.Instance.Delay(latency);
            return await action();
        }
    }
}