using System;
using System.Threading.Tasks;

using App;

using Castle.Core.Internal;

using CustomSmartFox;

using JetBrains.Annotations;

using Newtonsoft.Json;

using Scenes.ConnectScene.Scripts.Connectors;

using Senspark;

using Services.Server;

using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;
using Share.Scripts.Dialog;

namespace Reconnect.Backend {
    public class MainReconnectBackend : IReconnectBackend {
        [NotNull]
        private readonly IServerManager _serverManager;

        private readonly ObserverHandle _handle;

        private TaskCompletionSource<object> _connectionLostTcs;

        private bool _disposed;

        public MainReconnectBackend() {
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnServerStateChanged = state => {
                    if (state == ServerConnectionState.LostConnection) {
                        _connectionLostTcs?.SetResult(null);
                    }
                },
            });
        }

        ~MainReconnectBackend() => Dispose(false);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }
            if (disposing) {
                _handle.Dispose();
                _connectionLostTcs?.SetException(new ObjectDisposedException(nameof(_connectionLostTcs)));
            }
            _disposed = true;
        }

        public async Task WaitForConnectionLost() {
            if (!IsConnected()) {
                return;
            }
            if (_connectionLostTcs != null) {
                await _connectionLostTcs.Task;
                return;
            }
            try {
                _connectionLostTcs = new TaskCompletionSource<object>();
                await _connectionLostTcs.Task;
            } finally {
                _connectionLostTcs = null;
            }
        }

        public async Task Reconnect() {
            await _serverManager.Connect(30f);
            if (_disposed) {
                throw new ObjectDisposedException(nameof(MainReconnectBackend));
            }
            //await _serverManager.ReLogin();
            var account = ServiceLocator.Instance.Resolve<IUserAccountManager>().GetRememberedAccount();
            var unityCommunicate = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
            // lấy lại jwt mới rồi mới login smartfox
            await unityCommunicate.Handshake(HandshakeType.Reconnect);
            
            if(account == null)
                return;
            
            var jwt = unityCommunicate.SmartFox.GetJwtForLogin();
 
            
            if(jwt.IsNullOrEmpty()) {
                throw new Exception("Failed to reconnect");
            }
            
            account.jwtToken = jwt;
            ILoginData loginData = GameReadyController.GetLoginData(account);
            await _serverManager.ReLogin(loginData);
            if (_disposed) {
                throw new ObjectDisposedException(nameof(MainReconnectBackend));
            }
        }

        private bool IsConnected() {
            return _serverManager.CurrentState == ServerConnectionState.LoggedIn;
        }
    }
}
