using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Dialog.AIO;
using Game.Manager;

using Senspark;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;

namespace Scenes.MainMenuScene.Scripts {
    public class DialogDeleteAccount : Dialog {
        [SerializeField]
        private TextMeshProUGUI processTxt;

        [SerializeField]
        private HoldButton deleteBtn;

        [SerializeField]
        private float holdSeconds = 3;

        private IUserAccountManager _userAccountManager;
        private IServerManager _serverManager;
        
        public static UniTask<DialogDeleteAccount> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogDeleteAccount>();
        }
        
        private void Start() {
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            deleteBtn.Init(OnDeleteBtnHold);
            ResetUI();
        }

        private void OnDeleteBtnHold(float timePassed) {
            if (timePassed == 0) {
                processTxt.text = null;
                return;
            }
            if (timePassed < holdSeconds) {
                var t = holdSeconds - timePassed;
                processTxt.text = $"{t:N0}";
                return;
            }
            processTxt.text = "...";
            deleteBtn.SetEnable(false);
            
            UniTask.Void(async () => {
                var waiting = new WaitingUiManager(DialogCanvas);
                waiting.Begin();
                var isGuest = false;
                try {
                    var acc = _userAccountManager.GetRememberedAccount();
                    isGuest = acc.loginType == LoginType.Guest;
                    //await _serverManager.BomberLand.DeleteUser(acc.thirdPartyAccessToken);
                    App.Utils.KickToConnectScene();
                } catch (Exception e) {
                    if (e.Message == "kick") {
                        App.Utils.KickToConnectScene();
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message);
                        ResetUI();
                    }
                } finally {
                    _userAccountManager.EraseData();
                    if (isGuest) {
                        _userAccountManager.EraseGuest();
                    }
                    waiting.End();
                }
            });
        }

        private void ResetUI() {
            deleteBtn.SetEnable(true);
            processTxt.text = null;
        }
    }
}