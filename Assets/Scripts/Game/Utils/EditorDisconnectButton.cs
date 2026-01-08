using App;

using BLPvpMode.Engine.User;

using Senspark;

using UnityEngine;

namespace Utils {
    public class EditorDisconnectButton : MonoBehaviour {
#if UNITY_EDITOR
        public static EditorDisconnectButton Instance;
        private IUser _pvpServer;

        private void Awake() {
            if (Instance) {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);
        }

        public void SetPvpServer(IUser pvpServer) {
            _pvpServer = pvpServer;
        }

        [Button]
        public void KillMain() {
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            serverManager.TestKillServer();
        }

        [Button]
        public void DisconnectMain() {
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            serverManager.Disconnect();
        }

        [Button]
        public void DisconnectPvp() {
            _pvpServer.Disconnect();
        }

        [Button]
        public void DisconnectAll() {
            DisconnectMain();
            DisconnectPvp();
        }
#endif
    }
}