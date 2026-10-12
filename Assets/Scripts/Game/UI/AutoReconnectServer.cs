using System;
using System.Threading.Tasks;

using App;

using Senspark;

using Reconnect;
using Reconnect.Backend;

using Share.Scripts.Communicate;
using Share.Scripts.Dialog;

using UnityEngine;

namespace Game.UI {
    public class AutoReconnectServer : MonoBehaviour {
        [SerializeField]
        private LevelScene levelScene;

        [SerializeField]
        private Canvas canvasDialog;

        private ObserverHandle _handle;
        private IServerManager _serverManager;
        private IStorageManager _storageManager;
        private IUserAccountManager _userAccountManager;
        private IReconnectStrategy _reconnectStrategy;
        private ILogManager _logManager;
        private IReconnectBackend _backend;
        private IReconnectView _reconnectView;
        private IMasterUnityCommunication _unityCommunication;

        private bool _isReconnecting;
        private const int MaxRetryCount = 10;

        private bool _isBeingKicked;
        private static bool _freeReconnectUsed;
        private bool _isWaitingManual;

        private void Awake() {
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
            _backend = new MainReconnectBackend();
            
            _reconnectView = new ReconnectThModeView(canvasDialog);

            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnServerStateChanged = OnServerStateChanged,
            });
        }

        private void OnDisable() {
            _handle.Dispose();
        }

        private void OnServerStateChanged(ServerConnectionState state) {
            switch (state) {
                case ServerConnectionState.LostConnection:
                    ReconnectBegin();
                    break;
                case ServerConnectionState.LoggedOut:
                    break;
                case ServerConnectionState.LoggedIn:
                    ReconnectDone();
                    break;
                case ServerConnectionState.KickOut:
                    if (AppConfig.IsWebAirdrop()) {
                        KickByOtherDevice();
                    } else {
                        Kick();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void ReconnectDone() {
            if (_isBeingKicked)
                return;
            _isReconnecting = false;
            _reconnectView.FinishReconnection(true);
        }

        private async void ReconnectBegin() {
            if (_isReconnecting || _isBeingKicked)
                return;

            _isReconnecting = true;
            try {
                var reconnect = _storageManager.EnableAutoMine;
                var acc = _userAccountManager.GetRememberedAccount();
                if (acc == null || !acc.isUserFi) {
                    reconnect = false;
                }
                // cho auto reconnect trên các network Airdrop
                if (AppConfig.IsWebAirdrop()) {
                    reconnect = true;
                }
                #if UNITY_EDITOR
                reconnect = !UnityEditor.EditorPrefs.GetBool("SimulateNoAutoMine");
                #endif

                if (!reconnect && !_freeReconnectUsed) {
                    // Lần đầu mất kết nối trong session: cho reconnect miễn phí
                    _freeReconnectUsed = true;
                    reconnect = true;
                }

                if (reconnect) {
                    await Reconnect();
                } else {
                    _isWaitingManual = true;
                    ShowManualReconnect();
                }
            } catch (Exception e) {
                _logManager.Log($"Reconnect fail {e}");
            } finally {
                if (!_isWaitingManual) {
                    _isReconnecting = false;
                }
            }
        }

        public async Task Reconnect() {
            while (true) {
                _logManager.Log("[ReconnectStrategy] WaitForConnectionLost");
                try {
                    await _backend.WaitForConnectionLost();
                } catch (Exception ex) {
                    _logManager.Log($"[ReconnectStrategy] ex={ex.Message}");
                    Debug.LogException(ex);
                    return;
                }
                var successful = false;
                try {
                    if (_isBeingKicked) {
                        return;
                    }
                    _logManager.Log("[ReconnectStrategy] StartReconnection");
                    await _reconnectView.StartReconnection();

                    for (var i = 0; i < MaxRetryCount; ++i) {
                        try {
                            if (_isBeingKicked) {
                                return;
                            }
                            _logManager.Log($"[ReconnectStrategy] Reconnect attempt={i + 1}");
                            _reconnectView.UpdateProgress(i);
                            await _backend.Reconnect();
                            _logManager.Log($"[ReconnectStrategy] Reconnect server complete");

                            successful = true;
                            break;
                        } catch (ObjectDisposedException ex) {
                            _logManager.Log($"[ReconnectStrategy] ex={ex.Message}");
                            return;
                        } catch (Exception ex) {
                            _logManager.Log($"[ReconnectStrategy] ex={ex.Message}");
                            Debug.LogException(ex);
                        }
                        await WebGLTaskDelay.Instance.Delay(3000);
                    }
                } finally {
                    _logManager.Log($"[ReconnectStrategy] FinishReconnection result = {successful}");
                    if (!_isBeingKicked) {
                        await _reconnectView.FinishReconnection(successful);
                        _isReconnecting = false;
                    }
                }

                break;
            }
        }

        private async void KickByOtherDevice() {
            _isBeingKicked = true;
            _reconnectView.KickByOtherDevice();
            var dialog = await DialogOK.Create();
            dialog.SetInfo("Already logged in",
                "Your account is currently logged in on another device.\n You will be disconnected.");
            dialog.OnWillHide(App.Utils.Logout);
            dialog.Show(canvasDialog);
        }

        private async void ShowManualReconnect() {
            try {
                var dialog = await DialogOK.Create();
                dialog.SetInfo("Connection lost",
                    "Please check your internet and tap OK to reconnect.");
                dialog.BlockOutsideClick();
                dialog.OnDidHide(OnManualReconnectClicked);
                dialog.Show(canvasDialog);
            } catch (Exception e) {
                _logManager.Log($"Show manual reconnect fail {e}");
                _isWaitingManual = false;
                _isReconnecting = false;
            }
        }

        private async void OnManualReconnectClicked() {
            try {
                await Reconnect();
            } catch (Exception e) {
                _logManager.Log($"Manual reconnect fail {e}");
            } finally {
                _isWaitingManual = false;
                _isReconnecting = false;
            }
        }

        private void Kick() {
            _isBeingKicked = true;
            var reason = "The account is having a data conflict with server.\n Try login again";
            DialogOK.ShowErrorMsgOnly(canvasDialog, reason, App.Utils.KickToConnectScene);
        }
    }
}