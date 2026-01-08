using System;
using System.Threading.Tasks;
using App;
using BLPvpMode.Manager.Api;
using Cysharp.Threading.Tasks;
using Senspark;
using Sfs2X.Entities.Data;

namespace Share.Scripts.Communicate {
    public class DefaultServerNotifyManager :AutoImplementServerMethod, IServerNotifyManager {
        private readonly IServerNotifyManager _serverNotifyManager;

        public DefaultServerNotifyManager(ILogManager logManager) {
            //DevHoang: Open for all platforms
            // if (AppConfig.IsBscOrPolygon() || AppConfig.IsWebAirdrop()) {
            if (true) {
                _serverNotifyManager = new ServerNotifyManager(logManager);
            } else {
                _serverNotifyManager = new NullServerNotifyManager();
            }
        }
        
        public void OnExtensionResponse(string cmd, ISFSObject value) {
            _serverNotifyManager.OnExtensionResponse(cmd, value);
        }

        public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
            _serverNotifyManager.OnExtensionResponse(cmd, requestId, data);
        }

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
            _serverNotifyManager.OnExtensionError(cmd, requestId, errorCode, errorMessage);
        }

        public int AddObserver(ServerNotifyObserver observer) {
            return _serverNotifyManager.AddObserver(observer);
        }

        public bool RemoveObserver(int id) {
            return _serverNotifyManager.RemoveObserver(id);
        }

        public void DispatchEvent(Action<ServerNotifyObserver> dispatcher) {
            _serverNotifyManager.DispatchEvent(dispatcher);
        }

        public UniTask<T> WaitForEvent<T>(ServerNotifyEvent eventType) {
            return _serverNotifyManager.WaitForEvent<T>(eventType);
        }

        protected override IServerListener Instance() {
            return _serverNotifyManager;
        }

        public Task<bool> Initialize() {
            return _serverNotifyManager.Initialize();
        }

        public void Destroy() {
            _serverNotifyManager.Destroy();
        }
    }
}