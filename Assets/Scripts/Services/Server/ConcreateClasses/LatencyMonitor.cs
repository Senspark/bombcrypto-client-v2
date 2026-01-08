using System;
using System.Collections;

using Senspark;

using UnityEngine;

using Object = UnityEngine.Object;

namespace App {
    public class LatencyMonitor {
        private bool _isMonitoring; // Flag to indicate if the server is being monitored
        private float _lastPingTime; // Timestamp of the last successful ping

        private readonly float _pingInterval; // Interval between ping attempts
        private readonly float _disconnectThreshold; // Threshold for considering the server disconnected
        private Runner _runner;
        private readonly Action _serverDisconnected; // Event raised when the server is disconnected

        private readonly ILogManager _logManager;
        public LatencyMonitor(float pingInterval, float disconnectThreshold, Action serverDisconnected, ILogManager logManager) {
            _pingInterval = pingInterval;
            _disconnectThreshold = disconnectThreshold;
            _serverDisconnected = serverDisconnected;
            _logManager = logManager;
        }

        public void StartMonitoring() {
            if (_isMonitoring) {
                return;
            }
            _isMonitoring = true;
            _lastPingTime = Time.time;
            var go = new GameObject(nameof(LatencyMonitor));
            Object.DontDestroyOnLoad(go);
            _runner = go.AddComponent<Runner>();
            _runner.StartCo(MonitorServer());
        }

        public void StopMonitoring() {
            _logManager.Log("[LatencyMonitor]: ------------------ Stop monitor ------------------");

            if (!_isMonitoring) {
                return;
            }
            _isMonitoring = false;
            if (_runner) {
                Object.Destroy(_runner.gameObject);
                _runner = null;
            }
        }

        private IEnumerator MonitorServer() {
            _lastPingTime = Time.time;
            _logManager.Log($"[LatencyMonitor]: ------------------ Start monitor : threshold = {_disconnectThreshold} ------------------");

            while (_isMonitoring) {
                yield return new WaitForSeconds(_pingInterval);

                // Calculate the time elapsed since the last successful ping
                var timeSinceLastPing = Time.time - _lastPingTime;

                if (timeSinceLastPing >= _disconnectThreshold) {
                    _logManager.Log($"[LatencyMonitor]: ------------------ {timeSinceLastPing} - {_disconnectThreshold} ------------------");
                    _logManager.Log("[LatencyMonitor]: ------------------ Server disconnected ------------------");
                    // Raise the ServerDisconnected event if the threshold is exceeded
                    _serverDisconnected?.Invoke();
                }
            }
        }

        public void NotifyPingSuccess() {
            // Update the last ping time when a ping is successful
            _lastPingTime = Time.time;
        }

        private class Runner : MonoBehaviour {
            public void StartCo(IEnumerator coroutine) {
                StartCoroutine(coroutine);
            }
        }
    }
}