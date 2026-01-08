using App;

using Cysharp.Threading.Tasks;

namespace Scenes.ConnectScene.Scripts.Connectors.Mobile {
    public partial class MobileConnectController {
        private class CheckRememberAccountState : IState {
            private readonly StateData _stateData;
            private readonly UniTaskCompletionSource<IState> _completion;

            public CheckRememberAccountState(StateData data) {
                _stateData = data;
                var controller = new ConnectContinueLoginController(data.LogManager, data.AuthManager, data.SavedAccount,
                    OnResolve, OnReject);
                _completion = new UniTaskCompletionSource<IState>();
                controller.ToCheckRememberAccount();
            }

            public async UniTask<IState> GetNextState() {
                return await _completion.Task;
            }

            private void OnResolve(UserAccount newAcc) {
                _stateData.NewAccount = newAcc;
                _completion.TrySetResult(new ConnectCompletionState());
            }

            private void OnReject() {
                _completion.TrySetResult(new LoginNewGuestState(_stateData));
            }
        }
    }
}