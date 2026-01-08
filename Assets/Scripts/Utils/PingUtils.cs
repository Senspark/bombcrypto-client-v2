using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App;

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;

namespace Utils {
    public static class PingUtils {
        private class Data {
            private readonly int _max;
            private readonly Queue<TimeSpan> _values = new();

            public Data(int max) {
                _max = max;
            }

            public TimeSpan Value { get; private set; }

            public void AddValue(TimeSpan value) {
#if UNITY_EDITOR
                Debug.Log(value);
#endif
                _values.Enqueue(value);
                if (_values.Count > _max) {
                    _values.Dequeue();
                }
                Value = TimeSpan.FromTicks(_values.Sum(it => it.Ticks) / _values.Count);
            }
        }

        public static TimeSpan Time => _data.Value;

        private static readonly Data _data = new(3);

        public static void Initialize(string address, int delay) {
            UniTask.Void(async () => {
                while (true) {
                    _data.AddValue(await Ping(address));
                    await WebGLTaskDelay.Instance.Delay(delay);
                }
            });
        }

        private static async Task<TimeSpan> Ping(string address) {
            using var request = UnityWebRequest.Get(address);
            var now = DateTime.Now;
            await request.SendWebRequest();
            var delta = DateTime.Now - now;
#if UNITY_EDITOR
            Debug.Log($"address: {address}, delta: {delta}");
#endif
            return delta;
        }

        public static Task<TimeSpan> Ping(string address, int tryCount) {
            return MedianPing(address, tryCount);
        }

        private static async Task<TimeSpan> MedianPing(string address, int tryCount) {
            if (tryCount % 2 == 0) {
                throw new Exception($"Could not find median: {tryCount}");
            }
            var values = new TimeSpan[tryCount];
            for (var i = 0; i < values.Length; i++) {
                values[i] = await Ping(address);
            }
            Array.Sort(values, (a, b) => (int) (a - b).Ticks);
            return values[tryCount / 2];
        }

        private static async Task<TimeSpan> AveragePing(string address, int skip, int tryCount) {
            var count = 0;
            var ts = TimeSpan.Zero;
            while (count < tryCount) {
                var delta = await Ping(address);
                count++;
                if (count > skip) {
                    ts += delta;
                }
#if UNITY_EDITOR
                Debug.Log($"address: {address}, count: {count}, delta: {delta}, ts: {ts}");
#endif
            }
            var average = ts / (tryCount - skip);
#if UNITY_EDITOR
            Debug.Log($"address: {address}, try count: {tryCount}, average: {average}");
#endif
            return average;
        }
    }
}