using System;
using System.Collections.Generic;

using App;

using Communicate;

using CustomSmartFox;

using Cysharp.Threading.Tasks;

using Game.Manager;

using Senspark;

using Game.UI;

using Scenes.ConnectScene.Scripts.Connectors;

using Services.WebGL;

using Share.Scripts.Communicate;

namespace Game.Dialog.Connects {
    public class AfSignInThirdPartyState {
        private readonly ILogManager _logManager;
        private readonly AccountFlowData _data;
        
        private UniTaskCompletionSource<UserAccount> _task;
        private WaitingUiManager _waiting;

        public AfSignInThirdPartyState(ILogManager logManager, AccountFlowData data) {
            _logManager = logManager;
            _data = data;
        }

        public async UniTask<StateReturnValue<UserAccount>> StartFlow(AccountFlowLoginType type, AccountFlowData data) {
            _task = new UniTaskCompletionSource<UserAccount>();
            try {
                var t = type switch {
                    AccountFlowLoginType.Apple => ThirdPartyLogin.Apple,
                    _ => throw new Exception($"Not support type: {type}")
                };
                ShowWaiting();
                LoginThirdParty(t, data);
                var usr = await _task.Task;
                return new StateReturnValue<UserAccount>(false, usr);
            } catch (Exception e) {
                // ignore
                return new StateReturnValue<UserAccount>(true, null);
            } finally {
                HideWaiting();
            }
        }

        private void LoginThirdParty(ThirdPartyLogin t, AccountFlowData data) {
            var sv = new List<ServerAddress.Info> { data.CurrentServer };
            void OnResolve(UserAccount usr) {
                _task.TrySetResult(usr);
            }
            void OnReject() {
                _task.TrySetCanceled();
            }
            new ConnectThirdPartyController(new NullEncoder(_logManager), new NullMasterUnity(), _logManager, new NullBridgeUtils(), sv, t, OnResolve, OnReject, data.CurrentCanvas)
                .ToLoginThirdParty();
        }
        
        private void ShowWaiting() {
            if (_waiting != null) {
                return;
            }
            _waiting = new WaitingUiManager(_data.CurrentCanvas);
            _waiting.Begin();
        }

        private void HideWaiting() {
            if (_waiting == null) {
                return;
            }
            _waiting.End();
            _waiting = null;
        }
    }
}