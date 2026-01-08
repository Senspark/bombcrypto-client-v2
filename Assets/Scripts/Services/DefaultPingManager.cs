using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App;

using BLPvpMode.Engine.Strategy.Network;
using BLPvpMode.Manager.Api;

using Senspark;

using UnityEngine;
using UnityEngine.Networking;

using UniTask = Cysharp.Threading.Tasks.UniTask;

namespace Services {
    public class DefaultPingManager : IPingManager {
        private class PingHelper {
            private readonly string _host;
            private readonly int _size;
            private readonly IStatsMeter _meter;
            private readonly Queue<int> _queue;

            public int Latency => _meter.Value;

            public PingHelper(string host, int size) {
                _host = host;
                _size = size;
                _meter = new MinStatsMeter();
                _queue = new Queue<int>();
            }

            public async Task<int> Ping() {
                var latency = await GetLatency(_host);
                _queue.Enqueue(latency);
                _meter.Add(latency);
                if (_queue.Count > _size) {
                    var value = _queue.Dequeue();
                    _meter.Remove(value);
                }
                return latency;
            }
        }

        private class PingInfo : IPingInfo {
            public string ZoneId { get; set; }
            public int Latency { get; set; }
        }

        private readonly ILogManager _logManager;
        private readonly IPvPServerConfigManager _configManager;
        private Task<bool> _initializer;
        private Dictionary<string, PingHelper> _helpers;
        private TaskCompletionSource<object> _firstPing;
        private bool _destroyed;

        private const int MAX_PING_COUNT = 3;
        private int _totalPingCount = 0;

        public DefaultPingManager(
            ILogManager logManager,
            IPvPServerConfigManager configManager
        ) {
            _logManager = logManager;
            _configManager = configManager;
        }

        public async Task<bool> Initialize() {
            await _configManager.InitializeAsync();
            var config = _configManager.GetConfig();
            _helpers = config.Zones.Associate(it => (it.ZoneId, new PingHelper(it.Host, 100)));
            _firstPing = new TaskCompletionSource<object>();
            UniTask.Void(async () => {
                while (_totalPingCount++ < MAX_PING_COUNT) {
                    if (_destroyed) {
                        return;
                    }
                    try {
                        await Ping();
                        _firstPing.TrySetResult(null);
                    } catch (Exception ex) {
                        Debug.LogException(ex);
                    }
                    await WebGLTaskDelay.Instance.Delay(1000);
                }
            });
            return true;
        }

        public void Destroy() {
            _destroyed = true;
        }

        private async Task Ping() {
            var values = await Task.WhenAll(_helpers.Select(async it => {
                var value = await it.Value.Ping();
                return $"[{it.Key}={value}]";
            }));
            // _logManager.Log(string.Join("", values));
        }

        public async Task<IPingInfo[]> GetLatencies() {
            await _firstPing.Task;
            var result = _helpers.Select(it => (IPingInfo) new PingInfo {
                ZoneId = it.Key, // 
                Latency = it.Value.Latency,
            }).ToArray();
            return result;
        }

        private static async Task<int> GetLatency(string url) {
            using var request = UnityWebRequest.Get(url);
            request.timeout = 3; // 3 seconds timeout.
            var now = DateTime.Now;
            await request.SendWebRequest();
            var delta = DateTime.Now - now;
            return (int) delta.TotalMilliseconds;
        }
    }
}