using System;

using App;

using Communicate;

using CustomSmartFox;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Manager;

using Senspark;

using Share.Scripts.Communicate;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

namespace Scenes.ConnectScene.Scripts {
    public class DialogConnectWallet : Dialog {
        private ILogManager _logManager;
        private ISoundManager _soundManager;
        private IAuthManager _authManager;
        private IMasterUnityCommunication _unityCommunication;

        private WalletType _chosen;
        private bool _mustSign;
        private bool _processing;
        private int _networkChainId;
        private Action<UserAccount> _resolve;
        private Action _reject;

        public static UniTask<DialogConnectWallet> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogConnectWallet>();
        }

        protected override void Awake() {
            base.Awake();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
        }

        public DialogConnectWallet Init(bool sign, bool isProduction, int networkChainId) {
            _mustSign = sign;
            _networkChainId = networkChainId;
            var signManager = new NewSignManager(_logManager);
            _authManager = new DefaultAuthManager(_logManager, signManager, new NullEncoder(_logManager),
                _unityCommunication, isProduction);
            return this;
        }

        public DialogConnectWallet StartFlow(Action<UserAccount> resolve, Action reject) {
            _resolve = resolve;
            _reject = reject;
            OnDidHide(() => _reject?.Invoke());
            return this;
        }

        public void OnMetamaskBtnClicked() {
            WalletCallback(WalletType.Metamask);
        }

        public void OnCoinbaseBtnClicked() {
            WalletCallback(WalletType.Coinbase);
        }

        public void OnTrustWalletBtnClicked() {
            WalletCallback(WalletType.TrustWallet);
        }

        public void OnOperaWalletBtnClicked() {
            WalletCallback(WalletType.OperaWallet);
        }

        private void WalletCallback(WalletType walletType) {
            _soundManager.PlaySound(Audio.Tap);
            _chosen = walletType;
            if (_mustSign) {
#if UNITY_EDITOR
                FakeSign();
                return;
#endif
                Sign();
            } else {
                var acc = new UserAccount {
                    walletType = _chosen
                };
                OnCompleted(acc);
            }
        }

        private void OnCompleted(UserAccount acc) {
            _resolve?.Invoke(acc);
            Clear();
            Hide();
        }

        private void Clear() {
            _resolve = null;
            _reject = null;
        }

        private void Sign() {
            if (_processing) {
                return;
            }
            if (_authManager == null) {
                DialogOK.ShowError(DialogCanvas, "Auth service not started");
                return;
            }
            _processing = true;
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    await WebGLBlockchainInitializer.InitNetworkConfig(_chosen);
                    var res = await _authManager.GetUserLoginDataBySign(_networkChainId);
                    var acc = new UserAccount {
                        userName = res.UsernameOrWallet,
                        id = res.UserId,
                        jwtToken = res.JwtToken,
                        walletAddress = res.IsUserFi ? res.UsernameOrWallet : null,
                        isUserFi = res.IsUserFi,
                        walletType = _chosen,
                        hasPasscode = res.HasPasscode
                    };
                    OnCompleted(acc);
                } catch (Exception e) {
                    DialogOK.ShowError(DialogCanvas, e.Message);
                } finally {
                    _processing = false;
                    waiting.End();
                }
            });
        }

        private void FakeSign() {
#if UNITY_EDITOR
            var walletInfo = AppConfig.GetTestWalletInfo();
            var acc = new UserAccount {
                userName = walletInfo.address,
                id = 1,
                jwtToken = walletInfo.jwt,
                walletAddress = walletInfo.address,
                isUserFi = true,
                walletType = _chosen,
                hasPasscode = false,
            };
            OnCompleted(acc);
#endif
        }
    }
}