using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Manager;

using Senspark;

using Share.Scripts.Communicate;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;

namespace Scenes.ConnectScene.Scripts {
    public class DialogRequestNewGuestAccount : Dialog {
        private IAuthManager _authManager;

        private Action<UserAccount> _resolver;
        private Action _reject;

        public static UniTask<DialogRequestNewGuestAccount> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogRequestNewGuestAccount>();
        }

        public DialogRequestNewGuestAccount Init(bool isProduction, ILogManager logManager, 
            IMasterUnityCommunication unityCommunication) {
            _authManager = new DefaultAuthManager(logManager, unityCommunication, isProduction);
            return this;
        }

        public DialogRequestNewGuestAccount StartFlow(Action<UserAccount> resolve, Action reject, Canvas canvas) {
            _resolver = resolve;
            _reject = reject;
            Process(canvas);
            return this;
        }

        private void OnCompleted(UserAccount acc) {
            _resolver?.Invoke(acc);
            Hide();
        }

        private void OnError() {
            _reject?.Invoke();
            Hide();
        }

        private void Process(Canvas canvas) {
            if (_authManager == null) {
                DialogOK.ShowErrorMsgOnly(canvas, "Auth service not started");
                return;
            }
            var waiting = new WaitingUiManager(canvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    await UniTask.SwitchToMainThread();
                    if (_authManager == null) {
                        throw new Exception("Auth service not started");
                    }
                    var data = await _authManager.RequestNewGuestAccountUsername();
                    if (string.IsNullOrWhiteSpace(data.UsernameOrWallet)) {
                        throw new Exception("Create new account failed");
                    }
                    var acc = new UserAccount {
                        userName = data.UsernameOrWallet,
                        id = data.UserId
                    };
                    OnCompleted(acc);
                } catch (Exception e) {
                    await DialogOK.ShowErrorAsync(canvas, e, new DialogOK.Optional { WaitUntilHidden = true });
                    OnError();
                } finally {
                    waiting.End();
                }
            });
        }
    }
}