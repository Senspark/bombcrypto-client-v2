using System.Collections.Generic;

using Analytics.Modules;

using App;

using Senspark;

using Share.Scripts.Communicate;

using UnityEngine;

namespace Scenes.ConnectScene.Scripts.Connectors.Mobile {
    public partial class MobileConnectController {
        private class StateData {
            public readonly IMasterUnityCommunication UnityCommunicate;
            public readonly ILogManager LogManager;
            public readonly IAnalyticsModuleLogin Analytics;
            public readonly IAuthManager AuthManager;
            public readonly IUserAccountManager UserAccountManager;
            public readonly ITaskDelay TaskDelay;
            public readonly UserAccount SavedAccount;
            public readonly List<ServerAddress.Info> ServerAddresses;
            public readonly Canvas CanvasDialog;
            public UserAccount NewAccount;

            public StateData(
                IMasterUnityCommunication unityCommunicate,
                ILogManager logManager,
                IAnalyticsModuleLogin analytics,
                IAuthManager authManager,
                IUserAccountManager userAccountManager,
                ITaskDelay taskDelay,
                List<ServerAddress.Info> serverAddresses,
                UserAccount savedAccount,
                Canvas canvasDialog
            ) {
                UnityCommunicate = unityCommunicate;
                LogManager = logManager;
                Analytics = analytics;
                AuthManager = authManager;
                UserAccountManager = userAccountManager;
                TaskDelay = taskDelay;
                ServerAddresses = serverAddresses;
                SavedAccount = savedAccount;
                CanvasDialog = canvasDialog;
            }
        }
    }
}