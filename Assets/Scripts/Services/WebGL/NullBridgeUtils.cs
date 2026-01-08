using System.Threading.Tasks;

namespace Services.WebGL {
    public class NullBridgeUtils : IWebGLBridgeUtils {
        public void CopyToClipboard(string text) {
        }

        public void OpenUrl(string url) {
            
        }

        public Task<string> GetStartTelegramParams() {
            return Task.FromResult("");
        }

        public Task<bool> IsIOSBrowser() {
            return Task.FromResult(false);
        }

        public Task<bool> IsAndroidBrowser() {
            return Task.FromResult(false);
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}