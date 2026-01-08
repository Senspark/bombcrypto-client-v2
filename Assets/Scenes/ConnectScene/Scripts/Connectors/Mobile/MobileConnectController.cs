using System.Threading.Tasks;

using Analytics.Modules;

using App;

using Senspark;

using Share.Scripts.Communicate;

using UnityEngine;

namespace Scenes.ConnectScene.Scripts.Connectors.Mobile {
    public partial class MobileConnectController : IConnectController {
        private readonly StateData _stateData;

        public MobileConnectController(
            IMasterUnityCommunication unityCommunicate,
            bool isProduction,
            ILogManager logManager,
            IAnalyticsModuleLogin analytics,
            IAuthManager authManager,
            ITaskDelay taskDelay,
            IUserAccountManager userAccountManager,
            Canvas canvasDialog
        ) {
            var savedAccount = userAccountManager.GetRememberedAccount();
            var svAddress = isProduction
                ? ServerAddress.ProdServerAddresses
                : ServerAddress.TestServerAddresses;
            _stateData = new StateData(
                unityCommunicate,
                logManager,
                analytics,
                authManager,
                userAccountManager,
                taskDelay,
                svAddress,
                savedAccount,
                canvasDialog
            );
        }

        public async Task<UserAccount> StartFlow() {
            IState state = new CheckRememberAccountState(_stateData);
            while (state is not ConnectCompletionState) {
                state = await state.GetNextState();
            }
            return _stateData.NewAccount;
        }
    }
}