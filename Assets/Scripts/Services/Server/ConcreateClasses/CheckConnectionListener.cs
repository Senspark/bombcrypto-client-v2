using System;

using BLPvpMode.Manager.Api;

using Cysharp.Threading.Tasks;

using Senspark;

using JetBrains.Annotations;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;

using UnityEngine;

namespace App {
    public class CheckConnectionListener : IServerListener {
        private readonly ILogManager _logManager;
        private readonly Action<ServerConnectionState> _onServerStateChanged;
        private readonly Action _manualDisconnect;

        private UniTaskCompletionSource<bool> _reconnectTask;
        private readonly LatencyMonitor _latencyMonitor;

        private const float PingInterval = 4f;
        private const float DisconnectThreshold = 6f;

        public CheckConnectionListener(
            [NotNull] ILogManager logManager,
            [NotNull] Action manualDisconnect,
            [NotNull] Action<ServerConnectionState> onServerStateChanged
        ) {
            _logManager = logManager;
            _manualDisconnect = manualDisconnect;
            _onServerStateChanged = onServerStateChanged;
            _latencyMonitor = new LatencyMonitor(PingInterval, DisconnectThreshold, LatencyMonitorOnServerDisconnected, logManager);
        }

        private void LatencyMonitorOnServerDisconnected() {
            _manualDisconnect();
            StopLatencyMonitor();
        }

        private void StopLatencyMonitor() {
            _latencyMonitor.StopMonitoring();
        }

        public void OnConnection() {
        }

        public void OnConnectionError(string message) {
            _logManager.Log(message);
            _onServerStateChanged(ServerConnectionState.LostConnection);
        }

        public void OnConnectionRetry() {
            _logManager.Log();
            _onServerStateChanged(ServerConnectionState.LostConnection);
        }

        public void OnConnectionResume() {
            _logManager.Log();
            _onServerStateChanged(ServerConnectionState.LoggedIn);
        }

        public void OnConnectionLost(string reason) {
            _logManager.Log();
            if (AppConfig.IsMobile()) {
                _onServerStateChanged(ServerConnectionState.LostConnection);
                StopLatencyMonitor();
                return;
            }
            _onServerStateChanged(reason == "kick"
                ? ServerConnectionState.KickOut
                : ServerConnectionState.LostConnection);
            StopLatencyMonitor();
        }

        public void OnLogin() {
            _logManager.Log();
            _onServerStateChanged(ServerConnectionState.LoggedIn);
            _latencyMonitor.StartMonitoring();
        }

        public void OnLoginError(int code, string message) {
            _logManager.Log($"Code: {code}, Message: {message}");
            if (code == ErrorCode.KICK_BY_OTHER_DEVICE) {
                _onServerStateChanged(ServerConnectionState.KickOut);
            } else {
                _onServerStateChanged(ServerConnectionState.LostConnection);
            }
            StopLatencyMonitor();
        }

        public void OnUdpInit(bool success) {
        }

        public void OnPingPong(int lagValue) {
            _latencyMonitor.NotifyPingSuccess();
        }

        public void OnRoomVariableUpdate(SFSRoom room) {
        }

        public void OnJoinRoom(SFSRoom room) {
        }

        public void OnExtensionResponse(string cmd, ISFSObject value) {
            _latencyMonitor.NotifyPingSuccess();
        }

        

        public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
        }

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
        }
    }
}