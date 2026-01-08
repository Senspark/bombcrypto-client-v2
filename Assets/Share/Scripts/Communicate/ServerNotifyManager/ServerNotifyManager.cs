using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using UnityEngine;

namespace Share.Scripts.Communicate {
    public class ServerNotifyObserver {
        public Action<bool> OnAdsVerified;
        public Action OnClientSendLog;
        public Action<bool> OnDepositComplete;
        public Action<ISFSObject> OnDepositResponse;
    }
    public enum ServerNotifyEvent {
        AdsVerified,
        ClientSendLog,
    }
    public class ServerNotifyManager: ObserverManager<ServerNotifyObserver>, IServerNotifyManager {
        private readonly ILogManager _logManager;
        private readonly Dictionary<ServerNotifyEvent, UniTaskCompletionSource<object>> _event = new();

        public ServerNotifyManager(ILogManager logManager) {
            _logManager = logManager;
        }
        public Task<bool> Initialize() {
            foreach (ServerNotifyEvent eventType in Enum.GetValues(typeof(ServerNotifyEvent))) {
                _event[eventType] = new UniTaskCompletionSource<object>();
            }
            _logManager.Log("[ServerNotifyManager]: Initialized and ready to listen for events.");
            return Task.FromResult(true);
        }
        
        public async UniTask<T> WaitForEvent<T>(ServerNotifyEvent eventType) {
            try {
                if (_event.TryGetValue(eventType, out var tcs)) {
                    return (T)await tcs.Task;
                }
                throw new ArgumentException($"Event {eventType} is not registered.");
            } catch (Exception e) {
                Debug.LogError($"Error waiting for event {eventType}: {e.Message}");
                return default;
            } finally {
                 _event[eventType] = new UniTaskCompletionSource<object>();
            }
        }

        public void OnExtensionResponse(string cmd, ISFSObject value) {
            _logManager.Log($"[ServerNotifyManager]: {cmd}");
            
            if (cmd == SFSDefine.SFSCommand.VERIFY_ADS_RESPONSE) {
                var result = value.GetBool("result");
                DispatchEvent(e => e.OnAdsVerified?.Invoke(result));
                if (_event.TryGetValue(ServerNotifyEvent.AdsVerified, out var tcs)) {
                    tcs.TrySetResult(result);
                }
                return;
            }

            if (cmd == SFSDefine.SFSCommand.FORCE_CLIENT_SEND_LOG) {
                DispatchEvent(e => e.OnClientSendLog?.Invoke());
                if (_event.TryGetValue(ServerNotifyEvent.ClientSendLog, out var tcs)) {
                    tcs.TrySetResult(null);
                }
                return;
            }
            
            if (cmd == SFSDefine.SFSCommand.DEPOSIT_RON_RESPONSE ||
                cmd == SFSDefine.SFSCommand.DEPOSIT_BAS_RESPONSE ||
                cmd == SFSDefine.SFSCommand.DEPOSIT_VIC_RESPONSE) {
                try {
                    _logManager.Log($"RECEIVED: cmd({cmd}) value({value})");
                    DispatchEvent(e => e.OnDepositResponse?.Invoke(value));
                    DispatchEvent(e => e.OnDepositComplete?.Invoke(true));
                } catch (Exception e) {
                    DispatchEvent(e => e.OnDepositComplete?.Invoke(false));
                }
                return;
            }

            // SYNC_BOMBERMAN_V3
            if (cmd == SFSDefine.SFSCommand.SYNC_HERO_RESPONSE) {
                try {
                    _logManager.Log($"RECEIVED: cmd({cmd}) value({value})");
                    var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
                    serverManager.General.SyncHero(value);
                } catch (Exception e) {
                    _logManager.Log($"RECEIVED FAILED: cmd({cmd}) value({value})");
                }
                return;
            }
            
            // SYNC_HOUSE_V3
            if (cmd == SFSDefine.SFSCommand.SYNC_HOUSE_RESPONSE) {
                try {
                    _logManager.Log($"RECEIVED: cmd({cmd}) value({value})");
                    var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
                    serverManager.General.SyncHouse(value);
                } catch (Exception e) {
                    _logManager.Log($"RECEIVED FAILED: cmd({cmd}) value({value})");
                }
                return;
            }
            
            // SYNC_DEPOSITED_V3
            if (cmd == SFSDefine.SFSCommand.SYNC_DEPOSIT_RESPONSE) {
                try {
                    _logManager.Log($"RECEIVED: cmd({cmd}) value({value})");
                    var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
                    serverManager.General.SyncDeposited(value);
                } catch (Exception e) {
                    _logManager.Log($"RECEIVED FAILED: cmd({cmd}) value({value})");
                }
                return;
            }
        }

        public void OnExtensionResponse(string cmd, int requestId, byte[] data) {}

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
            _logManager.Log($"[ServerNotifyManager]: {cmd} ERROR({errorCode}) {errorMessage}");
            
            if (cmd == SFSDefine.SFSCommand.VERIFY_ADS_RESPONSE) {
                DispatchEvent(e => e.OnAdsVerified?.Invoke(false));
                if (_event.TryGetValue(ServerNotifyEvent.AdsVerified, out var tcs)) {
                    tcs.TrySetException(new Exception());
                }
                return;
            }
        }
        
        
        public void Destroy() {}
        public void OnConnection() {}
        public void OnConnectionError(string message) {}
        public void OnConnectionRetry() {}
        public void OnConnectionResume() {}
        public void OnConnectionLost(string reason) {}
        public void OnLogin() {}
        public void OnLoginError(int code, string message) {}
        public void OnUdpInit(bool success) {}
        public void OnPingPong(int lagValue) {}
        public void OnRoomVariableUpdate(SFSRoom room) {}
        public void OnJoinRoom(SFSRoom room) {}
    }
}
