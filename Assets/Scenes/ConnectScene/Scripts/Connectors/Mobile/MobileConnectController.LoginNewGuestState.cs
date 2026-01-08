using App;

using Cysharp.Threading.Tasks;

namespace Scenes.ConnectScene.Scripts.Connectors.Mobile {
    public partial class MobileConnectController {
        private class LoginNewGuestState : IState {
            private readonly StateData _stateData;
            private readonly UniTaskCompletionSource<IState> _completion;

            public LoginNewGuestState(StateData stateData) {
                _stateData = stateData;
                var controller = new ConnectGuestController(
                    _stateData.UnityCommunicate,
                    _stateData.LogManager,
                    _stateData.UserAccountManager,
                    _stateData.Analytics,
                    _stateData.ServerAddresses,
                    OnResolve,
                    OnReject,
                    _stateData.CanvasDialog
                );
                _completion = new UniTaskCompletionSource<IState>();
                controller.ToLoginGuest();
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