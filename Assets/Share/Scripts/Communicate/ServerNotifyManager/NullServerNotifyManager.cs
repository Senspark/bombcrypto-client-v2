using System;
using System.Threading.Tasks;

using BLPvpMode.Manager.Api;

using Cysharp.Threading.Tasks;

using Sfs2X.Entities.Data;

namespace Share.Scripts.Communicate {
    public class NullServerNotifyManager: AutoImplementServerMethod, IServerNotifyManager {
        public void OnExtensionResponse(string cmd, ISFSObject value) {}

        public void OnExtensionResponse(string cmd, int requestId, byte[] data) {}

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {}

        public int AddObserver(ServerNotifyObserver observer) {
            return 0;
        }

        public bool RemoveObserver(int id) {
            return true;
        }

        public void DispatchEvent(Action<ServerNotifyObserver> dispatcher) {}
        public UniTask<T> WaitForEvent<T>(ServerNotifyEvent eventType){
            return UniTask.FromResult<T>(default);
        }

        protected override IServerListener Instance() {
            return null;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}