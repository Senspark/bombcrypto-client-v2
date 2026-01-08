using System.Threading.Tasks;

using Analytics.Modules;

using App;

using CustomSmartFox;

using Game.UI;

using Scenes.ConnectScene.Scripts.Connectors;
using Scenes.ConnectScene.Scripts.Connectors.Mobile;

using Senspark;

using Services.WebGL;

using Share.Scripts;
using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

using UnityEngine;

namespace Game.ConnectControl {
    public class DefaultConnectController : IConnectController {
        private readonly IConnectController _bridge;

        public DefaultConnectController(
            IExtResponseEncoder encoder,
            IMasterUnityCommunication unityCommunicate,
            IUserAccountManager userAccountManager,
            ILogManager logManager,
            IWebGLBridgeUtils webGLBridgeUtils,
            IAnalyticsModuleLogin analytics,
            Canvas canvasDialog,
            bool isProduction,
            bool allowLogin
        ) {
            var authManager = new DefaultAuthManager(logManager, new NullSignManager(), encoder, unityCommunicate, isProduction);
            var isWeb = false;
#if UNITY_WEBGL
            isWeb = true;
#endif
            ITaskDelay taskDelay = WebGLTaskDelay.Instance;
            if (AppConfig.IsTon()) {
                _bridge = new TelegramConnectController(unityCommunicate, userAccountManager, logManager, webGLBridgeUtils, analytics, taskDelay, canvasDialog,
                    isProduction);
            } else if(AppConfig.IsSolana()) {
                _bridge = new SolanaConnectController(encoder, unityCommunicate, userAccountManager, logManager, webGLBridgeUtils, analytics, taskDelay, canvasDialog,
                    isProduction);
            }
            else if (allowLogin || isWeb) {
                _bridge = new WebConnectController(unityCommunicate, userAccountManager, logManager, analytics, taskDelay, canvasDialog,
                    isProduction);
            } else {
                _bridge = new MobileConnectController(unityCommunicate, isProduction, logManager, analytics, authManager,
                    taskDelay,
                    userAccountManager, canvasDialog);
            }
        }

        public Task<UserAccount> StartFlow() {
            return _bridge.StartFlow();
        }
    }
}