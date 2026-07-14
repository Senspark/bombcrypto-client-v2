using System.Threading.Tasks;

using App;

using Scenes.ConnectScene.Scripts.Connectors;
using Scenes.ConnectScene.Scripts.Connectors.Mobile;

using Senspark;

using Services.WebGL;

using Share.Scripts.Communicate;

using UnityEngine;

namespace Game.ConnectControl {
    public class DefaultConnectController : IConnectController {
        private readonly IConnectController _bridge;

        public DefaultConnectController(
            IMasterUnityCommunication unityCommunicate,
            IUserAccountManager userAccountManager,
            ILogManager logManager,
            IWebGLBridgeUtils webGLBridgeUtils,
            Canvas canvasDialog,
            bool isProduction,
            bool allowLogin
        ) {
            var authManager = new DefaultAuthManager(logManager, unityCommunicate, isProduction);
            var isWeb = false;
#if UNITY_WEBGL
            isWeb = true;
#endif
            ITaskDelay taskDelay = WebGLTaskDelay.Instance;
            if (AppConfig.IsTon()) {
                _bridge = new TelegramConnectController(unityCommunicate, userAccountManager, logManager, webGLBridgeUtils, taskDelay, canvasDialog,
                    isProduction);
            } else if(AppConfig.IsSolana()) {
                _bridge = new SolanaConnectController(unityCommunicate, userAccountManager, logManager, webGLBridgeUtils, taskDelay, canvasDialog,
                    isProduction);
            }
            else if (allowLogin || isWeb) {
                _bridge = new WebConnectController(unityCommunicate, userAccountManager, logManager, taskDelay, canvasDialog,
                    isProduction);
            } else {
                _bridge = new MobileConnectController(unityCommunicate, isProduction, logManager, authManager,
                    taskDelay, userAccountManager, canvasDialog);
            }
        }

        public Task<UserAccount> StartFlow() {
            return _bridge.StartFlow();
        }
    }
}