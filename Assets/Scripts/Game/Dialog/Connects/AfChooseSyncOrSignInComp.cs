using System;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using Scenes.ConnectScene.Scripts;
using Scenes.MainMenuScene.Scripts;

using Services.DeepLink;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Game.Dialog.Connects {
    public class AfChooseSyncOrSignInComp : MonoBehaviour {
        [SerializeField]
        private Button btnSyncGuestAcc;

        [SerializeField]
        private Button btnSignInOldAcc;
        
        [SerializeField]
        private Button btnDeleteAcc;

        [SerializeField]
        private AccountFlowData accountFlowData;
        
        private Canvas _canvasDialog;
        private Action _onReload;

        private ILogManager _logManager;
        private IServerManager _serverManager;
        private ISoundManager _soundManager;
        private IUserAccountManager _userAccountManager;
        private IDeepLinkListener _deepLinkListener;
        private IAccountFlowController _accountFlowController;

        private void Awake() {
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            _deepLinkListener = ServiceLocator.Instance.Resolve<IDeepLinkListener>();

            var isIOs = false;
#if UNITY_IOS
            isIOs = true;
#endif
            btnDeleteAcc.gameObject.SetActive(isIOs);
        }

        private void OnDestroy() {
            _accountFlowController?.Destroy();
        }

        public void Init(Canvas canvasDialog, Action onReload) {
            _canvasDialog = canvasDialog;
            _onReload = onReload;
            
            btnSyncGuestAcc.onClick.AddListener(OnSyncGuestAcc);
            btnSignInOldAcc.onClick.AddListener(OnSignInOldAcc);
            btnDeleteAcc.onClick.AddListener(OnDeleteAcc);
        }
        
        private void OnSyncGuestAcc() {
            _soundManager.PlaySound(Audio.Tap);
            _accountFlowController?.Destroy();
            _accountFlowController = NewFlow();
            UniTask.Void(async () => {
                var completed = await _accountFlowController.SyncGuest();
                if (completed) {
                    // show new user profile
                    //_onReload();
                    //Login again
                    App.Utils.KickToConnectScene();
                }
            });
        }

        private void OnSignInOldAcc() {
            _soundManager.PlaySound(Audio.Tap);
            _accountFlowController?.Destroy();
            _accountFlowController = NewFlow();
            UniTask.Void(async () => {
                var completed = await _accountFlowController.LoginOldAccount();
                if (completed) {
                    // reload scene
                    _serverManager.Disconnect();
                     SceneManager.LoadScene(nameof(ConnectScene));
                }
            });
        }

        private IAccountFlowController NewFlow() {
            return new AccountFlowController(
                _logManager,
                _serverManager,
                _userAccountManager,
                _deepLinkListener,
                GetServerAddress(),
                accountFlowData,
                _canvasDialog
            );
        }

        private void OnDeleteAcc() {
            _soundManager.PlaySound(Audio.Tap);
            DialogDeleteAccount.Create().ContinueWith(dialog => {
                dialog.Show(_canvasDialog);
            });
        }

        private ServerAddress.Info GetServerAddress() {
            var usr = _userAccountManager.GetRememberedAccount();
            Assert.IsNotNull(usr);
            var info = ServerAddress.GetServerInfo(usr.server.Address);
            Assert.IsNotNull(info);
            return info;
        }
    }
}