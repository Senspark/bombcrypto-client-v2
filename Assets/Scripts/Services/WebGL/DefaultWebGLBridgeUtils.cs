using System.Threading.Tasks;

using Senspark;

using Share.Scripts.Communicate;

namespace Services.WebGL {
    public class DefaultWebGLBridgeUtils : IWebGLBridgeUtils {
        private readonly IWebGLBridgeUtils _webGLBridgeUtils;
        private readonly IMasterUnityCommunication _unityCommunication;

        public DefaultWebGLBridgeUtils(ILogManager logManager, IMasterUnityCommunication unityCommunication) {
            _unityCommunication = unityCommunication;
    #if UNITY_EDITOR
            _webGLBridgeUtils = new EditorBridgeUtils();

#elif UNITY_WEBGL
            _webGLBridgeUtils = new WebGLBridgeUtils(logManager, unityCommunication);
#else
            _webGLBridgeUtils = new NullBridgeUtils();
#endif
        }

        public Task<bool> Initialize() {
            return _webGLBridgeUtils.Initialize();
        }

        public void Destroy() {
            _webGLBridgeUtils.Destroy();
        }

        public void CopyToClipboard(string text) {
            _webGLBridgeUtils.CopyToClipboard(text);
        }

        public void OpenUrl(string url) {
            _webGLBridgeUtils.OpenUrl(url);
        }

        public Task<string> GetStartTelegramParams() {
            return _webGLBridgeUtils.GetStartTelegramParams();
        }

        public Task<bool> IsIOSBrowser() {
            return _webGLBridgeUtils.IsIOSBrowser();
        }

        public Task<bool> IsAndroidBrowser() {
            return _webGLBridgeUtils.IsAndroidBrowser();
        }
    }
}